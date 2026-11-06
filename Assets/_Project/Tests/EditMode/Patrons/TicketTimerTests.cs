using NUnit.Framework;
using UncannyValet.Patrons;

namespace UncannyValet.Tests.Patrons
{
    public class TicketTimerTests
    {
        [Test]
        public void ElapsedFraction_GrowsWithMinutes()
        {
            var timer = new TicketTimer(40f);
            timer.Start(10f);
            timer.Tick(20f);
            Assert.AreEqual(0.25f, timer.Elapsed01, 0.0001f);
        }

        [Test]
        public void ExpiresExactlyOnce()
        {
            var timer = new TicketTimer(10f);
            timer.Start(0f);
            Assert.IsFalse(timer.Tick(9f));
            Assert.IsTrue(timer.Tick(10f));
            Assert.IsFalse(timer.Tick(11f));
            Assert.IsTrue(timer.IsExpired);
            Assert.AreEqual(1f, timer.Elapsed01);
        }

        [Test]
        public void PausedTime_DoesNotCount()
        {
            var timer = new TicketTimer(10f);
            timer.Start(0f);
            timer.Pause(4f);
            Assert.IsFalse(timer.Tick(50f));
            timer.Resume(50f);
            Assert.AreEqual(0.4f, timer.Elapsed01, 0.0001f);
            Assert.IsFalse(timer.Tick(55f));
            Assert.IsTrue(timer.Tick(56f));
        }

        [Test]
        public void Stop_ReportsOnceAndHaltsExpiry()
        {
            var timer = new TicketTimer(10f);
            timer.Start(0f);
            Assert.IsTrue(timer.Stop());
            Assert.IsFalse(timer.Stop());
            Assert.IsFalse(timer.Tick(100f));
            Assert.IsFalse(timer.IsExpired);
        }

        [Test]
        public void NeverStarted_DoesNothing()
        {
            var timer = new TicketTimer(10f);
            Assert.IsFalse(timer.Tick(100f));
            Assert.IsFalse(timer.Stop());
        }

        [Test]
        public void ExpiredTimer_StaysActiveUntilStopped()
        {
            var timer = new TicketTimer(5f);
            timer.Start(0f);
            timer.Tick(5f);
            Assert.IsTrue(timer.IsActive);
            Assert.IsTrue(timer.Stop());
        }
    }
}
