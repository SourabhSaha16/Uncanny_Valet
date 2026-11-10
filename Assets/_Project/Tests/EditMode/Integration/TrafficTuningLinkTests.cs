using NUnit.Framework;
using UncannyValet.Bootstrap;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Tests.Integration
{
    public class TrafficTuningLinkTests
    {
        private EventBus _bus;
        private float _rate;
        private float _scale;

        [SetUp]
        public void SetUp()
        {
            _bus = new EventBus();
            _rate = -1f;
            _scale = -1f;
        }

        [Test]
        public void ForwardsTheAmbientRateAndTheSpeedScale()
        {
            using (new TrafficTuningLink(_bus, r => _rate = r, s => _scale = s))
            {
                _bus.Publish(new TuningChanged(TuningKey.TrafficAmbientPerMinute, 12f));
                _bus.Publish(new TuningChanged(TuningKey.TrafficSpeedScale, 1.5f));
            }
            Assert.AreEqual(12f, _rate);
            Assert.AreEqual(1.5f, _scale);
        }

        [Test]
        public void ForwardsTheEntryAndExitSpeeds()
        {
            float entry = -1f;
            float exit = -1f;
            using (new TrafficTuningLink(_bus, r => _rate = r, s => _scale = s, v => entry = v, v => exit = v))
            {
                _bus.Publish(new TuningChanged(TuningKey.TrafficEntrySpeed, 7f));
                _bus.Publish(new TuningChanged(TuningKey.TrafficExitSpeed, 3.5f));
            }
            Assert.AreEqual(7f, entry);
            Assert.AreEqual(3.5f, exit);
            Assert.AreEqual(-1f, _rate);
            Assert.AreEqual(-1f, _scale);
        }

        [Test]
        public void EntryAndExitKeysAreIgnoredWithoutSetters()
        {
            using (new TrafficTuningLink(_bus, r => _rate = r, s => _scale = s))
            {
                Assert.DoesNotThrow(() => _bus.Publish(new TuningChanged(TuningKey.TrafficEntrySpeed, 7f)));
                Assert.DoesNotThrow(() => _bus.Publish(new TuningChanged(TuningKey.TrafficExitSpeed, 7f)));
            }
        }

        [Test]
        public void IgnoresOtherKeysAndStopsAfterDispose()
        {
            var link = new TrafficTuningLink(_bus, r => _rate = r, s => _scale = s);
            _bus.Publish(new TuningChanged(TuningKey.BaseTip, 5f));
            link.Dispose();
            _bus.Publish(new TuningChanged(TuningKey.TrafficAmbientPerMinute, 3f));
            Assert.AreEqual(-1f, _rate);
            Assert.AreEqual(-1f, _scale);
        }
    }
}
