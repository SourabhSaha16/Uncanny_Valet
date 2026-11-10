using NUnit.Framework;
using UncannyValet.UI;
using UncannyValet.UI.Hud;
using UncannyValet.UI.Mood;
using UnityEngine;

namespace UncannyValet.Tests.UI
{
    public class PickupMarkTests
    {
        private HudEmotionConfig _config;
        private EmotionStyle _style;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<HudEmotionConfig>();
            _style = new EmotionStyle(_config);
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_config);

        private static TicketRow Row(TicketRowState state, float elapsed, bool expired = false)
            => new TicketRow { State = state, Elapsed01 = elapsed, Expired = expired };

        [Test]
        public void OnlyPickupRowsCarryTheMark()
        {
            Assert.IsTrue(PickupMark.IsRequested(Row(TicketRowState.PickupRequested, 0f)));
            Assert.IsFalse(PickupMark.IsRequested(Row(TicketRowState.Parked, 0f)));
            Assert.IsFalse(PickupMark.IsRequested(Row(TicketRowState.AwaitingPark, 0f)));
        }

        [Test]
        public void FlashSpeedFollowsTheEmotion()
        {
            var happy = Row(TicketRowState.PickupRequested, 0f);
            var angry = Row(TicketRowState.PickupRequested, 1f);
            Assert.IsTrue(PickupMark.IsLit(happy, 0.1f, _style));
            Assert.IsFalse(PickupMark.IsLit(happy, 0.5f, _style)); // 1.5 Hz: period 0.67 s, lit for the first 0.33 s
            Assert.IsFalse(PickupMark.IsLit(angry, 0.1f, _style)); // 7 Hz: dark again after 0.07 s
        }

        [Test]
        public void TintIsTheEmotionColour()
        {
            Assert.AreEqual(_config.HappyColor, PickupMark.Tint(Row(TicketRowState.PickupRequested, 0.1f), _style));
            Assert.AreEqual(_config.ImpatientColor, PickupMark.Tint(Row(TicketRowState.PickupRequested, 0.7f), _style));
            Assert.AreEqual(_config.AngryColor, PickupMark.Tint(Row(TicketRowState.PickupRequested, 0.3f, true), _style));
        }
    }
}
