using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Core.Modifiers;
using UncannyValet.Director;

namespace UncannyValet.Tests.Director
{
    public class LotStateStationTests : TrackerTestBase
    {
        [Test]
        public void MuddyCarInWashSlot_WorkingThenBlockedThenFree()
        {
            Spawn(muddy: true);
            Bus.Publish(new CarArrived(Car));
            Bus.Publish(new CarEnteredStation(Car, StationKind.Wash));
            Assert.AreEqual(CarLotState.InStation, Only().State);
            Assert.AreEqual(StationSlotState.Working, Slot(StationKind.Wash, 0).State);
            Bus.Publish(new ModifierCleared(Car, ModifierId.Muddy));
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
            Bus.Publish(new ModifierCleared(Car, ModifierId.NeedsCharge));
            Assert.AreEqual(StationSlotState.Blocked, Slot(StationKind.Charging, 0).State);
        }

        [Test]
        public void DrivenCarThatNeedsTheWashReservesTheFreeSlot()
        {
            Spawn(muddy: true);
            Bus.Publish(new CarArrived(Car));
            Bus.Publish(new CarEntered(Car));
            Assert.AreEqual(StationSlotState.Reserved, Slot(StationKind.Wash, 0).State);
            Assert.AreEqual(StationSlotState.Free, Slot(StationKind.Charging, 0).State);
        }

        [Test]
        public void ClosedTicketRemovesTheCarAndEmptiesItsStation()
        {
            Spawn(muddy: true);
            Bus.Publish(new CarArrived(Car));
            Bus.Publish(new CarEnteredStation(Car, StationKind.Wash));
            Bus.Publish(new TicketClosed(Ticket, Car, 0f, 0));
            Assert.AreEqual(0, Tracker.Capture().Cars.Count);
            Assert.AreEqual(StationSlotState.Free, Slot(StationKind.Wash, 0).State);
        }

        [Test]
        public void ClosedWashSlotIsNotListed()
        {
            Bus.Publish(new ShiftPlanChanged(new ShiftPlan(3, 1f, true, false)));
            Assert.AreEqual(0, Tracker.Capture().CountSlots(StationKind.Wash, StationSlotState.Free));
            Assert.AreEqual(Bays, Tracker.Capture().CountSlots(StationKind.Charging, StationSlotState.Free));
        }

        [Test]
        public void CarHoldingTwoModifiers_IsTrackedPerModifierAndPerStation()
        {
            Spawn(ev: true, muddy: true);
            Bus.Publish(new ModifierProgressChanged(Car, ModifierId.NeedsCharge, 0.2f));
            Bus.Publish(new CarArrived(Car));
            var open = Only();
            Assert.AreEqual(ModifierSet.Of(ModifierId.Muddy, ModifierId.NeedsCharge), open.Held);
            Assert.AreEqual(2, open.Open.Count);
            Assert.IsTrue(open.NeedsService);
            Assert.IsTrue(open.NeedsAt(StationKind.Wash));
            Assert.IsTrue(open.NeedsAt(StationKind.Charging));
            Assert.AreEqual(1f, open.LeftAt(StationKind.Wash), 1e-5f);
            Assert.AreEqual(0.8f, open.LeftAt(StationKind.Charging), 1e-5f);

            Bus.Publish(new CarEnteredStation(Car, StationKind.Wash));
            Bus.Publish(new ModifierProgressChanged(Car, ModifierId.Muddy, 0.5f));
            Assert.AreEqual(0.5f, Only().LeftAt(StationKind.Wash), 1e-5f);
            Bus.Publish(new ModifierCleared(Car, ModifierId.Muddy));
            var half = Only();
            Assert.AreEqual(ModifierSet.Of(ModifierId.NeedsCharge), half.Open);
            Assert.IsFalse(half.NeedsAt(StationKind.Wash));
            Assert.IsTrue(half.NeedsService, "the charge is still open");
            Assert.AreEqual(StationSlotState.Blocked, Slot(StationKind.Wash, 0).State, "washed but still holding the slot");

            Bus.Publish(new ModifierCleared(Car, ModifierId.NeedsCharge));
            Assert.IsFalse(Only().NeedsService);
        }

        [Test]
        public void AMuddyCarSquatsAChargingBay_AndAPlainOneSquatsTheWash()
        {
            Spawn(muddy: true);
            Bus.Publish(new CarArrived(Car));
            Bus.Publish(new CarEnteredStation(Car, StationKind.Charging));
            Assert.AreEqual(StationSlotState.Squatted, Slot(StationKind.Charging, 0).State);
        }
    }
}
