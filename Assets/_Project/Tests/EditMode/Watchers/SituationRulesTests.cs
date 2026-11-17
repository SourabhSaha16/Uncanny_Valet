using NUnit.Framework;
using UncannyValet.Core.Watchers;
using UncannyValet.Watchers;

namespace UncannyValet.Tests.Watchers
{
    public sealed class SituationRulesTests
    {
        [TestCase(0.79f, false)]
        [TestCase(0.8f, true)]
        [TestCase(1f, true)]
        public void PatienceNearlyGone(float elapsed, bool expected)
            => Assert.AreEqual(expected, SituationRules.IsPatienceNearlyGone(elapsed, 0.8f));

        [TestCase(3, 4, false)]
        [TestCase(4, 4, true)]
        [TestCase(5, 4, true)]
        [TestCase(0, 0, false)]
        public void QueueFull(int waiting, int slots, bool expected)
            => Assert.AreEqual(expected, SituationRules.IsQueueFull(waiting, slots));

        [TestCase(0.29f, false)]
        [TestCase(0.3f, false)]
        [TestCase(0.31f, true)]
        public void BigCrash_NeedsMoreThanTheAmount(float hit, bool expected)
            => Assert.AreEqual(expected, SituationRules.IsBigCrash(hit, 0.3f));

        [Test]
        public void Milestones_ReportHalfwayThenReached_EachOnce()
        {
            var tracker = new MilestoneTracker();
            Assert.AreEqual(GoalMilestone.None, tracker.Update(10, 100));
            Assert.AreEqual(GoalMilestone.Halfway, tracker.Update(50, 100));
            Assert.AreEqual(GoalMilestone.None, tracker.Update(60, 100));
            Assert.AreEqual(GoalMilestone.Reached, tracker.Update(100, 100));
            Assert.AreEqual(GoalMilestone.None, tracker.Update(150, 100));
        }

        [Test]
        public void Milestones_JumpingPastBoth_ReportsReachedOnly()
        {
            var tracker = new MilestoneTracker();
            Assert.AreEqual(GoalMilestone.Reached, tracker.Update(120, 100));
            Assert.AreEqual(GoalMilestone.None, tracker.Update(130, 100));
        }

        [Test]
        public void Milestones_DroppingBackDoesNotRepeat()
        {
            var tracker = new MilestoneTracker();
            tracker.Update(60, 100);
            Assert.AreEqual(GoalMilestone.None, tracker.Update(20, 100));
            Assert.AreEqual(GoalMilestone.None, tracker.Update(55, 100));
        }

        [Test]
        public void Milestones_ResetStartsAgain()
        {
            var tracker = new MilestoneTracker();
            tracker.Update(100, 100);
            tracker.Reset();
            Assert.AreEqual(GoalMilestone.Halfway, tracker.Update(50, 100));
        }

        [Test]
        public void LinePicker_GoesRoundRobinPerEvent()
        {
            var picker = new LinePicker();
            var lines = new[] { "A", "B" };
            picker.TryNext(WatcherEvent.PerfectPark, lines, out var first);
            picker.TryNext(WatcherEvent.PerfectPark, lines, out var second);
            picker.TryNext(WatcherEvent.PerfectPark, lines, out var third);
            picker.TryNext(WatcherEvent.Curse, lines, out var other);
            Assert.AreEqual("A", first);
            Assert.AreEqual("B", second);
            Assert.AreEqual("A", third);
            Assert.AreEqual("A", other, "every event keeps its own turn");
        }

        [Test]
        public void LinePicker_EmptyListSaysNothing()
            => Assert.IsFalse(new LinePicker().TryNext(WatcherEvent.Curse, new string[0], out _));
    }
}
