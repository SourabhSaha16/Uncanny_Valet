using NUnit.Framework;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Core.Watchers;
using UncannyValet.Run;
using UncannyValet.Watchers;

namespace UncannyValet.Tests.Watchers
{
    /// <summary>Event trigger mapping: which game events make a watcher comment, and at which priority.</summary>
    public sealed class WatcherCommentsTests
    {
        private WatcherTestKit _kit;
        private WatcherComments _comments;
        private int _threshold;

        [SetUp]
        public void SetUp()
        {
            _kit = new WatcherTestKit();
            _threshold = 100;
            _comments = new WatcherComments(_kit.Bus, _kit.Voice(), WatcherTestKit.Text(), 0.3f,
                () => new LotGoal(new LotId(World.Midgard, LotKind.Crazy), _threshold));
        }

        [TearDown]
        public void TearDown() => _comments.Dispose();

        private void AssertOneComment(string startsWith)
        {
            Assert.AreEqual(1, _kit.Requests.Count);
            Assert.IsTrue(_kit.Requests[0].Text.StartsWith(startsWith), _kit.Requests[0].Text);
            Assert.AreEqual(SpeechPriority.Comment, _kit.Requests[0].Priority);
            Assert.AreEqual("TESTER", _kit.Requests[0].Speaker);
        }

        [Test]
        public void Curse_Comments() { _kit.Bus.Publish(new CurseApplied(new TicketId(1), 10)); AssertOneComment("Curse"); }

        [Test]
        public void PerfectPark_Comments() { _kit.Bus.Publish(new PerfectPark(new CarId(1), new TicketId(1))); AssertOneComment("PerfectPark"); }

        [Test]
        public void PerfectDropOff_Comments() { _kit.Bus.Publish(new PerfectDropOff(new CarId(1), new TicketId(1))); AssertOneComment("PerfectDropOff"); }

        [Test]
        public void Last30Seconds_Comments() { _kit.Bus.Publish(new ShiftLast30Seconds()); AssertOneComment("Last30Seconds"); }

        [Test]
        public void CrashAboveTheAmount_Comments() { _kit.Bus.Publish(new CarDamaged(new CarId(1), 0.5f, 0.4f)); AssertOneComment("BigCrash"); }

        [Test]
        public void CrashAtOrBelowTheAmount_IsIgnored()
        {
            _kit.Bus.Publish(new CarDamaged(new CarId(1), 0.9f, 0.3f));
            _kit.Bus.Publish(new CarDamaged(new CarId(1), 0.9f, 0.1f));
            Assert.AreEqual(0, _kit.Requests.Count);
        }

        [Test]
        public void ManyHitsAddingUpBeyondTheAmount_AreNotOneBigCrash()
            => Assert.IsFalse(SituationRules.IsBigCrash(0.1f, 0.3f), "only a single hit counts");

        [Test]
        public void HalfwayToTheGoal_CommentsOnce()
        {
            _kit.Bus.Publish(new BlessingsChanged(40, 1, 0));
            Assert.AreEqual(0, _kit.Requests.Count);
            _kit.Bus.Publish(new BlessingsChanged(50, 1, 0));
            AssertOneComment("HalfwayToGoal");
            _kit.Bus.Publish(new BlessingsChanged(70, 1, 0));
            Assert.AreEqual(1, _kit.Requests.Count);
        }

        [Test]
        public void ReachingTheGoal_Comments()
        {
            _kit.Bus.Publish(new BlessingsChanged(100, 1, 0));
            AssertOneComment("GoalReached");
        }

        [Test]
        public void MilestonesStartAgainOnANewShift()
        {
            _kit.Bus.Publish(new BlessingsChanged(100, 1, 0));
            _kit.Requests.Clear();
            _kit.Bus.Publish(new ShiftStarted());
            _kit.Bus.Publish(new BlessingsChanged(100, 1, 0));
            AssertOneComment("GoalReached");
        }

        [Test]
        public void TheSameEventTwice_SaysDifferentLines()
        {
            _kit.Bus.Publish(new PerfectPark(new CarId(1), new TicketId(1)));
            _kit.Bus.Publish(new PerfectPark(new CarId(2), new TicketId(2)));
            Assert.AreNotEqual(_kit.Requests[0].Text, _kit.Requests[1].Text);
        }

        [Test]
        public void AfterDispose_NothingIsSaid()
        {
            _comments.Dispose();
            _kit.Bus.Publish(new CurseApplied(new TicketId(1), 10));
            Assert.AreEqual(0, _kit.Requests.Count);
        }
    }
}
