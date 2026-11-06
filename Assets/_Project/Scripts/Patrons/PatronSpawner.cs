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
        private PatronDeparture _departure;
        private float _nextArrivalMinute;
        private int _nextCarId = 1;

        public TicketRegistry Registry { get; private set; }

        // Bootstrap registers services, then activates this object, so OnEnable is the first safe moment.
        private void OnEnable()
        {
            if (_config == null) { Debug.LogError("PatronSpawner has no PatronConfig assigned.", this); enabled = false; return; }
            _bus = Services.Get<IEventBus>();
            _cars = Services.Get<ICarFactory>();
            _lot = Services.Get<ILotLayout>();
            _scanner = new ArrivalSlotScanner(_arrivalClearSize);
            Registry = new TicketRegistry(_bus, Services.Get<IGameClock>(), Services.Get<ITipCalculator>());
            _departure = new PatronDeparture(_bus, _cars);
            _bus.Subscribe<NightStarted>(OnNightStarted);
            _bus.Subscribe<NightTick>(OnNightTick);
        }

        private void OnDisable()
        {
            if (_bus == null) return;
            _bus.Unsubscribe<NightStarted>(OnNightStarted);
            _bus.Unsubscribe<NightTick>(OnNightTick);
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
            var blocked = _scanner.Scan(_lot.ArrivalSlots, _lot.ArrivalRotationDegrees, _config.MaxWaitingCars);
            var slot = ArrivalGate.FirstFreeSlot(Registry.AwaitingParkCount, _config.MaxWaitingCars, blocked);
            if (slot == ArrivalGate.NoSlot) return false;
            var car = new CarId(_nextCarId++);
            _cars.Spawn(car, _lot.ArrivalSlots[slot], _lot.ArrivalRotationDegrees);
            ticket = Registry.Open(car, request);
            _bus.Publish(new CarSpawned(car, ticket.TicketId));
            _bus.Publish(new CarLetterAssigned(car, ticket.Letter));
            return true;
        }

        private void OnNightStarted(NightStarted e) => _nextArrivalMinute = 0f;

        private void OnNightTick(NightTick e)
        {
            if (e.MinuteOfNight < _nextArrivalMinute) return;
            if (!TrySpawn(RollRequest(), out _)) return;
            var gap = Random.Range(_config.ArrivalIntervalRange.x, _config.ArrivalIntervalRange.y);
            _nextArrivalMinute = e.MinuteOfNight + gap;
        }

        private PatronRequest RollRequest()
        {
            var stay = Random.Range(_config.StayRange.x, _config.StayRange.y);
            return new PatronRequest(stay, _config.BaseTip, Random.value < _config.ImpatientChance,
                _config.ParkWindowMinutes, _config.PickupWindowMinutes);
        }
    }
}
