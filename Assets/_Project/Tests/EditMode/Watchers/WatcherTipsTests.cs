using NUnit.Framework;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Core.Modifiers;
using UncannyValet.Core.Watchers;
using UncannyValet.Watchers;
using UnityEngine;

namespace UncannyValet.Tests.Watchers
{
    /// <summary>Situation triggers of the tips: each situation (condition true and false), once per tip, spacing, and the director dropping a tip.</summary>
    public sealed class WatcherTipsTests
    {
        private const int Slots = 4;
        private WatcherTestKit _kit;
        private WatcherTips _tips;
        private WatcherConfig _config;
        private float _now;
        private int _waiting;

        [SetUp]
        public void SetUp()
        {
            _kit = new WatcherTestKit();
            _config = ScriptableObject.CreateInstance<WatcherConfig>();
            _now = 0f;
            _waiting = 0;
            _tips = new WatcherTips(_kit.Bus, _kit.Voice(), WatcherTestKit.Text(), _config, Slots, () => _waiting, () => _now);
        }

        [TearDown]
        public void TearDown()
        {
            _tips.Dispose();
            Object.DestroyImmediate(_config);
        }

        private void AssertTip(TipSituation situation)
        {
            Assert.AreEqual(1, _kit.Requests.Count);
            Assert.AreEqual("TIP " + situation, _kit.Requests[0].Text);
            Assert.AreEqual(SpeechPriority.Tip, _kit.Requests[0].Priority);
        }

        // The director showed the request: the tip counts as said.
        private void ShowLast() => _kit.Bus.Publish(new SpeechShown(_kit.Requests[_kit.Requests.Count - 1], 4f));

        [Test]
        public void FirstArrival_Tips() { _kit.Bus.Publish(new TicketOpened(new TicketId(1), new CarId(1), 5f)); AssertTip(TipSituation.FirstArrival); }

        [Test]
        public void MuddyCar_Tips()
        {
            _kit.Bus.Publish(new TicketModifiersAssigned(new TicketId(1), new CarId(1), ModifierSet.Of(ModifierId.Muddy)));
            AssertTip(TipSituation.MuddyCar);
        }

        [Test]
        public void ChargingCar_Tips()
        {
            _kit.Bus.Publish(new TicketModifiersAssigned(new TicketId(1), new CarId(1), ModifierSet.Of(ModifierId.NeedsCharge)));
            AssertTip(TipSituation.ChargeCar);
        }

        [Test]
        public void CleanCar_NoTip()
        {
            _kit.Bus.Publish(new TicketModifiersAssigned(new TicketId(1), new CarId(1), ModifierSet.None));
            Assert.AreEqual(0, _kit.Requests.Count);
        }

        [Test]
        public void PatienceNearlyGone_Tips_AndPlentyLeft_Does_Not()
        {
            _kit.Bus.Publish(new TicketTimerChanged(new TicketId(1), new CarId(1), TimerKind.Pickup, 0.5f));
            Assert.AreEqual(0, _kit.Requests.Count);
            _kit.Bus.Publish(new TicketTimerChanged(new TicketId(1), new CarId(1), TimerKind.Pickup, 0.85f));
            AssertTip(TipSituation.PatienceLow);
        }

        [Test]
        public void FullQueue_Tips_AndRoomLeft_Does_Not()
        {
            _waiting = Slots - 1;
            _kit.Bus.Publish(new ShiftTick(1f));
            Assert.AreEqual(0, _kit.Requests.Count);
            _waiting = Slots;
            _kit.Bus.Publish(new ShiftTick(2f));
            AssertTip(TipSituation.QueueFull);
        }

        [Test]
        public void DamagedCar_Tips() { _kit.Bus.Publish(new CarDamaged(new CarId(1), 0.1f, 0.1f)); AssertTip(TipSituation.CarDamaged); }

        [Test]
        public void ATipThatWasShown_IsNotSaidAgain()
        {
            _kit.Bus.Publish(new CarDamaged(new CarId(1), 0.1f, 0.1f));
            ShowLast();
            _now = 100f;
            _kit.Bus.Publish(new CarDamaged(new CarId(1), 0.2f, 0.1f));
            Assert.AreEqual(1, _kit.Requests.Count, "each tip is said at most once per lot");
        }

        [Test]
        public void ATipThatWasDropped_IsOfferedAgain()
        {
            _kit.Bus.Publish(new CarDamaged(new CarId(1), 0.1f, 0.1f));
            _kit.Bus.Publish(new CarDamaged(new CarId(1), 0.2f, 0.1f));
            Assert.AreEqual(2, _kit.Requests.Count);
        }

        [Test]
        public void TwoTips_AreAtLeastTheTipGapApart()
        {
            _kit.Bus.Publish(new CarDamaged(new CarId(1), 0.1f, 0.1f));
            ShowLast();
            _now = _config.TipGapSeconds - 0.1f;
            _kit.Bus.Publish(new TicketOpened(new TicketId(1), new CarId(1), 5f));
            Assert.AreEqual(1, _kit.Requests.Count);
            _now = _config.TipGapSeconds;
            _kit.Bus.Publish(new TicketOpened(new TicketId(1), new CarId(1), 5f));
            Assert.AreEqual(2, _kit.Requests.Count);
        }

        [Test]
        public void SomeoneElsesBubble_DoesNotUseUpATip()
        {
            _kit.Bus.Publish(new CarDamaged(new CarId(1), 0.1f, 0.1f));
            var other = new SpeechRequest("OTHER", _kit.Requests[0].Text, Vector2.zero, null, SpeechPriority.Tip);
            _kit.Bus.Publish(new SpeechShown(other, 4f));
            _kit.Bus.Publish(new CarDamaged(new CarId(1), 0.2f, 0.1f));
            Assert.AreEqual(2, _kit.Requests.Count);
        }

        [Test]
        public void ThroughTheDirector_TipBeatsAComment_AndOnlyOneBubbleShows()
        {
            var director = new SpeechDirector(_kit.Bus, new SpeechScheduler(4f, 8f));
            var comments = new WatcherComments(_kit.Bus, _kit.Voice(), WatcherTestKit.Text(), 0.3f, () => null);
            _kit.Bus.Publish(new CarDamaged(new CarId(1), 0.6f, 0.5f)); // a big crash comment and a damage tip at once
            director.Tick(1f);
            Assert.AreEqual(1, _kit.Shown.Count);
            Assert.AreEqual(SpeechPriority.Tip, _kit.Shown[0].Request.Priority);
            comments.Dispose();
            director.Dispose();
        }
    }
}
