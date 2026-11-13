using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Core.Modifiers;
using UncannyValet.Patrons;

namespace UncannyValet.Tests.Patrons
{
    public class TicketRegistryServiceTests
    {
        private sealed class FakeClock : IGameClock
        {
            public float SecondsRemaining => 0f;
            public float ShiftLengthSeconds => 0f;
            public float ElapsedMinutes { get; set; }
            public bool IsRunning => true;
        }

        private sealed class FakeTips : ITipCalculator
        {
            public TipInput Last;
            public int Calculate(in TipInput input) => 40;
            public TipBreakdown Explain(in TipInput input)
            {
                Last = input;
                return new TipBreakdown(20, 5, 15, 40, TipZeroReason.None);
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
            _registry = new TicketRegistry(_bus, _clock, _tips, ServiceTuning.Default, null, null, ModifierTestKit.Catalog());
            _log = new List<IGameEvent>();
            _bus.Subscribe<CarSpawned>(e => _log.Add(e));
            _bus.Subscribe<TicketOpened>(e => _log.Add(e));
            _bus.Subscribe<CarTagAssigned>(e => _log.Add(e));
            _bus.Subscribe<TicketModifiersAssigned>(e => _log.Add(e));
            _bus.Subscribe<ModifierProgressChanged>(e => _log.Add(e));
            _bus.Subscribe<ModifierCleared>(e => _log.Add(e));
            _bus.Subscribe<TipExplained>(e => _log.Add(e));
            _bus.Subscribe<TicketClosed>(e => _log.Add(e));
            _bus.Subscribe<TicketTimerExpired>(e => _log.Add(e));
        }

        private Ticket Arrive(bool ev, bool muddy) => Arrive(ModifierTestKit.Of(ev, muddy));

        private Ticket Arrive(ModifierSet modifiers)
        {
            var t = _registry.Create(_car, new PatronRequest(10f, 20, false, 60f, modifiers));
            _registry.AnnounceArrival(t);
            _bus.Publish(new CarClearedQueue(_car));
            return t;
        }

        private void Tick(float minute)
        {
            _clock.ElapsedMinutes = minute;
            _bus.Publish(new ShiftTick(minute));
        }

        private int Count<T>() where T : IGameEvent
        {
            var n = 0;
            foreach (var e in _log) if (e is T) n++;
            return n;
        }

        private int Progress(ModifierId id)
        {
            var n = 0;
            foreach (var e in _log) if (e is ModifierProgressChanged p && p.Modifier == id) n++;
            return n;
        }

        private int Cleared(ModifierId id)
        {
            var n = 0;
            foreach (var e in _log) if (e is ModifierCleared c && c.Modifier == id) n++;
            return n;
        }

        [Test]
        public void Arrival_PublishesInRequiredOrder_ProgressAfterModifiers()
        {
            Arrive(true, false);
            Assert.IsInstanceOf<CarSpawned>(_log[0]);
            Assert.IsInstanceOf<TicketOpened>(_log[1]);
            Assert.IsInstanceOf<CarTagAssigned>(_log[2]);
            Assert.IsInstanceOf<TicketModifiersAssigned>(_log[3]);
            Assert.IsInstanceOf<ModifierProgressChanged>(_log[4]);
            Assert.AreEqual(0.2f, ((ModifierProgressChanged)_log[4]).Progress01, 1e-4f);
            Assert.AreEqual(5, _log.Count);
        }

        [Test]
        public void Arrival_MuddyCarAnnouncesZeroProgressAfterModifiers()
        {
            Arrive(false, true);
            Assert.IsInstanceOf<TicketModifiersAssigned>(_log[3]);
            Assert.IsTrue(((TicketModifiersAssigned)_log[3]).Modifiers.Contains(ModifierId.Muddy));
            Assert.AreEqual(ModifierId.Muddy, ((ModifierProgressChanged)_log[4]).Modifier);
            Assert.AreEqual(0f, ((ModifierProgressChanged)_log[4]).Progress01);
        }

        [Test]
        public void Charging_OnlyWhileInBay_ClearsNeedsChargeOnce()
        {
            Arrive(true, false);
            Tick(1f);
            Assert.AreEqual(1, Progress(ModifierId.NeedsCharge));
            _bus.Publish(new CarEnteredStation(_car, StationKind.Charging));
            for (var m = 2; m < 40; m++) Tick(m);
            Assert.AreEqual(1, Cleared(ModifierId.NeedsCharge));
            _bus.Publish(new CarLeftStation(_car, StationKind.Charging));
            var before = Progress(ModifierId.NeedsCharge);
            Tick(50f);
            Assert.AreEqual(before, Progress(ModifierId.NeedsCharge));
        }

        [Test]
        public void PlainCarInChargingBay_DoesNothing()
        {
            Arrive(false, false);
            _bus.Publish(new CarEnteredStation(_car, StationKind.Charging));
            Tick(1f);
            Assert.AreEqual(0, Count<ModifierProgressChanged>());
        }

        [Test]
        public void MuddyCarInChargingBay_DoesNotWash()
        {
            Arrive(false, true);
            _bus.Publish(new CarEnteredStation(_car, StationKind.Charging));
            for (var m = 1; m <= 20; m++) Tick(m);
            Assert.AreEqual(0, Cleared(ModifierId.Muddy));
            Assert.AreEqual(1, Count<ModifierProgressChanged>(), "only the arrival announcement");
        }

        [Test]
        public void Wash_ClearsMuddyOnceAtStation()
        {
            Arrive(false, true);
            _bus.Publish(new CarEnteredStation(_car, StationKind.Wash));
            for (var m = 1; m <= 20; m++) Tick(m);
            Assert.AreEqual(1, Cleared(ModifierId.Muddy));
            Assert.AreEqual(1 + 15, Progress(ModifierId.Muddy));
        }

        [Test]
        public void CarHoldingTwoModifiers_IsTrackedClearedAndReportedPerModifier()
        {
            var t = Arrive(true, true);
            Assert.AreEqual(2, t.Modifiers.Count);
            Assert.AreEqual(2, t.OpenModifiers.Count);
            _bus.Publish(new CarEnteredStation(_car, StationKind.Wash));
            for (var m = 1; m <= 16; m++) Tick(m);
            Assert.AreEqual(1, Cleared(ModifierId.Muddy));
            Assert.AreEqual(0, Cleared(ModifierId.NeedsCharge), "the wash does not charge");
            Assert.AreEqual(ModifierSet.Of(ModifierId.Muddy), t.ClearedModifiers);
            Assert.AreEqual(ModifierSet.Of(ModifierId.NeedsCharge), t.OpenModifiers);
            _bus.Publish(new CarLeftStation(_car, StationKind.Wash));
            _bus.Publish(new CarEnteredStation(_car, StationKind.Charging));
            for (var m = 17; m <= 40; m++) Tick(m);
            Assert.AreEqual(1, Cleared(ModifierId.NeedsCharge));
            Assert.IsTrue(t.OpenModifiers.IsEmpty);
            _bus.Publish(new CarParked(_car, 1));
            Tick(50f);
            _bus.Publish(new CarAtCurb(_car));
            Assert.AreEqual(ModifierSet.Of(ModifierId.Muddy, ModifierId.NeedsCharge), _tips.Last.ClearedModifiers);
            Assert.IsTrue(_tips.Last.OpenModifiers.IsEmpty);
        }

        [Test]
        public void CarInWashBay_StillGetsItsPickupRequest_WhenTheStayEnds()
        {
            var t = Arrive(false, false);
            Tick(5f);
            _bus.Publish(new CarEnteredStation(_car, StationKind.Wash));
            Tick(10f);
            Assert.AreEqual(TicketState.PickupRequested, t.State);
            Assert.AreEqual(0, Count<TicketTimerExpired>());
        }

        [Test]
        public void Delivery_ReportsClearedModifiers_AndExplainsBeforeClosing()
        {
            var t = Arrive(true, false);
            _bus.Publish(new CarEnteredStation(_car, StationKind.Charging));
            for (var m = 1; m <= 30; m++) Tick(m);
            _bus.Publish(new CarLeftStation(_car, StationKind.Charging));
            _bus.Publish(new CarParked(_car, 1));
            Tick(40f);
            _bus.Publish(new CarAtCurb(_car));

            Assert.AreEqual(ModifierSet.Of(ModifierId.NeedsCharge), _tips.Last.ClearedModifiers);
            Assert.IsTrue(_tips.Last.OpenModifiers.IsEmpty);
            var explained = _log.FindIndex(e => e is TipExplained);
            Assert.IsInstanceOf<TicketClosed>(_log[explained + 1]);
            Assert.AreEqual(40, ((TicketClosed)_log[explained + 1]).Tip);
            Assert.AreEqual(TicketState.Closed, t.State);
        }

        [Test]
        public void Delivery_UnwashedMuddyCarReportsTheOpenModifier()
        {
            Arrive(false, true);
            _bus.Publish(new CarParked(_car, 1));
            Tick(10f);
            _bus.Publish(new CarAtCurb(_car));
            Assert.AreEqual(ModifierSet.Of(ModifierId.Muddy), _tips.Last.OpenModifiers);
            Assert.IsTrue(_tips.Last.ClearedModifiers.IsEmpty);
        }

        [Test]
        public void NightEnd_ExplainsAllZeroWithReasonNone()
        {
            Arrive(false, false);
            _bus.Publish(new ShiftEnded());
            var e = (TipExplained)_log.Find(x => x is TipExplained);
            Assert.AreEqual(0, e.Breakdown.Total);
            Assert.AreEqual(TipZeroReason.None, e.Breakdown.ZeroReason);
        }
    }
}
