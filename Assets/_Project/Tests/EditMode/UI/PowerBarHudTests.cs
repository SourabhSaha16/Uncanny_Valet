using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.UI.Hud;

namespace UncannyValet.Tests.UI
{
    public class PowerBarHudTests
    {
        private const float Smoothing = 8f;
        private const float Pulse = 0.5f;
        private const float Flash = 0.5f;

        private sealed class Owned : IRunProgress
        {
            public readonly List<StoryPower> Powers = new List<StoryPower>();
            public bool HasCurrentLot => false;
            public LotId CurrentLot => default;
            public IReadOnlyList<StoryPower> EarnedPowers => Powers;
            public int PowerLevel(StoryPower power) => Powers.Contains(power) ? 1 : 0;
            public bool IsCleared(LotId lot) => false;
            public bool IsWorldCleared(World world) => false;
            public int BestBlessings(LotId lot) => 0;
        }

        private static PowerBarView NewView() => new PowerBarView(Smoothing, Pulse, Flash);

        [Test]
        public void Fill_EasesTowardTheTargetAndSettlesExactly()
        {
            var view = NewView();
            view.Set(50f, 1, 0);
            view.Tick(0.05f);
            Assert.Greater(view.Fill, 0f);
            Assert.Less(view.Fill, 0.5f);
            for (var i = 0; i < 100; i++) view.Tick(0.05f);
            Assert.AreEqual(0.5f, view.Fill);
        }

        [Test]
        public void Values_AreClampedAndBucketsKeptInRange()
        {
            var view = NewView();
            view.Set(250f, 7, 0);
            for (var i = 0; i < 100; i++) view.Tick(0.1f);
            Assert.AreEqual(1f, view.Fill);
            Assert.AreEqual(3, view.OpenBuckets);
        }

        [Test]
        public void GainingPulses_AndLosingDoesNot()
        {
            var view = NewView();
            view.Set(20f, 0, 0);
            Assert.AreEqual(1f, view.Pulse01);
            view.Tick(Pulse);
            Assert.AreEqual(0f, view.Pulse01);
            view.Set(5f, 0, 0);
            Assert.AreEqual(0f, view.Pulse01);
        }

        [Test]
        public void SpendingFlashesThatBucketThenFades()
        {
            var view = NewView();
            view.Set(75f, 2, 0);
            view.Set(41.7f, 1, 2);
            Assert.AreEqual(2, view.SpentBucket);
            Assert.AreEqual(1f, view.Flash01);
            view.Tick(Flash / 2f);
            Assert.AreEqual(0.5f, view.Flash01, 0.001f);
            view.Tick(Flash);
            Assert.AreEqual(0f, view.Flash01);
        }

        [Test]
        public void ActivePower_CountsDownEndsAndIgnoresOthers()
        {
            var active = new ActivePowers(new PowerBarSettings());
            active.Start(PowerKind.FreezeTime);
            Assert.IsTrue(active.IsActive(PowerKind.FreezeTime));
            Assert.AreEqual(1f, active.Remaining01(PowerKind.FreezeTime));
            active.Tick(3f);
            Assert.AreEqual(0.5f, active.Remaining01(PowerKind.FreezeTime), 0.001f);
            active.Start(PowerKind.FreezeTime);
            Assert.AreEqual(1f, active.Remaining01(PowerKind.FreezeTime));
            active.End(PowerKind.FreezeTime);
            Assert.IsFalse(active.IsActive(PowerKind.FreezeTime));
            active.Start((PowerKind)9);
            Assert.IsFalse(active.IsActive((PowerKind)9));
        }

        [Test]
        public void Tracker_FollowsTheCoreEventsAndStopsAfterDispose()
        {
            var bus = new EventBus();
            var tracker = new PowerBarTracker(bus, new PowerBarSettings(), new Owned());
            bus.Publish(new PowerBarChanged(75f, 2));
            bus.Publish(new PowerActivated(PowerKind.FreezeTime));
            Assert.AreEqual(2, tracker.View.OpenBuckets);
            Assert.IsTrue(tracker.Active.IsActive(PowerKind.FreezeTime));
            bus.Publish(new PowerEnded(PowerKind.FreezeTime));
            Assert.IsFalse(tracker.Active.IsActive(PowerKind.FreezeTime));
            tracker.Dispose();
            bus.Publish(new PowerBarChanged(100f, 3));
            Assert.AreEqual(2, tracker.View.OpenBuckets);
        }

        [Test]
        public void TheBarIsVisibleOnlyOnceAPowerIsOwned()
        {
            var owned = new Owned();
            var tracker = new PowerBarTracker(new EventBus(), new PowerBarSettings(), owned);
            Assert.IsFalse(tracker.IsVisible);
            owned.Powers.Add(StoryPower.FreezeTime);
            Assert.IsTrue(tracker.IsVisible);
        }

        [Test]
        public void Tracker_FirstTickOnlyStartsTheClock()
        {
            var tracker = new PowerBarTracker(new EventBus(), new PowerBarSettings(), new Owned());
            tracker.Active.Start(PowerKind.FreezeTime);
            tracker.Tick(500f);
            Assert.AreEqual(1f, tracker.Active.Remaining01(PowerKind.FreezeTime));
            tracker.Tick(501f);
            Assert.Less(tracker.Active.Remaining01(PowerKind.FreezeTime), 1f);
        }

        [Test]
        public void Bolt_HasPixelsInEveryRowAndFitsItsBox()
        {
            Assert.Greater(BoltGlyph.PixelCount, BoltGlyph.Height);
            Assert.LessOrEqual(BoltGlyph.Width, 7);
        }

        [Test]
        public void SquareRing_LitRoundsUpAndClamps()
        {
            Assert.AreEqual(0, SquareRing.Lit(13, 0f));
            Assert.AreEqual(1, SquareRing.Lit(13, 0.001f));
            Assert.AreEqual(SquareRing.Cells(13), SquareRing.Lit(13, 5f));
        }
    }
}
