using NUnit.Framework;
using UncannyValet.Lot;

namespace UncannyValet.Tests.Lot
{
    public class StopTimerTests
    {
        [Test]
        public void Fires_AfterRequiredTimeStopped()
        {
            var t = new StopTimer(0.5f);
            Assert.IsFalse(t.Tick(0.3f, true));
            Assert.IsTrue(t.Tick(0.3f, true));
            Assert.IsTrue(t.IsDone);
        }

        [Test]
        public void FiresOnlyOnce()
        {
            var t = new StopTimer(0.5f);
            Assert.IsTrue(t.Tick(0.6f, true));
            Assert.IsFalse(t.Tick(0.6f, true));
        }

        [Test]
        public void MovingOrOccupied_ResetsProgress()
        {
            var t = new StopTimer(0.5f);
            t.Tick(0.4f, true);
            t.Tick(0.1f, false);
            Assert.IsFalse(t.Tick(0.4f, true));
            Assert.IsTrue(t.Tick(0.1f, true));
        }

        [Test]
        public void Reset_AllowsFiringAgain()
        {
            var t = new StopTimer(0.5f);
            t.Tick(1f, true);
            t.Reset();
            Assert.IsFalse(t.IsDone);
            Assert.IsTrue(t.Tick(1f, true));
        }
    }
}
