using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Core.Modifiers;
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
        private QueueAdvancer _queue;
        private PatronTuning _tuning;
        private PatronDeparture _departure;
        private float _nextArrivalMinute;
        private int _nextCarId = 1;
        private bool _shiftOpen = true;
        private PatronPlan _plan;
        private PatronRequestRoller _roller;
        private IGameClock _clock;
        private ITuning _liveTuning;
        private IModifierCatalog _catalog;
        private PatronRequest _pending;
        private bool _hasPending;
        private IArrivalPacing _pacing = new DirectorArrivalPacing();
        private MinQueueTopUp _minQueue;

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
            _catalog = Services.Get<IModifierCatalog>();
            var tuning = _tuning = new PatronTuning(_liveTuning, _config);
            Services.TryGet<ILotSnapshotSource>(out var lotView);
            _roller = new PatronRequestRoller(_config, tuning, _plan, Random, _catalog, lotView);
            _clock = Services.Get<IGameClock>();
            _minQueue = new MinQueueTopUp(_liveTuning, _config.TopUpSpacingMinutes);
            Services.TryGet<IShiftBlessingsView>(out var score);
            Registry = new TicketRegistry(_bus, _clock, Services.Get<IBlessingCalculator>(), tuning, score, _catalog);
            Registry.CurbWalkTimeoutMinutes = _config.CurbWalkTimeoutMinutes;
            Services.Register<IPickupAcceptance>(new PickupAcceptance(Registry));
            var capacity = Mathf.Min(_config.Pickup.MaxQueuedCars, _lot.ArrivalSlots.Count);
            _slots = new QueueSlotAllocator(_bus, Registry, new ArrivalSlotScanner(_arrivalClearSize, _lot), capacity);
            Services.Register<IQueueSlotSource>(_slots);
            _queue = new QueueAdvancer(_bus, _cars, _lot, new ArrivalSlotScanner(_config.Pickup.AdvanceClearSize, _lot), _config.Pickup);
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
            _queue.Dispose();
            _departure.Dispose();
        }

        private void Update()
        {
            if (_queue == null) return;
            if (_queue.Focus == null && Services.TryGet<IPlayerFocus>(out var focus)) _queue.Focus = focus;
            _queue.Tick();
            if (_shiftOpen && !_pacing.TopsUpQueue) KeepMinimumQueued();
        }

        // Lots with a queue minimum (Vanaheim training, issue 178) send a car whenever fewer cars wait than the minimum.
        private void KeepMinimumQueued()
        {
            var now = _clock.ElapsedMinutes;
            if (!_minQueue.Due(Registry.AwaitingParkCount, now)) return;
            if (TrySpawn(_roller.Roll(ShiftProgress()), out _)) _minQueue.Sent(now);
        }

        /// <summary>
        /// The single entry point for a patron arrival (story cards call this too). Fails when the queue is
        /// full (every position holds a car); the caller may retry later.
        /// </summary>
        public bool TrySpawn(PatronRequest request, out Ticket ticket)
        {
            ticket = null;
            if (!_shiftOpen) return false;
            var slot = _slots.ChooseForSpawn();
            if (slot == ArrivalGate.NoSlot) return false;
            var car = new CarId(_nextCarId++);
            _cars.SpawnArriving(car, _lot.ArrivalSlots[slot], _lot.ArrivalRotationDegrees, slot);
            ticket = Registry.Create(car, ArrivalStay.Fit(request, car, _tuning, _clock.SecondsRemaining));
            _slots.Reserve(car, slot);
            Registry.AnnounceArrival(ticket);
            return true;
        }

        private void OnShiftStarted(ShiftStarted e)
        {
            _shiftOpen = true;
            var kind = Services.TryGet<IRunProgress>(out var run) && run.HasCurrentLot ? run.CurrentLot.Kind : LotKind.None;
            _pacing = ArrivalPacings.For(kind, _config.TopUpSpacingMinutes);
            _pacing.Reset();
            _minQueue.Reset();
            _roller.ResetShift();
            _nextArrivalMinute = 0f;
            _hasPending = false;
            // The first car drives in at once (issue 179); the director path sends it from its first tick.
            if (_pacing.TopsUpQueue) TopUpQueue(0f);
            else if (!DirectorOn) SpawnOnTimer(0f);
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
            if (_pacing.TopsUpQueue || !DirectorOn || !_shiftOpen) return;
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

        // Crazy lots: every free queue slot is filled, one car per pacing spacing, whatever the director says.
        private void TopUpQueue(float elapsedMinutes)
        {
            while (_shiftOpen && _pacing.Due(elapsedMinutes) && _slots.ChooseForSpawn() != ArrivalGate.NoSlot)
            {
                if (!TrySpawn(_roller.Roll(ShiftProgress()), out _)) return;
                _pacing.Spawned(elapsedMinutes);
            }
        }

        private void OnShiftTick(ShiftTick e)
        {
            if (_pacing.TopsUpQueue) { TopUpQueue(e.ElapsedMinutes); return; }
            if (DirectorOn) { TrySpawnPending(); return; }
            _hasPending = false;
            SpawnOnTimer(e.ElapsedMinutes);
        }

        // The fixed timer: one car, then the next after a rolled gap.
        private void SpawnOnTimer(float elapsedMinutes)
        {
            if (elapsedMinutes < _nextArrivalMinute) return;
            if (!TrySpawn(_roller.Roll(ShiftProgress()), out _)) return;
            _nextArrivalMinute = elapsedMinutes + _roller.NextGap();
        }
    }
}
