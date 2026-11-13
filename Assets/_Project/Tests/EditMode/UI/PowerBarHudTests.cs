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
            public readonly Dictionary<StoryPower, int> Levels = new Dictionary<StoryPower, int>();
            public readonly Dictionary<StoryPower, int> Flashed = new Dictionary<StoryPower, int>();
            public bool HasCurrentLot => false;
            public LotId CurrentLot => default;
            public IReadOnlyList<StoryPower> EarnedPowers => Powers;
            public int PowerLevel(StoryPower power) => Levels.TryGetValue(power, out var level) ? level : Powers.Contains(power) ? 1 : 0;
            public int FlashedLevel(StoryPower power) => Flashed.TryGetValue(power, out var level) ? level : 0;
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

        private sealed class FakePowers : IPowerView
        {
            public PowerState State;
            public bool Has = true;
            public bool TryGetState(StoryPower power, out PowerState state) { state = State; return Has; }
            public bool TryGetValues(StoryPower power, int level, out PowerLevelValues values) { values = default; return false; }
        }

        private static PowerState StateOf(PowerPhase phase, float remaining01 = 0f)
            => new PowerState(StoryPower.FreezeTime, 1, phase, remaining01, default);

        [Test]
        public void Icon_IsLitWhenReadyWithABucket_AndDimWithoutOne()
        {
            Assert.IsFalse(PowerIconLook.Of(StateOf(PowerPhase.Ready), 1).Dimmed);
            Assert.IsTrue(PowerIconLook.Of(StateOf(PowerPhase.Ready), 0).Dimmed);
            Assert.AreEqual(0f, PowerIconLook.Of(StateOf(PowerPhase.Ready), 3).RingFraction);
        }

        [Test]
        public void Icon_OnCooldownIsDimWithACoolingRing_AndRunningIsLitWithAnActiveRing()
        {
            var cooling = PowerIconLook.Of(StateOf(PowerPhase.Cooling, 0.4f), 3);
            Assert.IsTrue(cooling.Dimmed);
            Assert.AreEqual(0.4f, cooling.RingFraction, 0.0001f);
            Assert.IsFalse(cooling.RingIsActive);
            var active = PowerIconLook.Of(StateOf(PowerPhase.Active, 0.7f), 0);
            Assert.IsFalse(active.Dimmed);
            Assert.AreEqual(0.7f, active.RingFraction, 0.0001f);
            Assert.IsTrue(active.RingIsActive);
        }

        [Test]
        public void Flashes_FadeOutOnceStarted()
        {
            var flashes = new PowerIconFlashes(1f);
            Assert.AreEqual(0f, flashes.Flash01(StoryPower.FreezeTime));
            flashes.Begin(StoryPower.FreezeTime);
            Assert.AreEqual(1f, flashes.Flash01(StoryPower.FreezeTime));
            flashes.Tick(0.25f);
            Assert.AreEqual(0.75f, flashes.Flash01(StoryPower.FreezeTime), 0.0001f);
            flashes.Tick(5f);
            Assert.AreEqual(0f, flashes.Flash01(StoryPower.FreezeTime));
            Assert.AreEqual(0f, flashes.Flash01(StoryPower.None));
        }

        [Test]
        public void TheIconFlashesOnceAtTheFirstLotStartAfterTheGrant_AndNotAgain()
        {
            var bus = new EventBus();
            var owned = new Owned();
            var flashed = new List<PowerIconFlashed>();
            bus.Subscribe<PowerIconFlashed>(e => { flashed.Add(e); owned.Flashed[e.Power] = e.Level; });
            var tracker = new PowerBarTracker(bus, new PowerBarSettings(), owned, new FakePowers());
            owned.Powers.Add(StoryPower.FreezeTime);
            Assert.AreEqual(0f, tracker.Flashes.Flash01(StoryPower.FreezeTime), "no flash between lots, at the reward card");
            bus.Publish(new ShiftStarted());
            Assert.AreEqual(1f, tracker.Flashes.Flash01(StoryPower.FreezeTime));
            Assert.AreEqual(1, flashed.Count);
            tracker.Flashes.Clear();
            bus.Publish(new ShiftStarted());
            Assert.AreEqual(0f, tracker.Flashes.Flash01(StoryPower.FreezeTime), "already flashed at this level");
            owned.Levels[StoryPower.FreezeTime] = 2;
            bus.Publish(new ShiftStarted());
            Assert.AreEqual(1f, tracker.Flashes.Flash01(StoryPower.FreezeTime), "an upgrade flashes at the next lot start too");
            Assert.AreEqual(2, flashed[1].Level);
        }

        [Test]
        public void Tracker_ReadsTheLookFromThePowerViewAndStopsAfterDispose()
        {
            var bus = new EventBus();
            var powers = new FakePowers { State = StateOf(PowerPhase.Cooling, 0.5f) };
            var tracker = new PowerBarTracker(bus, new PowerBarSettings(), new Owned(), powers);
            bus.Publish(new PowerBarChanged(75f, 2));
            Assert.AreEqual(2, tracker.View.OpenBuckets);
            Assert.IsTrue(tracker.TryGetLook(StoryPower.FreezeTime, out var look));
            Assert.IsTrue(look.Dimmed);
            powers.Has = false;
            Assert.IsFalse(tracker.TryGetLook(StoryPower.FreezeTime, out _));
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
            var tracker = new PowerBarTracker(new EventBus(), new PowerBarSettings(), new Owned(), new FakePowers());
            tracker.Flashes.Begin(StoryPower.FreezeTime);
            tracker.Tick(500f);
            Assert.AreEqual(1f, tracker.Flashes.Flash01(StoryPower.FreezeTime));
            tracker.Tick(500.1f);
            Assert.Less(tracker.Flashes.Flash01(StoryPower.FreezeTime), 1f);
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
