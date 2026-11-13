using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Patrons;

namespace UncannyValet.Tests.Patrons
{
    public class ChargeSpotTests
    {
        private sealed class FakeClock : IGameClock
        {
            public float SecondsRemaining => 0f;
            public float ShiftLengthSeconds => 0f;
            public float ElapsedMinutes { get; set; }
            public bool IsRunning => true;
        }

        private sealed class FakeBlessings : IBlessingCalculator
        {
            public int Calculate(in BlessingInput input) => 1;
            public BlessingBreakdown Explain(in BlessingInput input) => new BlessingBreakdown(1, 0, 0, 1, BlessingZeroReason.None);
        }

        private EventBus _bus;
        private FakeClock _clock;
        private TicketRegistry _registry;
        private int _charges;
        private int _expired;
        private readonly CarId _car = new CarId(1);

        [SetUp]
        public void SetUp()
        {
            _bus = new EventBus(); _clock = new FakeClock();
            _registry = new TicketRegistry(_bus, _clock, new FakeBlessings(), ServiceTuning.Default, null, null, ModifierTestKit.Catalog());
            _charges = 0; _expired = 0;
            _bus.Subscribe<ModifierProgressChanged>(e => _charges++);
            _bus.Subscribe<TicketTimerExpired>(e => _expired++);
        }

        private Ticket Arrive(bool ev)
        {
            var t = _registry.Create(_car, new PatronRequest(100f, 20, false, 60f, ModifierTestKit.Of(ev, false)));
            _registry.AnnounceArrival(t);
            return t;
        }

        private void Tick(float minute)
        {
            _clock.ElapsedMinutes = minute;
            _bus.Publish(new ShiftTick(minute));
        }

        [Test]
        public void EvParkedInChargingSpot_IsParkedAndKeepsCharging_UntilItLeaves()
        {
            var t = Arrive(true);
            _bus.Publish(new CarParked(_car, 1));
            _bus.Publish(new CarEnteredStation(_car, StationKind.Charging));
            var before = _charges;
            Tick(1f); Tick(2f);
            Assert.AreEqual(TicketState.Parked, t.State);
            Assert.AreEqual(before + 2, _charges);
            _bus.Publish(new CarLeftStation(_car, StationKind.Charging));
            Tick(3f);
            Assert.AreEqual(before + 2, _charges);
        }

        [Test]
        public void NonEvParkedInChargingSpot_NothingCharges()
        {
            var t = Arrive(false);
            _bus.Publish(new CarEnteredStation(_car, StationKind.Charging));
            _bus.Publish(new CarParked(_car, 1));
            Tick(60f);
            Assert.AreEqual(TicketState.Parked, t.State);
            Assert.AreEqual(0, _charges);
            Assert.AreEqual(0, _expired);
        }

        [Test]
        public void UnparkedEvInChargingStation_NeverExpiresAnything()
        {
            Arrive(true);
            _bus.Publish(new CarEnteredStation(_car, StationKind.Charging));
            Tick(200f);
            Assert.AreEqual(0, _expired);
        }
    }
}
