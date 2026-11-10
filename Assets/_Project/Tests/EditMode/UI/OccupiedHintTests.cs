using NUnit.Framework;
using UncannyValet.Core.Events;
using UncannyValet.UI;

namespace UncannyValet.Tests.UI
{
    public class OccupiedHintTests
    {
        private EventBus _bus;
        private float _now;
        private OccupiedHintTracker _tracker;

        [SetUp]
        public void SetUp()
        {
            _bus = new EventBus();
            _now = 10f;
            _tracker = new OccupiedHintTracker(_bus, () => _now);
        }

        [TearDown]
        public void TearDown() => _tracker.Dispose();

        [Test]
        public void HiddenUntilARefusal() => Assert.IsFalse(_tracker.IsVisible);

        [Test]
        public void ShowsAtTheStation_ThenHidesOneSecondAfterTheLastRefusal()
        {
            _bus.Publish(new StationEntryRefused(StationKind.Wash, 3f, 4f));
            Assert.IsTrue(_tracker.IsVisible);
            Assert.AreEqual(3f, _tracker.Anchor.x);
            _now += OccupiedHintTracker.HoldSeconds - 0.01f;
            Assert.IsTrue(_tracker.IsVisible);
            _now += 0.02f;
            Assert.IsFalse(_tracker.IsVisible);
        }

        [Test]
        public void RepeatedRefusalsKeepItVisible()
        {
            _bus.Publish(new StationEntryRefused(StationKind.Wash, 0f, 0f));
            _now += 0.9f;
            _bus.Publish(new StationEntryRefused(StationKind.Wash, 0f, 0f));
            _now += 0.9f;
            Assert.IsTrue(_tracker.IsVisible);
        }
    }
}
