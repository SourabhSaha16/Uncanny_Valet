using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Orders;

namespace UncannyValet.Tests.Orders
{
    public class StrikeTrackerTests
    {
        private EventBus _bus;
        private TuningStore _tuning;
        private StrikeTracker _strikes;
        private System.Collections.Generic.List<StrikeGiven> _given;

        [SetUp]
        public void SetUp()
        {
            _bus = new EventBus();
            _tuning = new TuningStore(_bus);
            _tuning.Seed(TuningKey.MaxStrikes, 3f);
            _strikes = new StrikeTracker(_bus, _tuning);
            _strikes.Subscribe();
            _given = new System.Collections.Generic.List<StrikeGiven>();
            _bus.Subscribe<StrikeGiven>(e => _given.Add(e));
        }

        [Test]
        public void AddStrike_CountsAndPublishesTotalAndMax()
        {
            _strikes.AddStrike();
            _strikes.AddStrike();
            Assert.AreEqual(2, _strikes.Total);
            Assert.AreEqual(2, _given[1].Total);
            Assert.AreEqual(3, _given[1].Max);
            Assert.IsFalse(_given[1].IsFinal);
        }

        [Test]
        public void ThirdStrike_IsFinal()
        {
            _strikes.AddStrike(); _strikes.AddStrike(); _strikes.AddStrike();
            Assert.IsTrue(_given[2].IsFinal);
        }

        [Test]
        public void Max_FollowsLiveTuning()
        {
            _tuning.Set(TuningKey.MaxStrikes, 5f);
            _strikes.AddStrike();
            Assert.AreEqual(5, _given[0].Max);
        }

        [Test]
        public void StrikesCarryAcrossNights()
        {
            _strikes.AddStrike();
            _bus.Publish(new NightEnded(1));
            _bus.Publish(new NightStarted(2));
            _bus.Publish(new NightStarted(3));
            Assert.AreEqual(1, _strikes.Total);
        }

        [Test]
        public void NewRun_ResetsOnNightOneStarting()
        {
            _strikes.AddStrike(); _strikes.AddStrike();
            _bus.Publish(new NightStarted(1));
            Assert.AreEqual(0, _strikes.Total);
            _strikes.AddStrike();
            Assert.AreEqual(1, _given[2].Total);
        }

        [Test]
        public void Reset_ClearsTotal()
        {
            _strikes.AddStrike();
            _strikes.Reset();
            Assert.AreEqual(0, _strikes.Total);
        }

        [Test]
        public void Unsubscribe_StopsNewRunReset()
        {
            _strikes.AddStrike();
            _strikes.Unsubscribe();
            _bus.Publish(new NightStarted(1));
            Assert.AreEqual(1, _strikes.Total);
        }
    }
}
