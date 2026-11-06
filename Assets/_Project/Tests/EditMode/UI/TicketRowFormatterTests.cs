using NUnit.Framework;
using UncannyValet.Core.Events;
using UncannyValet.UI;

namespace UncannyValet.Tests.UI
{
    public class TicketRowFormatterTests
    {
        private static TicketRow Row(char letter, TicketRowState state, int spot = 0) =>
            new TicketRow { Letter = letter, State = state, Spot = spot };

        [Test]
        public void Waiting_NoTimer() =>
            Assert.AreEqual("  A  Waiting", TicketRowFormatter.Format(Row('A', TicketRowState.AwaitingPark)));

        [Test]
        public void ParkTimer_ShowsPercentLeft()
        {
            var row = Row('A', TicketRowState.AwaitingPark, 3);
            row.StartTimer(TimerKind.Park, 0.69f);
            Assert.AreEqual("  A  [Park 31%]  (spot 3)", TicketRowFormatter.Format(row));
        }

        [Test]
        public void Parked_ShowsSpot() =>
            Assert.AreEqual("  B  Parked  in spot 3", TicketRowFormatter.Format(Row('B', TicketRowState.Parked, 3)));

        [Test]
        public void PickupTimer_ShowsPercentLeft()
        {
            var row = Row('C', TicketRowState.PickupRequested, 5);
            row.StartTimer(TimerKind.Pickup, 0.2f);
            Assert.AreEqual("  C  [Pickup 80%]  in spot 5", TicketRowFormatter.Format(row));
        }

        [Test]
        public void ExpiredPickup_IsLate()
        {
            var row = Row('C', TicketRowState.PickupRequested);
            row.StartTimer(TimerKind.Pickup, 1f);
            row.Expired = true;
            Assert.AreEqual("  C  LATE: tip $0", TicketRowFormatter.Format(row));
        }

        [Test]
        public void PercentLeft_ClampsAndRounds()
        {
            Assert.AreEqual(100, TicketRowFormatter.PercentLeft(-1f));
            Assert.AreEqual(0, TicketRowFormatter.PercentLeft(2f));
            Assert.AreEqual(31, TicketRowFormatter.PercentLeft(0.69f));
        }
    }
}
