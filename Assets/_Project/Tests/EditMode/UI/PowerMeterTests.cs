using NUnit.Framework;
using UncannyValet.Core.Events;
using UncannyValet.UI.Hud;

namespace UncannyValet.Tests.UI
{
    public class PowerMeterTests
    {
        private const float Smoothing = 8f;
        private const float Pulse = 0.5f;

        private static PowerMeterView NewView() => new PowerMeterView(Smoothing, Pulse);

        [Test]
        public void Fill_EasesTowardTheTargetAndSettlesExactly()
        {
            var view = NewView();
            view.Set(0, 0.5f);
            view.Tick(0.05f);
            Assert.Greater(view.Fill, 0f);
            Assert.Less(view.Fill, 0.5f);
            for (var i = 0; i < 100; i++) view.Tick(0.05f);
            Assert.AreEqual(0.5f, view.Fill);
        }

        [Test]
        public void Fill_IsClampedAndLevelKeptInRange()
        {
            var view = NewView();
            view.Set(7, 3f);
            for (var i = 0; i < 100; i++) view.Tick(0.1f);
            Assert.AreEqual(1f, view.Fill);
            Assert.AreEqual(3, view.Level);
        }

        [Test]
        public void RisingFill_PulsesAndFades()
        {
            var view = NewView();
            view.Set(0, 0.2f);
            Assert.AreEqual(1f, view.Pulse01);
            view.Tick(Pulse / 2f);
            Assert.AreEqual(0.5f, view.Pulse01, 0.001f);
            view.Tick(Pulse);
            Assert.AreEqual(0f, view.Pulse01);
        }

        [Test]
        public void FallingFill_DoesNotPulse()
        {
            var view = NewView();
            view.Set(1, 0.8f);
            view.Tick(Pulse);
            view.Set(0, 0f);
            Assert.AreEqual(0f, view.Pulse01);
        }

        [Test]
        public void ReachingALevel_StartsTheReadyPulseOnce()
        {
            var view = NewView();
            view.Set(1, 0.4f);
            Assert.AreEqual(1f, view.Ready01);
            view.Tick(Pulse);
            view.Set(1, 0.5f);
            Assert.AreEqual(0f, view.Ready01);
            view.Set(2, 0.7f);
            Assert.AreEqual(1f, view.Ready01);
        }

        [Test]
        public void ActivePower_CountsDownAndEnds()
        {
            var active = new ActivePowers(new PowerMeterSettings());
            active.Start(1);
            Assert.IsTrue(active.IsActive(1));
            Assert.AreEqual(1f, active.Remaining01(1));
            active.Tick(2f);
            Assert.AreEqual(0.5f, active.Remaining01(1), 0.001f);
            active.Tick(10f);
            Assert.IsFalse(active.IsActive(1));
        }

        [Test]
        public void ActivePower_RefreshRestartsAndEndClears_UnknownPowerIgnored()
        {
            var active = new ActivePowers(new PowerMeterSettings());
            active.Start(2);
            active.Tick(3f);
            active.Start(2);
            Assert.AreEqual(1f, active.Remaining01(2));
            active.End(2);
            Assert.IsFalse(active.IsActive(2));
            active.Start(9);
            Assert.IsFalse(active.IsActive(9));
        }

        [Test]
        public void Tracker_FollowsTheCoreEventsAndStopsAfterDispose()
        {
            var bus = new EventBus();
            var tracker = new PowerMeterTracker(bus, new PowerMeterSettings());
            bus.Publish(new PowerMeterChanged(2, 0.7f));
            bus.Publish(new PowerActivated(PowerKind.SpeedBoost));
            Assert.AreEqual(2, tracker.View.Level);
            Assert.IsTrue(tracker.Active.IsActive((int)PowerKind.SpeedBoost));
            bus.Publish(new PowerEnded(PowerKind.SpeedBoost));
            Assert.IsFalse(tracker.Active.IsActive((int)PowerKind.SpeedBoost));
            tracker.Dispose();
            bus.Publish(new PowerMeterChanged(3, 1f));
            Assert.AreEqual(2, tracker.View.Level);
        }

        [Test]
        public void Tracker_FirstTickOnlyStartsTheClock()
        {
            var tracker = new PowerMeterTracker(new EventBus(), new PowerMeterSettings());
            tracker.Active.Start(1);
            tracker.Tick(500f);
            Assert.IsTrue(tracker.Active.IsActive(1));
            tracker.Tick(501f);
            Assert.Less(tracker.Active.Remaining01(1), 1f);
        }

        [Test]
        public void SquareRing_WalksEveryEdgePixelOnceClockwise()
        {
            const int side = 5;
            var seen = new System.Collections.Generic.HashSet<int>();
            for (var i = 0; i < SquareRing.Cells(side); i++)
            {
                SquareRing.Cell(side, i, out var x, out var y);
                Assert.IsTrue(x == 0 || y == 0 || x == side - 1 || y == side - 1);
                Assert.IsTrue(seen.Add(y * side + x));
            }
            Assert.AreEqual(16, seen.Count);
            SquareRing.Cell(side, 0, out var x0, out var y0);
            SquareRing.Cell(side, 4, out var x1, out var y1);
            Assert.AreEqual((0, 0), (x0, y0));
            Assert.AreEqual((4, 0), (x1, y1));
        }

        [Test]
        public void SquareRing_LitRoundsUpAndClamps()
        {
            Assert.AreEqual(0, SquareRing.Lit(13, 0f));
            Assert.AreEqual(1, SquareRing.Lit(13, 0.001f));
            Assert.AreEqual(SquareRing.Cells(13), SquareRing.Lit(13, 5f));
        }

        [Test]
        public void Settings_NotchesRiseAndLevelThreeIsAFullBar()
        {
            var s = new PowerMeterSettings();
            Assert.Less(s.NotchFill(1), s.NotchFill(2));
            Assert.AreEqual(1f, s.NotchFill(3));
        }
    }
}
