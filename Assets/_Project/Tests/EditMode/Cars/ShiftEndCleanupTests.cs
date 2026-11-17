using NUnit.Framework;
using UncannyValet.Cars;
using UncannyValet.Core.Events;

namespace UncannyValet.Tests.Cars
{
    public class ShiftEndCleanupTests
    {
        [Test]
        public void ShiftEnded_RunsTheDespawnOncePerShift_AndDisposeUnsubscribes()
        {
            var bus = new EventBus();
            var calls = 0;
            var cleanup = new ShiftEndCleanup(bus, () => calls++);

            bus.Publish(new ShiftStarted());
            Assert.AreEqual(0, calls, "only ShiftEnded triggers it");
            bus.Publish(new ShiftEnded());
            Assert.AreEqual(1, calls);

            cleanup.Dispose();
            bus.Publish(new ShiftEnded());
            Assert.AreEqual(1, calls, "no subscription left behind");
        }
    }
}
