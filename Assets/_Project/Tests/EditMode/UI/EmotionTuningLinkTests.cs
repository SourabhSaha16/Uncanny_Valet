using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.UI.Mood;

namespace UncannyValet.Tests.UI
{
    public class EmotionTuningLinkTests
    {
        private EventBus _bus;
        private TuningStore _tuning;
        private EmotionRule _rule;

        [SetUp]
        public void SetUp()
        {
            _bus = new EventBus();
            _tuning = new TuningStore(_bus);
            _rule = new EmotionRule(0.6f, 0.8f, 1f);
        }

        [Test]
        public void UnseededKeysKeepTheFallbackLevels()
        {
            using (new EmotionTuningLink(_bus, _tuning, _rule, 0.8f, 1f))
                Assert.AreEqual(Emotion.Angry, _rule.Resolve(0.8f, false));
        }

        [Test]
        public void SeededLevelsApplyAndLiveChangesFollow()
        {
            _tuning.Seed(TuningKey.EmotionAngryFrom, 0.7f);
            _tuning.Seed(TuningKey.EmotionWillNotBlessFrom, 0.95f);
            using (new EmotionTuningLink(_bus, _tuning, _rule, 0.8f, 1f))
            {
                Assert.AreEqual(Emotion.Angry, _rule.Resolve(0.7f, false));
                Assert.AreEqual(Emotion.WillNotBless, _rule.Resolve(0.95f, false));
                _tuning.Set(TuningKey.EmotionWillNotBlessFrom, 1f);
                Assert.AreEqual(Emotion.Angry, _rule.Resolve(0.95f, false));
            }
        }

        [Test]
        public void EmotionRowsAreInTheCatalog()
        {
            foreach (var row in EmotionRows.Rows)
                Assert.IsTrue(row.Advanced && row.Label.Length > 0, row.Key.ToString());
            Assert.AreEqual(2, EmotionRows.Rows.Length);
        }
    }
}
