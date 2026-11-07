using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UnityEngine;

namespace UncannyValet.Orders
{
    /// <summary>
    /// Runs the boss's orders. Add one to the Bootstrap scene and assign an <see cref="OrdersConfig"/>. Needs
    /// IEventBus, ITuning, ILotLayout and IChargingSpotQuery in Services. It publishes OrderIssued,
    /// OrderResolved and StrikeGiven; the UI shows them.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BossOrderDirector : MonoBehaviour
    {
        [SerializeField] private OrdersConfig _config;

        private IEventSubscriber[] _parts;
        private StrikeTracker _strikes;

        /// <summary>Strikes given so far this run.</summary>
        public int Strikes => _strikes?.Total ?? 0;

        private void Start()
        {
            if (_config == null)
            {
                Debug.LogError("BossOrderDirector: no OrdersConfig assigned; boss orders are off.", this);
                return;
            }
            if (!Services.TryGet<IEventBus>(out var bus) || !Services.TryGet<ITuning>(out var tuning)
                || !Services.TryGet<ILotLayout>(out var lot) || !Services.TryGet<IChargingSpotQuery>(out var charging))
            {
                Debug.LogError("BossOrderDirector: IEventBus, ITuning, ILotLayout and IChargingSpotQuery must be registered in Services; boss orders are off.", this);
                return;
            }
            Build(bus, tuning, lot, charging);
            if (isActiveAndEnabled) OnEnable();
        }

        private void OnEnable()
        {
            if (_parts == null) return;
            for (var i = 0; i < _parts.Length; i++) _parts[i].Subscribe();
        }

        private void OnDisable()
        {
            if (_parts == null) return;
            for (var i = 0; i < _parts.Length; i++) _parts[i].Unsubscribe();
        }

        private void Build(IEventBus bus, ITuning tuning, ILotLayout lot, IChargingSpotQuery charging)
        {
            var random = new UnityOrderRandom();
            var book = new OrderBook();
            var parked = new ParkedCars();
            var history = new CarVisitHistory();
            _strikes = new StrikeTracker(bus, tuning);
            var scheduler = new OrderScheduler(random);
            var selector = new OrderSelector(random);
            _parts = new IEventSubscriber[]
            {
                _strikes,
                new OrderTracker(bus, parked, history),
                new OrderResolver(bus, book, new OrderJudge(history), _strikes),
                new OrderIssuer(bus, tuning, lot, charging, book, parked, selector, scheduler, _config.FromNight),
            };
        }
    }
}
