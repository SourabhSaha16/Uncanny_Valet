using System.Text;
using NUnit.Framework;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.UI;

namespace UncannyValet.Tests.UI
{
    public class MessageFeedTests
    {
        private static readonly CarId Car = new CarId(4);

        private EventBus _bus;
        private MessageFeed _feed;
        private float _now;

        [SetUp]
        public void SetUp()
        {
            _now = 0f;
            _bus = new EventBus();
            // maxVisible 3, maxStrikes 3, done 4s, failed 8s, fade 1s, flash 2s
            _feed = new MessageFeed(_bus, () => _now, new MessageFeedSettings(3, 3, 4f, 8f, 1f, 2f));
        }

        [TearDown]
        public void TearDown() => _feed.Dispose();

        private void Issue(int id, char letter = 'B', int spot = 7)
            => _bus.Publish(new OrderIssued(new OrderId(id), Car, letter, spot));

        private void Resolve(int id, OrderOutcome outcome)
            => _bus.Publish(new OrderResolved(new OrderId(id), Car, outcome));

        [Test]
        public void Issue_AddsOpenMessageWithText()
        {
            Issue(1);
            Assert.AreEqual(1, _feed.Count);
            Assert.AreEqual("Take car B to spot 7.", _feed[0].Text);
            Assert.AreEqual(MessageStatus.Open, _feed[0].Status);
            Assert.AreEqual("OPEN", MessageFormatter.Badge(_feed[0].Status));
        }

        [Test]
        public void NewMessage_Flashes_ThenSettles()
        {
            Issue(1);
            Assert.AreEqual(1f, _feed.Flash(_feed[0]), 0.001f);
            _now = 1f;
            Assert.AreEqual(0.5f, _feed.Flash(_feed[0]), 0.001f);
            _now = 3f;
            Assert.AreEqual(0f, _feed.Flash(_feed[0]));
        }

        [Test]
        public void Passed_BecomesDone_HoldsThenFadesAndLeaves()
        {
            Issue(1);
            Resolve(1, OrderOutcome.Passed);
            Assert.AreEqual("DONE", MessageFormatter.Badge(_feed[0].Status));
            _now = 4f;
            Assert.AreEqual(1f, _feed.Alpha(_feed[0]), 0.001f);
            _now = 4.5f;
            Assert.AreEqual(0.5f, _feed.Alpha(_feed[0]), 0.001f);
            _feed.Tick();
            Assert.AreEqual(1, _feed.Count);
            _now = 5f;
            _feed.Tick();
            Assert.AreEqual(0, _feed.Count);
        }

        [Test]
        public void Failed_StaysLongerThanDone()
        {
            Issue(1);
            Issue(2);
            Resolve(1, OrderOutcome.Passed);
            Resolve(2, OrderOutcome.Failed);
            Assert.AreEqual("FAILED", MessageFormatter.Badge(_feed[1].Status));
            _now = 6f;
            _feed.Tick();
            Assert.AreEqual(1, _feed.Count);
            Assert.AreEqual(MessageStatus.Failed, _feed[0].Status);
            _now = 9f;
            _feed.Tick();
            Assert.AreEqual(0, _feed.Count);
        }

        [Test]
        public void Cancelled_FadesOutWithoutHold()
        {
            Issue(1);
            Resolve(1, OrderOutcome.Cancelled);
            _now = 0.5f;
            Assert.AreEqual(0.5f, _feed.Alpha(_feed[0]), 0.001f);
            _now = 1f;
            _feed.Tick();
            Assert.AreEqual(0, _feed.Count);
        }

        [Test]
        public void OpenMessage_NeverExpires()
        {
            Issue(1);
            _now = 1000f;
            _feed.Tick();
            Assert.AreEqual(1, _feed.Count);
            Assert.AreEqual(1f, _feed.Alpha(_feed[0]));
        }

        [Test]
        public void Cap_DropsOldest_NewestStaysAtBottom()
        {
            for (var i = 1; i <= 4; i++) Issue(i, (char)('A' + i), i);
            Assert.AreEqual(3, _feed.Count);
            Assert.AreEqual("Take car C to spot 2.", _feed[0].Text);
            Assert.AreEqual("Take car E to spot 4.", _feed[2].Text);
        }

        [Test]
        public void ResolveUnknownOrDroppedOrder_IsIgnored()
        {
            Issue(1);
            Resolve(99, OrderOutcome.Failed);
            Assert.AreEqual(MessageStatus.Open, _feed[0].Status);
        }

        [Test]
        public void SecondResolve_DoesNotOverwriteFirst()
        {
            Issue(1);
            Resolve(1, OrderOutcome.Passed);
            Resolve(1, OrderOutcome.Failed);
            Assert.AreEqual(MessageStatus.Done, _feed[0].Status);
        }

        [Test]
        public void Strikes_TrackTotalAndMax_FinalMarksRunLost()
        {
            Assert.AreEqual(3, _feed.MaxStrikes);
            _bus.Publish(new StrikeGiven(1, 3));
            Assert.AreEqual(1, _feed.Strikes);
            Assert.IsFalse(_feed.RunLost);
            _bus.Publish(new StrikeGiven(3, 3));
            Assert.IsTrue(_feed.RunLost);
        }

        [Test]
        public void NextNight_ClearsMessages_KeepsStrikes()
        {
            Issue(1);
            _bus.Publish(new StrikeGiven(2, 3));
            _bus.Publish(new NightStarted(2));
            Assert.AreEqual(0, _feed.Count);
            Assert.AreEqual(2, _feed.Strikes);
        }

        [Test]
        public void NightOne_ResetsStrikesAndRunLost()
        {
            Issue(1);
            _bus.Publish(new StrikeGiven(3, 3));
            _bus.Publish(new NightStarted(1));
            Assert.AreEqual(0, _feed.Count);
            Assert.AreEqual(0, _feed.Strikes);
            Assert.IsFalse(_feed.RunLost);
        }

        [Test]
        public void Dispose_StopsListening()
        {
            _feed.Dispose();
            Issue(1);
            Assert.AreEqual(0, _feed.Count);
            _feed = new MessageFeed(_bus, () => _now, new MessageFeedSettings(3, 3, 4f, 8f, 1f, 2f));
        }

        [Test]
        public void StrikeMarks_FillGiven()
        {
            var text = new StringBuilder();
            MessageFormatter.AppendStrikes(text, 2, 3);
            Assert.AreEqual("[X] [X] [ ]", text.ToString());
        }

        [Test]
        public void LostDetail_ShowsCount()
        {
            var text = new StringBuilder();
            MessageFormatter.AppendLostDetail(text, 3, 3);
            Assert.AreEqual("Strikes: 3 of 3", text.ToString());
        }
    }
}
