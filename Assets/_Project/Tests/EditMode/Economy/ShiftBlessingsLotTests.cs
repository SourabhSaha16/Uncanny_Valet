using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UncannyValet.Core.Config;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Economy;

namespace UncannyValet.Tests.Economy
{
    /// <summary>How ShiftBlessings judges the lot against its threshold when the shift ends.</summary>
    public class ShiftBlessingsLotTests
    {
        private sealed class FakeGoal : ILotGoal
        {
            public LotId Lot { get; set; } = new LotId(World.Midgard, LotKind.Training);
            public int BlessingThreshold { get; set; } = 30;
        }

        private EventBus _bus;
        private ShiftConfig _config;
        private ShiftBlessings _blessings;
        private FakeGoal _goal;

        [SetUp]
        public void SetUp()
        {
            _bus = new EventBus();
            _config = ScriptableObject.CreateInstance<ShiftConfig>();
            _goal = new FakeGoal();
            _blessings = new ShiftBlessings(_bus, _config, () => _goal);
            _bus.Publish(new ShiftStarted());
        }

        [TearDown]
        public void TearDown()
        {
            _blessings.Dispose();
            Object.DestroyImmediate(_config);
        }

        private void Blessing(int amount) => _bus.Publish(new BlessingEarned(new TicketId(1), amount));
        private void Angry() => _bus.Publish(new TicketTimerExpired(new TicketId(1), new CarId(1), TimerKind.Pickup));

        [TestCase(29, false)]
        [TestCase(30, true)]
        [TestCase(31, true)]
        public void ShiftEnd_JudgesTheLotAgainstTheThreshold(int earned, bool cleared)
        {
            var log = new List<IGameEvent>();
            _bus.Subscribe<ShiftResultReady>(e => log.Add(e));
            _bus.Subscribe<LotCleared>(e => log.Add(e));
            _bus.Subscribe<LotFailed>(e => log.Add(e));
            Blessing(earned);
            _bus.Publish(new ShiftEnded());

            Assert.AreEqual(cleared, ((ShiftResultReady)log[0]).Result.Cleared);
            Assert.AreEqual(2, log.Count, "the result, then one lot event");
            if (cleared) Assert.AreEqual(_goal.Lot, ((LotCleared)log[1]).Lot);
            else Assert.AreEqual(_goal.Lot, ((LotFailed)log[1]).Lot);
        }

        [Test]
        public void ACurseLateInTheShift_CanDropTheLotBelowItsThreshold()
        {
            var failed = 0;
            _bus.Subscribe<LotFailed>(e => failed++);
            Blessing(40);
            Angry();
            _bus.Publish(new ShiftEnded());
            Assert.AreEqual(1, failed);
        }

        [Test]
        public void WithoutALotGoal_TheResultIsPublishedButNoLotIsJudged()
        {
            _goal = null;
            var lotEvents = 0;
            _bus.Subscribe<LotCleared>(e => lotEvents++);
            _bus.Subscribe<LotFailed>(e => lotEvents++);
            ShiftResult result = default;
            _bus.Subscribe<ShiftResultReady>(e => result = e.Result);
            Blessing(10);
            _bus.Publish(new ShiftEnded());
            Assert.AreEqual(0, lotEvents);
            Assert.AreEqual(10, result.Blessings);
            Assert.IsFalse(result.Cleared);
            Assert.AreEqual(0, result.Threshold);
        }
    }
}
