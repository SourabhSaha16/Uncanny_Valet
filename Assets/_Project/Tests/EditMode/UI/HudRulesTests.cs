using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using UncannyValet.UI;
using UncannyValet.UI.Hud;

namespace UncannyValet.Tests.UI
{
    public class HudRulesTests
    {
        [TestCase(72f, "1:12")]
        [TestCase(240f, "4:00")]
        [TestCase(0.2f, "0:01")]
        [TestCase(-5f, "0:00")]
        [TestCase(9f, "0:09")]
        public void ClockText_IsMinutesAndPaddedSeconds(float seconds, string expected)
        {
            var text = new StringBuilder();
            ClockText.Write(text, ClockText.WholeSeconds(seconds));
            Assert.AreEqual(expected, text.ToString());
        }

        [TestCase(120, AlarmLevel.Calm)]
        [TestCase(31, AlarmLevel.Calm)]
        [TestCase(30, AlarmLevel.Low)]
        [TestCase(11, AlarmLevel.Low)]
        [TestCase(10, AlarmLevel.Danger)]
        [TestCase(0, AlarmLevel.Danger)]
        public void Alarm_Clock_TurnsAmberThenOrangeRed(int seconds, AlarmLevel expected)
            => Assert.AreEqual(expected, Alarm.ForClock(seconds));

        [Test]
        public void Alarm_Patience_TurnsAmberThenOrangeRed()
        {
            Assert.AreEqual(AlarmLevel.Calm, Alarm.ForPatience(0.3f, false));
            Assert.AreEqual(AlarmLevel.Low, Alarm.ForPatience(0.7f, false));
            Assert.AreEqual(AlarmLevel.Danger, Alarm.ForPatience(0.9f, false));
            Assert.AreEqual(AlarmLevel.Danger, Alarm.ForPatience(0.1f, true));
        }

        [Test]
        public void Alarm_PatienceFill_ShrinksAsTimeRunsOut()
        {
            Assert.AreEqual(40, Alarm.PatienceFill(0f, 40));
            Assert.AreEqual(20, Alarm.PatienceFill(0.5f, 40));
            Assert.AreEqual(0, Alarm.PatienceFill(1f, 40));
            Assert.AreEqual(0, Alarm.PatienceFill(3f, 40));
            Assert.AreEqual(40, Alarm.PatienceFill(-1f, 40));
        }

        [Test]
        public void StarBar_FillsTowardTheThreeStarScoreAndLightsStars()
        {
            var bar = new StarBarLayout(300, 150, 300, 450, 90);
            Assert.AreEqual(60, bar.Fill);
            Assert.AreEqual(2, bar.LitStars);
            Assert.AreEqual(30, bar.MarkerX(0));
            Assert.AreEqual(60, bar.MarkerX(1));
            Assert.AreEqual(90, bar.MarkerX(2));
            Assert.IsTrue(bar.IsLit(1));
            Assert.IsFalse(bar.IsLit(2));
        }

        [Test]
        public void StarBar_CapsAtFullAndSurvivesBadThresholds()
        {
            Assert.AreEqual(90, new StarBarLayout(9999, 150, 300, 450, 90).Fill);
            Assert.AreEqual(0, new StarBarLayout(-20, 150, 300, 450, 90).Fill);
            Assert.AreEqual(0, new StarBarLayout(0, 0, 0, 0, 90).Fill);
        }

        [Test]
        public void ComboWobble_ShakesThenSettlesToZero()
        {
            Assert.AreEqual(0, ComboWobble.OffsetY(0f));
            Assert.AreNotEqual(0, ComboWobble.OffsetY(0.0625f));
            Assert.AreEqual(0, ComboWobble.OffsetY(ComboWobble.DurationSeconds));
            Assert.AreEqual(0, ComboWobble.OffsetY(-1f));
        }

        private static TicketRow Row(int car, TicketRowState state, bool timer = false, float elapsed = 0f)
        {
            var row = new TicketRow { Car = car, State = state };
            if (timer) row.StartTimer(UncannyValet.Core.Events.TimerKind.Pickup, elapsed);
            return row;
        }

        [Test]
        public void TicketOrder_PickupsFirst_ThenLeastPatience_ThenCar()
        {
            var rows = new List<TicketRow>
            {
                Row(1, TicketRowState.Parked),
                Row(2, TicketRowState.AwaitingPark, true, 0.4f),
                Row(3, TicketRowState.AwaitingPark, true, 0.9f),
                Row(4, TicketRowState.PickupRequested),
                Row(5, TicketRowState.Parked),
            };
            TicketRowOrder.Sort(rows);
            Assert.AreEqual(new[] { 4, 3, 2, 1, 5 }, new[] { rows[0].Car, rows[1].Car, rows[2].Car, rows[3].Car, rows[4].Car });
        }

        [Test]
        public void TicketOrder_ExpiredPickupBeatsFreshPickup()
        {
            var fresh = Row(1, TicketRowState.PickupRequested);
            var late = Row(2, TicketRowState.PickupRequested);
            late.Expired = true;
            Assert.Less(TicketRowOrder.Compare(late, fresh), 0);
        }

        [Test]
        public void WorldTicketLayout_GrowsWithLinesAndStaysOnCanvas()
        {
            Assert.AreEqual(WorldTicketLayout.Height(1), WorldTicketLayout.Height(0));
            Assert.Greater(WorldTicketLayout.Height(2), WorldTicketLayout.Height(1));
            Assert.AreEqual(0, WorldTicketLayout.MeterFill(-1f));
            Assert.AreEqual(WorldTicketLayout.MeterWidth, WorldTicketLayout.MeterFill(3f));
            Assert.AreEqual(WorldTicketLayout.MeterWidth / 2, WorldTicketLayout.MeterFill(0.5f));
            Assert.AreEqual(WorldTicketLayout.ScreenMargin, WorldTicketLayout.ClampToCanvas(-50, 20, 480));
            Assert.AreEqual(480 - WorldTicketLayout.ScreenMargin - 20, WorldTicketLayout.ClampToCanvas(500, 20, 480));
            Assert.AreEqual(100, WorldTicketLayout.ClampToCanvas(100, 20, 480));
        }
    }
}
