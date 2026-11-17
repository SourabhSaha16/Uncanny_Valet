using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Director;

namespace UncannyValet.Tests.Director
{
    /// <summary>A lot without a wash slot or EV bay reports no station slots to the director, whatever the shift plan says.</summary>
    public sealed class LotStateNoWashTests
    {
        private const int EightBays = 8;

        [Test]
        public void WithoutAWashSlot_ThePlansWashFlagOpensNothing()
        {
            var bus = new EventBus();
            var tracker = new LotStateTracker(bus, new FakeClock(), new TuningStore(bus), ModifierTestKit.Catalog(), EightBays, 0, false);
            bus.Publish(new ShiftPlanChanged(new ShiftPlan(4, 1f, true, true)));
            var snapshot = tracker.Capture();
            Assert.AreEqual(0, snapshot.Slots.Count, "no wash and no charging slot");
            Assert.AreEqual(EightBays, snapshot.TotalSpots, "eight regular bays count");
            tracker.Dispose();
        }

        [Test]
        public void WithAWashSlot_ItStillShowsUp()
        {
            var bus = new EventBus();
            var tracker = new LotStateTracker(bus, new FakeClock(), new TuningStore(bus), ModifierTestKit.Catalog(), 6, 1);
            bus.Publish(new ShiftPlanChanged(new ShiftPlan(4, 1f, true, true)));
            Assert.AreEqual(2, tracker.Capture().Slots.Count, "the wash slot and the EV bay");
            tracker.Dispose();
        }
    }
}
