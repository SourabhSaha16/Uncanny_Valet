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
        private ArrivalSlotScanner _scanner;
        private readonly ArrivalReservations _reservations = new ArrivalReservations();
        private PatronDeparture _departure;
        private float _nextArrivalMinute;
        private int _nextCarId = 1;
        private bool _nightOpen = true;
        private PatronPlan _plan;
        private PatronRequestRoller _roller;

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
            _scanner = new ArrivalSlotScanner(_arrivalClearSize);
            _plan = new PatronPlan(_config.MaxWaitingCars);
            var tuning = new PatronTuning(Services.Get<ITuning>(), _config);
            _roller = new PatronRequestRoller(_config, tuning, _plan, Random);
            Registry = new TicketRegistry(_bus, Services.Get<IGameClock>(), Services.Get<ITipCalculator>(), tuning);
            _departure = new PatronDeparture(_bus, _cars);
            _bus.Subscribe<NightStarted>(OnNightStarted);
            _bus.Subscribe<NightTick>(OnNightTick);
            _bus.Subscribe<NightEnded>(OnNightEnded);
            _bus.Subscribe<NightPlanChanged>(OnNightPlanChanged);
        }

        private void OnDisable()
        {
            if (_bus == null) return;
            _bus.Unsubscribe<NightStarted>(OnNightStarted);
            _bus.Unsubscribe<NightTick>(OnNightTick);
            _bus.Unsubscribe<NightEnded>(OnNightEnded);
            _bus.Unsubscribe<NightPlanChanged>(OnNightPlanChanged);
            Registry.Dispose();
            _departure.Dispose();
        }

        /// <summary>
        /// The single entry point for a patron arrival (story cards call this too). Fails when the waiting
        /// cap is reached or no queue slot is clear; the caller may retry later.
        /// </summary>
        public bool TrySpawn(PatronRequest request, out Ticket ticket)
        {
            ticket = null;
            if (!_nightOpen) return false;
            var scanned = _scanner.Scan(_lot.ArrivalSlots, _lot.ArrivalRotationDegrees, _plan.MaxWaitingCars);
            var blocked = _reservations.Merge(scanned, Registry);
            var slot = ArrivalGate.FirstFreeSlot(Registry.AwaitingParkCount, _plan.MaxWaitingCars, blocked);
            if (slot == ArrivalGate.NoSlot) return false;
            var car = new CarId(_nextCarId++);
            _cars.SpawnArriving(car, _lot.ArrivalSlots[slot], _lot.ArrivalRotationDegrees);
            ticket = Registry.Create(car, request);
            _reservations.Reserve(ticket.TicketId, slot);
            Registry.AnnounceArrival(ticket);
            return true;
        }

        private void OnNightStarted(NightStarted e)
        {
            _nightOpen = true;
            _nextArrivalMinute = 0f;
        }

        private void OnNightEnded(NightEnded e) => _nightOpen = false;

        private void OnNightPlanChanged(NightPlanChanged e) => _plan.Apply(e.Plan);

        private void OnNightTick(NightTick e)
        {
            if (e.MinuteOfNight < _nextArrivalMinute) return;
            if (!TrySpawn(_roller.Roll(e.MinuteOfNight), out _)) return;
            _nextArrivalMinute = e.MinuteOfNight + _roller.NextGap();
        }
    }
}
