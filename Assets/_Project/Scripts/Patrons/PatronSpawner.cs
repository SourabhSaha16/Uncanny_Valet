using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UnityEngine;

namespace UncannyValet.Patrons
{
    /// <summary>Spawns a patron car every so often and owns the ticket registry for the night.</summary>
    public class PatronSpawner : MonoBehaviour
    {
        [SerializeField] private PatronConfig _config;
        [SerializeField] private Vector2 _arrivalClearSize = new Vector2(2.5f, 1.5f);

        private IEventBus _bus;
        private ICarFactory _cars;
        private ILotLayout _lot;
        private PatronDeparture _departure;
        private float _nextArrivalMinute;
        private int _nextCarId = 1;

        public TicketRegistry Registry { get; private set; }

        private void OnEnable()
        {
            _bus = Services.Get<IEventBus>();
            _cars = Services.Get<ICarFactory>();
            _lot = Services.Get<ILotLayout>();
            Registry = new TicketRegistry(_bus, Services.Get<IGameClock>(), Services.Get<ITipCalculator>());
            _departure = new PatronDeparture(_bus, _cars);
            _bus.Subscribe<NightStarted>(OnNightStarted);
            _bus.Subscribe<NightTick>(OnNightTick);
        }

        private void OnDisable()
        {
            _bus.Unsubscribe<NightStarted>(OnNightStarted);
            _bus.Unsubscribe<NightTick>(OnNightTick);
            Registry.Dispose();
            _departure.Dispose();
        }

        /// <summary>The single entry point for a patron arrival. Story cards call this too.</summary>
        public Ticket Spawn(PatronRequest request)
        {
            var car = new CarId(_nextCarId++);
            _cars.Spawn(car, _lot.ArrivalPoint, _lot.ArrivalRotationDegrees);
            var ticket = Registry.Open(car, request);
            _bus.Publish(new CarSpawned(car, ticket.TicketId));
            return ticket;
        }

        private void OnNightStarted(NightStarted e) => _nextArrivalMinute = 0f;

        private void OnNightTick(NightTick e)
        {
            if (e.MinuteOfNight < _nextArrivalMinute) return;
            var blocked = Physics2D.OverlapBox(_lot.ArrivalPoint, _arrivalClearSize, _lot.ArrivalRotationDegrees) != null;
            if (!ArrivalGate.CanArrive(Registry.AwaitingParkCount, _config.MaxWaitingCars, blocked)) return;
            Spawn(RollRequest());
            var gap = Random.Range(_config.ArrivalIntervalRange.x, _config.ArrivalIntervalRange.y);
            _nextArrivalMinute = e.MinuteOfNight + gap;
        }

        private PatronRequest RollRequest()
        {
            var stay = Random.Range(_config.StayRange.x, _config.StayRange.y);
            return new PatronRequest(stay, _config.BaseTip, Random.value < _config.ImpatientChance);
        }
    }
}
