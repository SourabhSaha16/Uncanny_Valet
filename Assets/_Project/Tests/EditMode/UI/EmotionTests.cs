using NUnit.Framework;
using UncannyValet.UI.Mood;
using UnityEngine;

namespace UncannyValet.Tests.UI
{
    public class EmotionTests
    {
        private readonly EmotionRule _rule = new EmotionRule(0.6f, 0.8f, 1f);

        [TestCase(0f, Emotion.Happy)]
        [TestCase(0.59f, Emotion.Happy)]
        [TestCase(0.6f, Emotion.Impatient)]
        [TestCase(0.79f, Emotion.Impatient)]
        [TestCase(0.8f, Emotion.Angry)]
        [TestCase(0.99f, Emotion.Angry)]
        [TestCase(1f, Emotion.WillNotTip)]
        public void PatienceMapsToAnEmotion(float elapsed, Emotion expected)
            => Assert.AreEqual(expected, _rule.Resolve(elapsed, false));

        [Test]
        public void ExpiredIsAlwaysWillNotTip() => Assert.AreEqual(Emotion.WillNotTip, _rule.Resolve(0f, true));

        [Test]
        public void LevelsCanBeRetunedLive()
        {
            var rule = new EmotionRule(0.6f, 0.8f, 1f);
            rule.SetLevels(0.7f, 0.9f);
            Assert.AreEqual(Emotion.Angry, rule.Resolve(0.7f, false));
            Assert.AreEqual(Emotion.Angry, rule.Resolve(0.89f, false));
            Assert.AreEqual(Emotion.WillNotTip, rule.Resolve(0.9f, false));
        }

        [Test]
        public void WillNotTipLevelBelowAngryIsRaisedToIt()
        {
            var rule = new EmotionRule(0.6f, 0.8f, 0.5f);
            Assert.AreEqual(Emotion.Impatient, rule.Resolve(0.79f, false));
            Assert.AreEqual(Emotion.WillNotTip, rule.Resolve(0.8f, false));
        }

        [Test]
        public void AngryThresholdBelowImpatientIsRaisedToIt()
        {
            var rule = new EmotionRule(0.7f, 0.2f);
            Assert.AreEqual(Emotion.Happy, rule.Resolve(0.3f, false));
            Assert.AreEqual(Emotion.Angry, rule.Resolve(0.7f, false));
        }

        [Test]
        public void DefaultsMatchTheSpecifiedFlashRatesAndColours()
        {
            var config = ScriptableObject.CreateInstance<HudEmotionConfig>();
            var style = new EmotionStyle(config);
            Assert.AreEqual(1f / 1.5f, style.PeriodOf(Emotion.Happy), 1e-4f);
            Assert.AreEqual(1f / 3f, style.PeriodOf(Emotion.Impatient), 1e-4f);
            Assert.AreEqual(1f / 7f, style.PeriodOf(Emotion.Angry), 1e-4f);
            Assert.AreEqual(config.HappyColor, style.ColorOf(Emotion.Happy));
            Assert.Greater(style.ColorOf(Emotion.Happy).g, style.ColorOf(Emotion.Happy).r);
            Assert.Greater(style.ColorOf(Emotion.Angry).r, style.ColorOf(Emotion.Angry).g);
            Object.DestroyImmediate(config);
        }

        [TestCase(Emotion.Happy)]
        [TestCase(Emotion.Impatient)]
        [TestCase(Emotion.Angry)]
        [TestCase(Emotion.WillNotTip)]
        public void EveryBubbleFaceHasTheFullGrid(Emotion emotion)
        {
            var face = MoodBubbleLayout.Face(emotion);
            Assert.AreEqual(MoodBubbleLayout.FaceHeight, face.Length);
            foreach (var row in face) Assert.AreEqual(MoodBubbleLayout.FaceWidth, row.Length);
        }

        [Test]
        public void TheThreeFacesDiffer()
        {
            Assert.AreNotEqual(MoodBubbleLayout.Face(Emotion.Happy), MoodBubbleLayout.Face(Emotion.Impatient));
            Assert.AreNotEqual(MoodBubbleLayout.Face(Emotion.Impatient), MoodBubbleLayout.Face(Emotion.Angry));
            Assert.AreNotEqual(MoodBubbleLayout.Face(Emotion.Angry), MoodBubbleLayout.Face(Emotion.WillNotTip));
        }

        [Test]
        public void CustAngryStatusFollowsTheAngryEmotion()
        {
            var rule = new UncannyValet.UI.Status.CustAngryStatusRule(_rule);
            var row = new UncannyValet.UI.TicketRow { State = UncannyValet.UI.TicketRowState.PickupRequested, Elapsed01 = 0.9f };
            Assert.IsTrue(rule.Applies(row));
            row.Elapsed01 = 0.7f;
            Assert.IsFalse(rule.Applies(row));
            row.Elapsed01 = 1f;
            Assert.IsFalse(rule.Applies(row), "a patron who gave up is not merely angry");
        }

        [Test]
        public void WillNotTipStatusFollowsTheGivenUpEmotion()
        {
            var rule = new UncannyValet.UI.Status.WillNotTipStatusRule(_rule);
            var row = new UncannyValet.UI.TicketRow { State = UncannyValet.UI.TicketRowState.PickupRequested, Elapsed01 = 0.95f };
            Assert.IsFalse(rule.Applies(row));
            row.Elapsed01 = 1f;
            Assert.IsTrue(rule.Applies(row));
            row.Elapsed01 = 0.2f;
            row.Expired = true;
            Assert.IsTrue(rule.Applies(row));
        }
    }
}
