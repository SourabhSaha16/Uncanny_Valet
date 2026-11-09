using NUnit.Framework;
using UncannyValet.Bootstrap;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;

namespace UncannyValet.Tests.UI
{
    public class ShiftFlowTests
    {
        private sealed class Fake : IShiftStarter, IValetPlacer
        {
            public int Started;
            public int Placed;
            public bool IsRunning { get; set; }
            public void StartShift() => Started++;
            public void PlaceAtStart() => Placed++;
        }

        private EventBus _bus;
        private Fake _fake;
        private ShiftFlow _flow;

        [SetUp]
        public void SetUp()
        {
            _bus = new EventBus();
            _fake = new Fake();
            _flow = new ShiftFlow(_bus, _fake, _fake);
        }

        [TearDown]
        public void TearDown() => _flow.Dispose();

        [Test]
        public void FirstShift_StartsTheShift()
        {
            _flow.StartFirstShift();
            Assert.AreEqual(1, _fake.Started);
        }

        [Test]
        public void Retry_StartsTheShiftAgain_AndPlacesTheValet()
        {
            _bus.Publish(new RetryShiftRequested());
            Assert.AreEqual(1, _fake.Started);
            Assert.AreEqual(1, _fake.Placed);
        }

        [Test]
        public void WhileDriving_ValetIsNotMoved()
        {
            _bus.Publish(new CarEntered(new CarId(1)));
            _bus.Publish(new RetryShiftRequested());
            Assert.AreEqual(0, _fake.Placed);
            Assert.AreEqual(1, _fake.Started);
        }

        [Test]
        public void AfterExit_ValetIsMovedAgain()
        {
            _bus.Publish(new CarEntered(new CarId(1)));
            _bus.Publish(new CarExited(new CarId(1)));
            _bus.Publish(new RetryShiftRequested());
            Assert.AreEqual(1, _fake.Placed);
        }

        [Test]
        public void ShiftStillRunning_IgnoresRetry()
        {
            _fake.IsRunning = true;
            _bus.Publish(new RetryShiftRequested());
            Assert.AreEqual(0, _fake.Started);
            Assert.AreEqual(0, _fake.Placed);
        }

        [Test]
        public void Dispose_Unsubscribes()
        {
            _flow.Dispose();
            _bus.Publish(new RetryShiftRequested());
            Assert.AreEqual(0, _fake.Started);
            _flow = new ShiftFlow(_bus, _fake, _fake);
        }
    }
}
