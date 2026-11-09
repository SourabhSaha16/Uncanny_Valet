using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UncannyValet.Core.Config;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Economy;

namespace UncannyValet.Tests.Economy
{
    public class ShiftScoreTests
    {
        private EventBus _bus;
        private ShiftConfig _config;
        private ShiftScore _score;

        [SetUp]
        public void SetUp()
        {
            _bus = new EventBus();
            _config = ScriptableObject.CreateInstance<ShiftConfig>();
            _score = new ShiftScore(_bus, _config);
            _bus.Publish(new ShiftStarted());
        }

        [TearDown]
        public void TearDown()
        {
            _score.Dispose();
            Object.DestroyImmediate(_config);
        }

        private void Tip(int amount) => _bus.Publish(new TipEarned(new TicketId(1), amount));
        private void Angry(TimerKind kind) => _bus.Publish(new TicketTimerExpired(new TicketId(1), new CarId(1), kind));

        [Test]
        public void Tips_AddToTheScore_AndRaiseTheCombo()
        {
            Tip(20);
            Tip(40);
            Assert.AreEqual(60, _score.Score);
            Assert.AreEqual(3, _score.ComboMultiplier);
        }

        [Test]
        public void ZeroTip_ResetsTheCombo_WithoutAComplaint()
        {
            Tip(20);
            Tip(0);
            Assert.AreEqual(1, _score.ComboMultiplier);
            Assert.AreEqual(20, _score.Score);
            Assert.AreEqual(0, _score.Complaints);
        }

        [TestCase(TimerKind.Park)]
        [TestCase(TimerKind.Pickup)]
        public void AngryPatron_CostsTheComplaintPenalty_AndResetsTheCombo(TimerKind kind)
        {
            Tip(20);
            Tip(40);
            Angry(kind);
            Assert.AreEqual(60 - 15, _score.Score);
            Assert.AreEqual(1, _score.Complaints);
            Assert.AreEqual(1, _score.ComboMultiplier);
        }

        [Test]
        public void Complaint_PublishesAnEventWithThePenalty()
        {
            ComplaintFiled seen = default;
            _bus.Subscribe<ComplaintFiled>(e => seen = e);
            Angry(TimerKind.Park);
            Assert.AreEqual(15, seen.Penalty);
            Assert.AreEqual(new TicketId(1), seen.Ticket);
        }

        [Test]
        public void Score_NeverDropsBelowZero()
        {
            Angry(TimerKind.Park);
            Angry(TimerKind.Pickup);
            Assert.AreEqual(0, _score.Score);
            Assert.AreEqual(2, _score.Complaints);
        }

        [Test]
        public void Stars_FollowThePlaceholderThresholds()
        {
            Assert.AreEqual(150, _score.OneStarScore);
            Assert.AreEqual(300, _score.TwoStarScore);
            Assert.AreEqual(450, _score.ThreeStarScore);
            Assert.AreEqual(0, _score.Stars);
            Tip(150);
            Assert.AreEqual(1, _score.Stars);
            Tip(150);
            Assert.AreEqual(2, _score.Stars);
            Tip(150);
            Assert.AreEqual(3, _score.Stars);
        }

        [Test]
        public void EveryChange_PublishesScoreChangedWithTheCurrentNumbers()
        {
            var seen = new List<ScoreChanged>();
            _bus.Subscribe<ScoreChanged>(seen.Add);
            Tip(200);
            Angry(TimerKind.Pickup);
            Assert.AreEqual(2, seen.Count);
            Assert.AreEqual(200, seen[0].Score);
            Assert.AreEqual(2, seen[0].ComboMultiplier);
            Assert.AreEqual(1, seen[0].Stars);
            Assert.AreEqual(185, seen[1].Score);
            Assert.AreEqual(1, seen[1].Complaints);
            Assert.AreEqual(1, seen[1].ComboMultiplier);
        }

        [Test]
        public void ShiftEnd_PublishesTheResultOnce_AndLaterEventsAreIgnored()
        {
            var results = new List<ShiftResult>();
            _bus.Subscribe<ShiftResultReady>(e => results.Add(e.Result));
            Tip(320);
            Angry(TimerKind.Park);
            _bus.Publish(new ShiftEnded());
            _bus.Publish(new ShiftEnded());
            Tip(500);
            Angry(TimerKind.Park);

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(305, results[0].Score);
            Assert.AreEqual(2, results[0].Stars);
            Assert.AreEqual(1, results[0].Complaints);
            Assert.AreEqual(150, results[0].OneStarScore);
            Assert.AreEqual(305, _score.Score);
        }

        [Test]
        public void NewShift_ResetsEverything()
        {
            Tip(100);
            Angry(TimerKind.Park);
            _bus.Publish(new ShiftEnded());
            _bus.Publish(new ShiftStarted());
            Assert.AreEqual(0, _score.Score);
            Assert.AreEqual(0, _score.Complaints);
            Assert.AreEqual(1, _score.ComboMultiplier);
            Tip(10);
            Assert.AreEqual(10, _score.Score);
        }

        [Test]
        public void BeforeTheFirstShift_NothingCounts()
        {
            var bus = new EventBus();
            var score = new ShiftScore(bus, _config);
            bus.Publish(new TipEarned(new TicketId(1), 50));
            bus.Publish(new TicketTimerExpired(new TicketId(1), new CarId(1), TimerKind.Park));
            Assert.AreEqual(0, score.Score);
            Assert.AreEqual(0, score.Complaints);
            score.Dispose();
        }

        [Test]
        public void Dispose_StopsListening()
        {
            _score.Dispose();
            Tip(50);
            Assert.AreEqual(0, _score.Score);
        }
    }
}
