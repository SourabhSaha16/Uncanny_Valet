using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Director;

namespace UncannyValet.Tests.Director
{
    /// <summary>A lot with several wash troughs reports one wash slot per trough to the director, and cars in different troughs hold different slots.</summary>
    public sealed class LotStateTroughTests
    {
        private const int EightBays = 8;
        private EventBus _bus;
        private LotStateTracker _tracker;

        [SetUp]
        public void SetUp()
        {
            _bus = new EventBus();
            _tracker = new LotStateTracker(_bus, new FakeClock(), new TuningStore(_bus), ModifierTestKit.Catalog(), EightBays, 0, true, 3);
            _bus.Publish(new ShiftPlanChanged(new ShiftPlan(4, 1f, true, true)));
        }

        [TearDown] public void TearDown() => _tracker.Dispose();

        private int WashSlots()
        {
            var count = 0;
            var slots = _tracker.Capture().Slots;
            for (var i = 0; i < slots.Count; i++) if (slots[i].Kind == StationKind.Wash) count++;
            return count;
        }

        [Test]
        public void ThreeTroughs_AreThreeWashSlots_AndNoChargingSlot() => Assert.AreEqual(3, _tracker.Capture().Slots.Count);

        [Test]
        public void OneTrough_IsOneWashSlot()
        {
            _tracker.Dispose();
            _tracker = new LotStateTracker(_bus, new FakeClock(), new TuningStore(_bus), ModifierTestKit.Catalog(), EightBays, 0, true, 1);
            _bus.Publish(new ShiftPlanChanged(new ShiftPlan(4, 1f, true, true)));
            Assert.AreEqual(1, WashSlots());
        }

        [Test]
        public void TwoMuddyCars_InTwoTroughs_HoldTwoSlots()
        {
            foreach (var n in new[] { 1, 2 })
            {
                var car = new CarId(n);
                var ticket = new TicketId(n);
                _bus.Publish(new CarSpawned(car, ticket));
                _bus.Publish(new TicketOpened(ticket, car, 30f));
                _bus.Publish(new TicketModifiersAssigned(ticket, car, ModifierTestKit.Of(false, true)));
                _bus.Publish(new CarArrived(car));
                _bus.Publish(new CarEnteredStation(car, StationKind.Wash));
            }
            var working = 0;
            var slots = _tracker.Capture().Slots;
            for (var i = 0; i < slots.Count; i++) if (slots[i].State == StationSlotState.Working) working++;
            Assert.AreEqual(2, working);
            Assert.AreEqual(3, slots.Count);
        }

        [Test]
        public void WithoutAWash_TheTroughCountIsIgnored()
        {
            _tracker.Dispose();
            _tracker = new LotStateTracker(_bus, new FakeClock(), new TuningStore(_bus), ModifierTestKit.Catalog(), EightBays, 0, false, 3);
            _bus.Publish(new ShiftPlanChanged(new ShiftPlan(4, 1f, true, true)));
            Assert.AreEqual(0, _tracker.Capture().Slots.Count);
        }
    }
}
