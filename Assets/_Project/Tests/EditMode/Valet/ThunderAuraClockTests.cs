using NUnit.Framework;
using UncannyValet.Valet;

namespace UncannyValet.Tests.Valet
{
    public class ThunderAuraClockTests
    {
        private ThunderAuraClock _clock;

        [SetUp]
        public void SetUp()
        {
            // 2 Hz pulse, floor 0.5, 1 s warning, 4 blinks per second.
            _clock = new ThunderAuraClock(new ThunderAuraTiming { PulseHz = 2f, PulseFloor = 0.5f, WarningSeconds = 1f, BlinkHz = 4f });
        }

        [Test]
        public void NothingShowsBeforeItStarts()
        {
            Assert.IsFalse(_clock.IsActive);
            Assert.AreEqual(0f, _clock.Strength);
            Assert.AreEqual(0f, _clock.Remaining);
        }

        [Test]
        public void ThePulseStaysBetweenTheFloorAndOneWhileItRuns()
        {
            _clock.Start(5f);
            for (var i = 0; i < 160; i++)
            {
                Assert.GreaterOrEqual(_clock.Strength, 0.5f - 0.0001f, "step " + i);
                Assert.LessOrEqual(_clock.Strength, 1.0001f, "step " + i);
                Assert.IsFalse(_clock.InWarning);
                _clock.Tick(0.025f);
            }
        }

        [Test]
        public void ThePulseReachesBothTheFloorAndTheTop()
        {
            _clock.Start(5f);
            var low = 1f;
            var high = 0f;
            for (var i = 0; i < 100; i++)
            {
                low = UnityEngine.Mathf.Min(low, _clock.Strength);
                high = UnityEngine.Mathf.Max(high, _clock.Strength);
                _clock.Tick(0.01f);
            }
            Assert.AreEqual(0.5f, low, 0.01f);
            Assert.AreEqual(1f, high, 0.01f);
        }

        [Test]
        public void TheWarningStartsInTheLastSecondAndBlinksFullOnAndOff()
        {
            _clock.Start(3f);
            _clock.Tick(1.9f);
            Assert.IsFalse(_clock.InWarning);
            _clock.Tick(0.15f);
            Assert.IsTrue(_clock.InWarning);
            Assert.AreEqual(1f, _clock.Strength, "first blink phase is on");
            _clock.Tick(0.13f);
            Assert.AreEqual(0f, _clock.Strength, "then off (4 blinks per second = 0.125 s per phase)");
            _clock.Tick(0.125f);
            Assert.AreEqual(1f, _clock.Strength);
        }

        [Test]
        public void ItClearsWhenTheTimeRunsOut()
        {
            _clock.Start(2f);
            _clock.Tick(1.99f);
            Assert.IsTrue(_clock.IsActive);
            _clock.Tick(0.02f);
            Assert.IsFalse(_clock.IsActive);
            Assert.AreEqual(0f, _clock.Strength);
            Assert.IsFalse(_clock.InWarning);
        }

        [Test]
        public void ClearStopsItAtOnce()
        {
            _clock.Start(5f);
            _clock.Tick(1f);
            _clock.Clear();
            Assert.IsFalse(_clock.IsActive);
            Assert.AreEqual(0f, _clock.Strength);
        }

        [Test]
        public void StartingAgainRestartsTheFullLength()
        {
            _clock.Start(3f);
            _clock.Tick(2.5f);
            Assert.IsTrue(_clock.InWarning);
            _clock.Start(3f);
            Assert.IsFalse(_clock.InWarning);
            Assert.AreEqual(3f, _clock.Remaining, 0.0001f);
        }

        [Test]
        public void ABoostShorterThanTheWarningBlinksFromTheStart()
        {
            _clock.Start(0.5f);
            Assert.IsTrue(_clock.InWarning);
            Assert.AreEqual(1f, _clock.Strength);
            _clock.Tick(0.13f);
            Assert.AreEqual(0f, _clock.Strength);
        }

        [Test]
        public void AZeroLengthBoostNeverShows()
        {
            _clock.Start(0f);
            Assert.IsFalse(_clock.IsActive);
            Assert.AreEqual(0f, _clock.Strength);
        }
    }
}
