using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Patrons;

namespace UncannyValet.Tests.Patrons
{
    public class TicketRegistryServiceTests
    {
        private sealed class FakeClock : IGameClock
        {
            public int Night => 1;
            public float MinuteOfNight { get; set; }
            public bool IsRunning => true;
        }

        private sealed class FakeTips : ITipCalculator
        {
            public TipInput Last;
            public int Calculate(in TipInput input) => 40;
            public TipBreakdown Explain(in TipInput input)
            {
                Last = input;
                return new TipBreakdown(20, 5, 10, 5, 40, TipZeroReason.None);
            }
        }

        private EventBus _bus;
        private FakeClock _clock;
        private FakeTips _tips;
        private TicketRegistry _registry;
        private List<IGameEvent> _log;
        private readonly CarId _car = new CarId(1);

        [SetUp]
        public void SetUp()
        {
            _bus = new EventBus(); _clock = new FakeClock(); _tips = new FakeTips();
            _registry = new TicketRegistry(_bus, _clock, _tips, new ServiceTuning(0.2f, 20f, 15f));
            _log = new List<IGameEvent>();
            _bus.Subscribe<CarSpawned>(e => _log.Add(e));
            _bus.Subscribe<TicketOpened>(e => _log.Add(e));
            _bus.Subscribe<CarLetterAssigned>(e => _log.Add(e));
            _bus.Subscribe<TicketTraitsAssigned>(e => _log.Add(e));
            _bus.Subscribe<CarChargeChanged>(e => _log.Add(e));
            _bus.Subscribe<CarCleanChanged>(e => _log.Add(e));
            _bus.Subscribe<CarFullyCharged>(e => _log.Add(e));
            _bus.Subscribe<CarCleaned>(e => _log.Add(e));
            _bus.Subscribe<TipExplained>(e => _log.Add(e));
            _bus.Subscribe<TicketClosed>(e => _log.Add(e));
            _bus.Subscribe<TicketTimerExpired>(e => _log.Add(e));
        }

        private Ticket Arrive(bool ev, bool dirty)
        {
            var t = _registry.Create(_car, new PatronRequest(10f, 20, false, 45f, 60f, ev, dirty));
            _registry.AnnounceArrival(t);
            return t;
        }

        private void Tick(float minute)
        {
            _clock.MinuteOfNight = minute;
            _bus.Publish(new NightTick(1, minute));
        }

        private int Count<T>() where T : IGameEvent
        {
            var n = 0;
            foreach (var e in _log) if (e is T) n++;
            return n;
        }

        [Test]
        public void Arrival_PublishesInRequiredOrder_ChargeAndCleanAfterTraits()
        {
            Arrive(true, false);
            Assert.IsInstanceOf<CarSpawned>(_log[0]);
            Assert.IsInstanceOf<TicketOpened>(_log[1]);
            Assert.IsInstanceOf<CarLetterAssigned>(_log[2]);
            Assert.IsInstanceOf<TicketTraitsAssigned>(_log[3]);
            Assert.IsInstanceOf<CarChargeChanged>(_log[4]);
            Assert.AreEqual(0.2f, ((CarChargeChanged)_log[4]).Charge01, 1e-4f);
            Assert.AreEqual(5, _log.Count);
        }

        [Test]
        public void Arrival_DirtyCarAnnouncesCleanZeroAfterTraits()
        {
            Arrive(false, true);
            Assert.IsInstanceOf<TicketTraitsAssigned>(_log[3]);
            Assert.IsTrue(((TicketTraitsAssigned)_log[3]).IsDirty);
            Assert.AreEqual(0f, ((CarCleanChanged)_log[4]).Clean01);
        }

        [Test]
        public void Charging_OnlyWhileInBay_PublishesFullyChargedOnce()
        {
            Arrive(true, false);
            Tick(1f);
            Assert.AreEqual(1, Count<CarChargeChanged>());
            _bus.Publish(new CarEnteredStation(_car, StationKind.Charging));
            for (var m = 2; m < 40; m++) Tick(m);
            Assert.AreEqual(1, Count<CarFullyCharged>());
            _bus.Publish(new CarLeftStation(_car, StationKind.Charging));
            var before = Count<CarChargeChanged>();
            Tick(50f);
            Assert.AreEqual(before, Count<CarChargeChanged>());
        }

        [Test]
        public void NonEvInChargingBay_DoesNothing()
        {
            Arrive(false, false);
            _bus.Publish(new CarEnteredStation(_car, StationKind.Charging));
            Tick(1f);
            Assert.AreEqual(0, Count<CarChargeChanged>());
        }

        [Test]
        public void Wash_CompletesOnceAtStation()
        {
            Arrive(false, true);
            _bus.Publish(new CarEnteredStation(_car, StationKind.Wash));
            for (var m = 1; m <= 20; m++) Tick(m);
            Assert.AreEqual(1, Count<CarCleaned>());
            Assert.AreEqual(1 + 15, Count<CarCleanChanged>());
        }

        [Test]
        public void ParkTimer_PausesWhileInWashBay()
        {
            Arrive(false, false);
            Tick(10f);
            _bus.Publish(new CarEnteredStation(_car, StationKind.Wash));
            Tick(60f);
            _bus.Publish(new CarLeftStation(_car, StationKind.Wash));
            Tick(80f);
            Assert.AreEqual(0, Count<TicketTimerExpired>());
            Tick(100f);
            Assert.AreEqual(1, Count<TicketTimerExpired>());
        }

        [Test]
        public void Delivery_SetsTipFlags_AndExplainsBeforeClosing()
        {
            var t = Arrive(true, false);
            _bus.Publish(new CarEnteredStation(_car, StationKind.Charging));
            for (var m = 1; m <= 30; m++) Tick(m);
            _bus.Publish(new CarLeftStation(_car, StationKind.Charging));
            _bus.Publish(new CarParked(_car, 1));
            Tick(40f);
            _bus.Publish(new CarAtCurb(_car));

            Assert.IsTrue(_tips.Last.ChargedEv);
            Assert.IsFalse(_tips.Last.CleanedDirty);
            Assert.IsFalse(_tips.Last.UncleanedDirty);
            var explained = _log.FindIndex(e => e is TipExplained);
            Assert.IsInstanceOf<TicketClosed>(_log[explained + 1]);
            Assert.AreEqual(40, ((TicketClosed)_log[explained + 1]).Tip);
            Assert.AreEqual(TicketState.Closed, t.State);
        }

        [Test]
        public void Delivery_UnwashedDirtyCarSetsUncleanedFlag()
        {
            Arrive(false, true);
            _bus.Publish(new CarParked(_car, 1));
            Tick(10f);
            _bus.Publish(new CarAtCurb(_car));
            Assert.IsTrue(_tips.Last.UncleanedDirty);
            Assert.IsFalse(_tips.Last.CleanedDirty);
        }

        [Test]
        public void NightEnd_ExplainsAllZeroWithReasonNone()
        {
            Arrive(false, false);
            _bus.Publish(new NightEnded(1));
            var e = (TipExplained)_log.Find(x => x is TipExplained);
            Assert.AreEqual(0, e.Breakdown.Total);
            Assert.AreEqual(TipZeroReason.None, e.Breakdown.ZeroReason);
        }
    }
}
