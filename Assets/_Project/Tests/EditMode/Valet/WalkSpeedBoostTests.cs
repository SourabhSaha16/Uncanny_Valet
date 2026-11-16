using NUnit.Framework;
using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Valet;

namespace UncannyValet.Tests.Valet
{
    public class WalkSpeedBoostTests
    {
        private sealed class View : IPowerView
        {
            private readonly float _levelOne;
            private readonly float _levelTwo;

            public View(float levelOne, float levelTwo) { _levelOne = levelOne; _levelTwo = levelTwo; }

            public bool TryGetState(StoryPower power, out PowerState state) { state = default; return false; }

            public bool TryGetValues(StoryPower power, int level, out PowerLevelValues values)
            {
                values = new PowerLevelValues(4f, 1.5f, 0f, 1.5f, 2f, level == 2 ? _levelTwo : _levelOne);
                return level == 1 || level == 2;
            }
        }

        private EventBus _bus;
        private WalkSpeedBoost _boost;

        [SetUp]
        public void SetUp()
        {
            _bus = new EventBus();
            _boost = new WalkSpeedBoost(_bus, () => new View(1.6f, 2f));
        }

        [TearDown]
        public void TearDown() => _boost.Dispose();

        [Test]
        public void ItIsPlainSpeedUntilThePowerStarts()
        {
            Assert.AreEqual(1f, _boost.Multiplier);
        }

        [Test]
        public void TheMultiplierIsOnWhileThePowerRuns_AndOffAfterItEnds()
        {
            _bus.Publish(new PowerActivated(StoryPower.ThunderStride, 1));
            Assert.AreEqual(1.6f, _boost.Multiplier, 0.0001f);
            _bus.Publish(new PowerEnded(StoryPower.ThunderStride));
            Assert.AreEqual(1f, _boost.Multiplier);
        }

        [Test]
        public void ARefiredPowerSetsTheLevelsNumberAndDoesNotStack()
        {
            _bus.Publish(new PowerActivated(StoryPower.ThunderStride, 1));
            _bus.Publish(new PowerActivated(StoryPower.ThunderStride, 1));
            Assert.AreEqual(1.6f, _boost.Multiplier, 0.0001f);
            _bus.Publish(new PowerActivated(StoryPower.ThunderStride, 2));
            Assert.AreEqual(2f, _boost.Multiplier, 0.0001f, "the new level replaces, not multiplies");
            _bus.Publish(new PowerEnded(StoryPower.ThunderStride));
            Assert.AreEqual(1f, _boost.Multiplier);
        }

        [Test]
        public void OtherPowersDoNotTouchIt()
        {
            _bus.Publish(new PowerActivated(StoryPower.ThunderStride, 1));
            _bus.Publish(new PowerActivated(StoryPower.FreezeTime, 1));
            _bus.Publish(new PowerEnded(StoryPower.FreezeTime));
            Assert.AreEqual(1.6f, _boost.Multiplier, 0.0001f);
        }

        [Test]
        public void ANumberBelowOneNeverSlowsTheValet()
        {
            using (var weak = new WalkSpeedBoost(_bus, () => new View(0.5f, 0.5f)))
            {
                _bus.Publish(new PowerActivated(StoryPower.ThunderStride, 1));
                Assert.AreEqual(1f, weak.Multiplier);
            }
        }

        [Test]
        public void ADisposedBoostStopsListeningAndIsBackToOne()
        {
            _bus.Publish(new PowerActivated(StoryPower.ThunderStride, 1));
            _boost.Dispose();
            Assert.AreEqual(1f, _boost.Multiplier);
            _bus.Publish(new PowerActivated(StoryPower.ThunderStride, 1));
            Assert.AreEqual(1f, _boost.Multiplier);
        }

        [Test]
        public void WithNoPowerViewYetItStaysPlain()
        {
            using (var none = new WalkSpeedBoost(_bus, () => null))
            {
                _bus.Publish(new PowerActivated(StoryPower.ThunderStride, 1));
                Assert.AreEqual(1f, none.Multiplier);
            }
        }
    }
}
