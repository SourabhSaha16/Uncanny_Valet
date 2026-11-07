using System;
using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Orders
{
    /// <summary>
    /// Sends the boss's orders. From the first order night on, every random gap of game minutes it tries to
    /// order one parked car into a free regular spot, while fewer than MaxOpenOrders are open. If nothing fits,
    /// it skips and tries again after the next gap.
    /// </summary>
    public sealed class OrderIssuer : IEventSubscriber
    {
        private readonly IEventBus _bus;
        private readonly ITuning _tuning;
        private readonly ILotLayout _lot;
        private readonly IChargingSpotQuery _charging;
        private readonly OrderBook _book;
        private readonly ParkedCars _parked;
        private readonly OrderSelector _selector;
        private readonly OrderScheduler _scheduler;
        private readonly int _fromNight;
        private readonly List<ParkedCar> _cars = new List<ParkedCar>();
        private readonly List<int> _freeSpots = new List<int>();

        public OrderIssuer(IEventBus bus, ITuning tuning, ILotLayout lot, IChargingSpotQuery charging,
            OrderBook book, ParkedCars parked, OrderSelector selector, OrderScheduler scheduler, int fromNight)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _tuning = tuning ?? throw new ArgumentNullException(nameof(tuning));
            _lot = lot ?? throw new ArgumentNullException(nameof(lot));
            _charging = charging ?? throw new ArgumentNullException(nameof(charging));
            _book = book ?? throw new ArgumentNullException(nameof(book));
            _parked = parked ?? throw new ArgumentNullException(nameof(parked));
            _selector = selector ?? throw new ArgumentNullException(nameof(selector));
            _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
            _fromNight = fromNight;
        }

        public void Subscribe()
        {
            _bus.Subscribe<NightStarted>(OnNightStarted);
            _bus.Subscribe<NightTick>(OnNightTick);
            _bus.Subscribe<NightEnded>(OnNightEnded);
        }

        public void Unsubscribe()
        {
            _bus.Unsubscribe<NightStarted>(OnNightStarted);
            _bus.Unsubscribe<NightTick>(OnNightTick);
            _bus.Unsubscribe<NightEnded>(OnNightEnded);
        }

        private float GapMin => _tuning.Get(TuningKey.OrderGapMinMinutes);
        private float GapMax => _tuning.Get(TuningKey.OrderGapMaxMinutes);

        private void OnNightStarted(NightStarted e)
        {
            if (e.Night >= _fromNight) _scheduler.Arm(0f, GapMin, GapMax);
            else _scheduler.Disarm();
        }

        private void OnNightEnded(NightEnded e) => _scheduler.Disarm();

        private void OnNightTick(NightTick e)
        {
            if (!_scheduler.TryConsume(e.MinuteOfNight, GapMin, GapMax)) return;
            if (_book.Count >= (int)_tuning.Get(TuningKey.MaxOpenOrders)) return;
            _cars.Clear();
            _parked.CollectEligible(_book, _cars);
            CollectFreeSpots();
            if (!_selector.TryPick(_cars, _freeSpots, out var pick)) return;
            var order = _book.Open(pick.Car.Car, pick.Car.Letter, pick.TargetSpot, e.MinuteOfNight);
            _bus.Publish(new OrderIssued(order.Id, order.Car, order.Letter, order.TargetSpot));
        }

        private void CollectFreeSpots()
        {
            _freeSpots.Clear();
            var ids = _lot.SpotIds;
            for (var i = 0; i < ids.Count; i++)
            {
                var id = ids[i];
                if (_charging.IsCharging(id) || !_lot.IsSpotFree(id) || _book.IsSpotTargeted(id)) continue;
                _freeSpots.Add(id);
            }
        }
    }
}
