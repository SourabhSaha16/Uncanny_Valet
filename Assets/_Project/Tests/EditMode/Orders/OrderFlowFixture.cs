using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Orders;
using UnityEngine;

namespace UncannyValet.Tests.Orders
{
    /// <summary>The Orders parts wired to a real EventBus the way BossOrderDirector wires them.</summary>
    public abstract class OrderFlowFixture
    {
        protected sealed class FakeLot : ILotLayout
        {
            public readonly HashSet<int> Taken = new HashSet<int>();
            public int[] Spots = { 1, 2, 3, 9 };
            public int ChargingSpot = 9;
            public Vector2 ArrivalPoint => Vector2.zero;
            public float ArrivalRotationDegrees => 0f;
            public IReadOnlyList<Vector2> ArrivalSlots => new[] { Vector2.zero };
            public Vector2 ValetStartPoint => Vector2.zero;
            public IReadOnlyList<int> SpotIds => Spots;
            public bool IsSpotFree(int spotId) => !Taken.Contains(spotId);
            public bool IsChargingSpot(int spotId) => spotId == ChargingSpot;
        }

        protected EventBus Bus;
        protected FakeLot Lot;
        protected ScriptedOrderRandom Rng;
        protected StrikeTracker Strikes;
        protected readonly List<OrderIssued> Issued = new List<OrderIssued>();
        protected readonly List<OrderResolved> Resolved = new List<OrderResolved>();
        protected readonly List<StrikeGiven> Struck = new List<StrikeGiven>();
        protected readonly List<IGameEvent> Sequence = new List<IGameEvent>();
        private IEventSubscriber[] _parts;
        private int _night;
        private int _nextTicket = 1;

        [SetUp]
        public void BuildFixture()
        {
            Bus = new EventBus();
            Lot = new FakeLot();
            Rng = new ScriptedOrderRandom();
            Issued.Clear(); Resolved.Clear(); Struck.Clear(); Sequence.Clear();
            var tuning = new TuningStore(Bus);
            tuning.Seed(TuningKey.OrderGapMinMinutes, 10f);
            tuning.Seed(TuningKey.OrderGapMaxMinutes, 10f);
            tuning.Seed(TuningKey.MaxOpenOrders, 2f);
            tuning.Seed(TuningKey.MaxStrikes, 3f);
            var book = new OrderBook();
            var parked = new ParkedCars();
            var history = new CarVisitHistory();
            Strikes = new StrikeTracker(Bus, tuning);
            _parts = new IEventSubscriber[]
            {
                Strikes,
                new OrderTracker(Bus, parked, history),
                new OrderResolver(Bus, book, new OrderJudge(history), Strikes),
                new OrderIssuer(Bus, tuning, Lot, book, parked, new OrderSelector(Rng),
                    new OrderScheduler(Rng), 2),
            };
            foreach (var part in _parts) part.Subscribe();
            Bus.Subscribe<OrderIssued>(e => { Issued.Add(e); Sequence.Add(e); });
            Bus.Subscribe<OrderResolved>(e => { Resolved.Add(e); Sequence.Add(e); });
            Bus.Subscribe<StrikeGiven>(e => { Struck.Add(e); Sequence.Add(e); });
        }

        protected void StartNight(int night)
        {
            _night = night;
            Bus.Publish(new NightStarted(night));
        }

        protected void Tick(float minute) => Bus.Publish(new NightTick(_night, minute));

        protected void EndNight() => Bus.Publish(new NightEnded(_night));

        protected void Show(CarId car, char letter) => Bus.Publish(new CarLetterAssigned(car, letter));

        protected void Park(CarId car, int spot)
        {
            Lot.Taken.Add(spot);
            Bus.Publish(new CarParked(car, spot));
        }

        protected void Leave(CarId car, int spot)
        {
            Lot.Taken.Remove(spot);
            Bus.Publish(new CarLeftSpot(car, spot));
        }

        protected void Deliver(CarId car)
        {
            var ticket = new TicketId(_nextTicket++);
            Bus.Publish(new CarDelivered(ticket, car));
            Bus.Publish(new TicketClosed(ticket, car, 0f, 0));
        }

        protected void CloseWithoutDelivery(CarId car)
        {
            Bus.Publish(new TicketClosed(new TicketId(_nextTicket++), car, 0f, 0));
        }
    }
}
