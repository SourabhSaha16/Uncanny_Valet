using NUnit.Framework;
using UncannyValet.UI;
using UncannyValet.UI.Hud;

namespace UncannyValet.Tests.UI
{
    public class PickupMarkTests
    {
        private static TicketRow Row(TicketRowState state) => new TicketRow { State = state };

        [Test]
        public void OnlyPickupRowsCarryTheMark()
        {
            Assert.IsTrue(PickupMark.IsRequested(Row(TicketRowState.PickupRequested)));
            Assert.IsFalse(PickupMark.IsRequested(Row(TicketRowState.Parked)));
            Assert.IsFalse(PickupMark.IsRequested(Row(TicketRowState.AwaitingPark)));
        }
    }
}
