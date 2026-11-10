using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Director;

namespace UncannyValet.Tests.Director
{
    public class LotStateStationTests : TrackerTestBase
    {
        [Test]
        public void DirtyCarInWashSlot_WorkingThenBlockedThenFree()
        {
            Spawn(dirty: true);
            Bus.Publish(new CarArrived(Car));
            Bus.Publish(new CarEnteredStation(Car, StationKind.Wash));
            Assert.AreEqual(CarLotState.InStation, Only().State);
            Assert.AreEqual(StationSlotState.Working, Slot(StationKind.Wash, 0).State);
            Bus.Publish(new CarCleaned(Car));
            Assert.AreEqual(StationSlotState.Blocked, Slot(StationKind.Wash, 0).State);
            Bus.Publish(new CarLeftStation(Car, StationKind.Wash));
            Assert.AreEqual(StationSlotState.Free, Slot(StationKind.Wash, 0).State);
        }

        [Test]
        public void PlainCarInAChargingBayIsSquatting()
        {
            Spawn();
            Bus.Publish(new CarArrived(Car));
            Bus.Publish(new CarEnteredStation(Car, StationKind.Charging));
            Assert.AreEqual(StationSlotState.Squatted, Slot(StationKind.Charging, 0).State);
        }

        [Test]
        public void EvInABay_WorkingThenBlockedWhenFull()
        {
            Spawn(ev: true);
            Bus.Publish(new CarArrived(Car));
            Bus.Publish(new CarEnteredStation(Car, StationKind.Charging));
            Assert.AreEqual(StationSlotState.Working, Slot(StationKind.Charging, 0).State);
            Bus.Publish(new CarFullyCharged(Car));
            Assert.AreEqual(StationSlotState.Blocked, Slot(StationKind.Charging, 0).State);
        }

        [Test]
        public void DrivenCarThatNeedsTheWashReservesTheFreeSlot()
        {
            Spawn(dirty: true);
            Bus.Publish(new CarArrived(Car));
            Bus.Publish(new CarEntered(Car));
            Assert.AreEqual(StationSlotState.Reserved, Slot(StationKind.Wash, 0).State);
            Assert.AreEqual(StationSlotState.Free, Slot(StationKind.Charging, 0).State);
        }

        [Test]
        public void ClosedTicketRemovesTheCarAndEmptiesItsStation()
        {
            Spawn(dirty: true);
            Bus.Publish(new CarArrived(Car));
            Bus.Publish(new CarEnteredStation(Car, StationKind.Wash));
            Bus.Publish(new TicketClosed(Ticket, Car, 0f, 0));
            Assert.AreEqual(0, Tracker.Capture().Cars.Count);
            Assert.AreEqual(StationSlotState.Free, Slot(StationKind.Wash, 0).State);
        }

        [Test]
        public void ClosedWashSlotIsNotListed()
        {
            Bus.Publish(new ShiftPlanChanged(new ShiftPlan(3, 1f, 0f, 0f, true, false)));
            Assert.AreEqual(0, Tracker.Capture().CountSlots(StationKind.Wash, StationSlotState.Free));
            Assert.AreEqual(Bays, Tracker.Capture().CountSlots(StationKind.Charging, StationSlotState.Free));
        }
    }
}
