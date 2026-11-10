using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UnityEngine;

namespace UncannyValet.Patrons
{
    /// <summary>Spawns a patron car every so often into the arrival queue and owns the ticket registry.</summary>
    [DisallowMultipleComponent]
    public sealed class PatronSpawner : MonoBehaviour
    {
        [SerializeField] private PatronConfig _config;
        [Tooltip("Size of the box that must be free of cars for an arrival slot to be usable.")]
        [SerializeField] private Vector2 _arrivalClearSize = new Vector2(2.5f, 1.5f);

        private IEventBus _bus;
        private ICarFactory _cars;
        private ILotLayout _lot;
        private QueueSlotAllocator _slots;
        private PatronTuning _tuning;
        private PatronDeparture _departure;
        private float _nextArrivalMinute;
        private int _nextCarId = 1;
        private bool _shiftOpen = true;
        private PatronPlan _plan;
        private PatronRequestRoller _roller;
        private IGameClock _clock;
        private ITuning _liveTuning;
        private PatronRequest _pending;
        private bool _hasPending;

        public TicketRegistry Registry { get; private set; }

        /// <summary>Swappable so tests can script the rolls; defaults to UnityEngine.Random.</summary>
        public IPatronRandom Random { get; set; } = new UnityPatronRandom();

        // Bootstrap registers services, then activates this object, so OnEnable is the first safe moment.
        private void OnEnable()
        {
            if (_config == null) { Debug.LogError("PatronSpawner has no PatronConfig assigned.", this); enabled = false; return; }
            _bus = Services.Get<IEventBus>();
            _cars = Services.Get<ICarFactory>();
            _lot = Services.Get<ILotLayout>();
            _plan = new PatronPlan(_config.MaxWaitingCars);
            _liveTuning = Services.Get<ITuning>();
            var tuning = _tuning = new PatronTuning(_liveTuning, _config);
            _roller = new PatronRequestRoller(_config, tuning, _plan, Random);
            _clock = Services.Get<IGameClock>();
            Services.TryGet<IShiftScoreView>(out var score);
            Registry = new TicketRegistry(_bus, _clock, Services.Get<ITipCalculator>(), tuning, score);
            Registry.CurbWalkTimeoutMinutes = _config.CurbWalkTimeoutMinutes;
            _slots = new QueueSlotAllocator(_bus, Registry, new ArrivalSlotScanner(_arrivalClearSize, _lot), _plan);
            Services.Register<IQueueSlotSource>(_slots);
            _departure = new PatronDeparture(_bus, _cars, _clock, _config.BoardingTimeoutMinutes);
            _bus.Subscribe<ShiftStarted>(OnShiftStarted);
            _bus.Subscribe<ShiftTick>(OnShiftTick);
            _bus.Subscribe<ShiftEnded>(OnShiftEnded);
            _bus.Subscribe<ShiftPlanChanged>(OnShiftPlanChanged);
            _bus.Subscribe<PatronScheduled>(OnPatronScheduled);
        }

        private void OnDisable()
        {
            if (_bus == null) return;
            _bus.Unsubscribe<ShiftStarted>(OnShiftStarted);
            _bus.Unsubscribe<ShiftTick>(OnShiftTick);
            _bus.Unsubscribe<ShiftEnded>(OnShiftEnded);
            _bus.Unsubscribe<ShiftPlanChanged>(OnShiftPlanChanged);
            _bus.Unsubscribe<PatronScheduled>(OnPatronScheduled);
            Registry.Dispose();
            _slots.Dispose();
            _departure.Dispose();
        }

        /// <summary>
        /// The single entry point for a patron arrival (story cards call this too). Fails when the waiting
        /// cap is reached or no queue slot is clear; the caller may retry later.
        /// </summary>
        public bool TrySpawn(PatronRequest request, out Ticket ticket)
        {
            ticket = null;
            if (!_shiftOpen) return false;
            var slot = _slots.ChooseForSpawn();
            if (slot == ArrivalGate.NoSlot) return false;
            var car = new CarId(_nextCarId++);
            _cars.SpawnArriving(car, _lot.ArrivalSlots[slot], _lot.ArrivalRotationDegrees, slot);
            ticket = Registry.Create(car, FitStay(request));
            _slots.Reserve(car, slot);
            Registry.AnnounceArrival(ticket);
            return true;
        }

        // A late arrival's stay is cut so its patron is back asking for the car before the shift ends.
        private PatronRequest FitStay(PatronRequest request)
        {
            var remaining = _clock.SecondsRemaining * _tuning.MinutesPerSecond;
            var stay = StayFit.Clamp(request.StayMinutes, remaining, _tuning.LateStayLead, _tuning.LateStayFloor);
            return stay < request.StayMinutes ? request.WithStay(stay) : request;
        }

        private void OnShiftStarted(ShiftStarted e)
        {
            _shiftOpen = true;
            _nextArrivalMinute = 0f;
            _hasPending = false;
        }

        private void OnShiftEnded(ShiftEnded e)
        {
            _shiftOpen = false;
            _hasPending = false;
        }

        // The Patron Director's switch (an ITuning key, so the F2 panel can flip it): on, arrivals come from
        // PatronScheduled; off, the old fixed timer below runs.
        private bool DirectorOn => _liveTuning.TryGet(TuningKey.DirEnabled, out var on) && on >= 0.5f;

        // Rolls the request once when the director asks, then keeps it until a queue slot is clear.
        private void OnPatronScheduled(PatronScheduled e)
        {
            if (!DirectorOn || !_shiftOpen) return;
            var progress = ShiftProgress();
            _pending = e.ArchetypeChosen ? _roller.Roll(progress, e.Archetype) : _roller.Roll(progress);
            _hasPending = true;
            TrySpawnPending();
        }

        private void TrySpawnPending()
        {
            if (_hasPending && TrySpawn(_pending, out _)) _hasPending = false;
        }

        private void OnShiftPlanChanged(ShiftPlanChanged e) => _plan.Apply(e.Plan);

        // How far into the shift we are, 0..1, from the clock's real seconds.
        private float ShiftProgress()
        {
            var length = _clock.ShiftLengthSeconds;
            return length > 0f ? Mathf.Clamp01(1f - _clock.SecondsRemaining / length) : 0f;
        }

        private void OnShiftTick(ShiftTick e)
        {
            if (DirectorOn) { TrySpawnPending(); return; }
            _hasPending = false;
            if (e.ElapsedMinutes < _nextArrivalMinute) return;
            if (!TrySpawn(_roller.Roll(ShiftProgress()), out _)) return;
            _nextArrivalMinute = e.ElapsedMinutes + _roller.NextGap();
        }
    }
}
