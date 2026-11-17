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

        [TestCase(0, 30, 0, false)]
        [TestCase(15, 30, 25, false)]
        [TestCase(29, 30, 48, false)]
        [TestCase(30, 30, 50, true)]
        [TestCase(400, 30, 50, true)]
        [TestCase(-5, 30, 0, false)]
        public void BlessingBar_FillsTowardTheThresholdAtTheRightEnd(int blessings, int threshold, int fill, bool reached)
        {
            var bar = new BlessingBarLayout(blessings, threshold, 50);
            Assert.AreEqual(fill, bar.Fill);
            Assert.AreEqual(reached, bar.Reached);
        }

        [Test]
        public void BlessingBar_SurvivesABadThreshold()
        {
            Assert.AreEqual(0, new BlessingBarLayout(0, 0, 50).Fill);
            Assert.AreEqual(50, new BlessingBarLayout(3, 0, 50).Fill);
        }

        [Test]
        public void BlessingPulse_StartsBrightAndFadesOut()
        {
            Assert.AreEqual(1f, BlessingPulse.Strength(0f), 1e-4f);
            Assert.AreEqual(0.5f, BlessingPulse.Strength(BlessingPulse.DurationSeconds * 0.5f), 1e-4f);
            Assert.AreEqual(0f, BlessingPulse.Strength(BlessingPulse.DurationSeconds));
            Assert.AreEqual(0f, BlessingPulse.Strength(-1f));
        }

        [Test]
        public void FavourWobble_ShakesThenSettlesToZero()
        {
            Assert.AreEqual(0, FavourWobble.OffsetY(0f));
            Assert.AreNotEqual(0, FavourWobble.OffsetY(0.0625f));
            Assert.AreEqual(0, FavourWobble.OffsetY(FavourWobble.DurationSeconds));
            Assert.AreEqual(0, FavourWobble.OffsetY(-1f));
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
        public void WorldTicketLayout_KeepsBoxesOnCanvas()
        {
            Assert.AreEqual(WorldTicketLayout.ScreenMargin, WorldTicketLayout.ClampToCanvas(-50, 20, 480));
            Assert.AreEqual(480 - WorldTicketLayout.ScreenMargin - 20, WorldTicketLayout.ClampToCanvas(500, 20, 480));
            Assert.AreEqual(100, WorldTicketLayout.ClampToCanvas(100, 20, 480));
        }

        [Test]
        public void PlateLayout_ServiceIconSitsRightOfThePlateAndCentred()
        {
            Assert.AreEqual(50 + 20 + PlateLayout.IconGap, PlateLayout.IconX(50, 20));
            Assert.AreEqual(10 + (PlateLayout.Height - 7) / 2, PlateLayout.IconY(10, 7));
        }
    }
}
