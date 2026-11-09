using NUnit.Framework;
using UncannyValet.UI;
using UncannyValet.UI.Hud;
using UncannyValet.UI.Kit;

namespace UncannyValet.Tests.UI
{
    public class PickupMarkTests
    {
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
        public void FlashFollowsThePlateUrgencyPeriod()
        {
            var calm = Row(TicketRowState.PickupRequested, 0f);
            var urgent = Row(TicketRowState.PickupRequested, 1f);
            Assert.IsTrue(PickupMark.IsLit(calm, 0.1f));
            Assert.IsFalse(PickupMark.IsLit(calm, 0.6f)); // slow period 1 s: lit for the first half
            Assert.IsFalse(PickupMark.IsLit(urgent, 0.1f)); // fast period 1/6 s: dark again after 1/12 s
        }

        [Test]
        public void TintFollowsTheAlarmColours()
        {
            Assert.AreEqual(PixelPalette.Cyan, PickupMark.Tint(Row(TicketRowState.PickupRequested, 0.1f)));
            Assert.AreEqual(PixelPalette.Amber, PickupMark.Tint(Row(TicketRowState.PickupRequested, 0.7f)));
            Assert.AreEqual(PixelPalette.OrangeRed, PickupMark.Tint(Row(TicketRowState.PickupRequested, 0.3f, true)));
        }
    }
}
