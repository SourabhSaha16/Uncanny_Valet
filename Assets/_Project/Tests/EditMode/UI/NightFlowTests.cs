using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Bootstrap;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;

namespace UncannyValet.Tests.UI
{
    public class NightFlowTests
    {
        private sealed class Fake : INightStarter, IValetPlacer, IRunReset
        {
            public readonly List<int> Started = new List<int>();
            public int Placed;
            public int Resets;
            public bool IsRunning { get; set; }
            public void StartNight(int night) => Started.Add(night);
            public void PlaceAtStart() => Placed++;
            public void ResetRun() => Resets++;
        }

        private const int Total = 3;
        private EventBus _bus;
        private Fake _fake;
        private NightFlow _flow;

        [SetUp]
        public void SetUp()
        {
            _bus = new EventBus();
            _fake = new Fake();
            _flow = new NightFlow(_bus, _fake, _fake, _fake, Total);
        }

        [TearDown]
        public void TearDown() => _flow.Dispose();

        [Test]
        public void FirstNight_StartsNightOne() { _flow.StartFirstNight(); CollectionAssert.AreEqual(new[] { 1 }, _fake.Started); }

        [Test]
        public void Next_StartsFollowingNight_AndPlacesValet()
        {
            _bus.Publish(new NextNightRequested(1));
            CollectionAssert.AreEqual(new[] { 2 }, _fake.Started);
            Assert.AreEqual(1, _fake.Placed);
            Assert.AreEqual(0, _fake.Resets);
        }

        [Test]
        public void AfterFinalNight_ResetsRunAndStartsNightOne()
        {
            _bus.Publish(new NextNightRequested(Total));
            CollectionAssert.AreEqual(new[] { 1 }, _fake.Started);
            Assert.AreEqual(1, _fake.Resets);
        }

        [Test]
        public void WhileDriving_ValetIsNotMoved()
        {
            _bus.Publish(new CarEntered(new CarId(1)));
            _bus.Publish(new NextNightRequested(1));
            Assert.AreEqual(0, _fake.Placed);
            CollectionAssert.AreEqual(new[] { 2 }, _fake.Started);
        }

        [Test]
        public void AfterExit_ValetIsMovedAgain()
        {
            _bus.Publish(new CarEntered(new CarId(1)));
            _bus.Publish(new CarExited(new CarId(1)));
            _bus.Publish(new NextNightRequested(1));
            Assert.AreEqual(1, _fake.Placed);
        }

        [Test]
        public void NightStillRunning_IgnoresRequest()
        {
            _fake.IsRunning = true;
            _bus.Publish(new NextNightRequested(1));
            Assert.IsEmpty(_fake.Started);
        }

        [Test]
        public void Dispose_Unsubscribes()
        {
            _flow.Dispose();
            _bus.Publish(new NextNightRequested(1));
            Assert.IsEmpty(_fake.Started);
            _flow = new NightFlow(_bus, _fake, _fake, _fake, Total);
        }
    }
}
