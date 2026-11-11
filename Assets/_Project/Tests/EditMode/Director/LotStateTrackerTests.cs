using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Director;

namespace UncannyValet.Tests.Director
{
    public class LotStateTrackerTests : TrackerTestBase
    {
        [Test]
        public void NewCarIsArrivingThenWaitingToPark()
        {
            Spawn();
            Assert.AreEqual(CarLotState.Arriving, Only().State);
            Bus.Publish(new CarArrived(Car));
            Assert.AreEqual(CarLotState.WaitingToPark, Only().State);
            Assert.IsTrue(Only().AwaitingPark);
        }

        [Test]
        public void ValetInTheCarMakesItDriven_ExitingReturnsItToWaiting()
        {
            Spawn();
            Bus.Publish(new CarArrived(Car));
            Bus.Publish(new CarEntered(Car));
            Assert.AreEqual(CarLotState.Driven, Only().State);
            Bus.Publish(new CarExited(Car));
            Assert.AreEqual(CarLotState.WaitingToPark, Only().State);
        }

        [Test]
        public void ParkingTakesASpot_LeavingItKeepsTheCarParkedAndFreesTheSpot()
        {
            Spawn();
            Bus.Publish(new CarArrived(Car));
            Bus.Publish(new CarParked(Car, 3));
            var snap = Tracker.Capture();
            Assert.AreEqual(CarLotState.Parked, snap.Cars[0].State);
            Assert.IsFalse(snap.Cars[0].AwaitingPark);
            Assert.AreEqual(RegularSpots - 1, snap.FreeSpots);
            Bus.Publish(new CarLeftSpot(Car, 3));
            Assert.AreEqual(CarLotState.Parked, Only().State);
            Assert.AreEqual(RegularSpots, Tracker.Capture().FreeSpots);
        }

        [Test]
        public void ChargingBaySpotDoesNotUseARegularSpot()
        {
            Spawn(ev: true);
            Bus.Publish(new CarArrived(Car));
            Bus.Publish(new CarParked(Car, RegularSpots + 1));
            Assert.AreEqual(RegularSpots, Tracker.Capture().FreeSpots);
        }

        [Test]
        public void TotalSpots_CountOnlyTheSixRegularSpots_NotTheStations()
        {
            var snap = Tracker.Capture();
            Assert.AreEqual(6, RegularSpots);
            Assert.AreEqual(RegularSpots, snap.TotalSpots);
            Assert.AreEqual(RegularSpots, snap.FreeSpots);
            Bus.Publish(new ShiftPlanChanged(new ShiftPlan(3, 1f, 0f, 0f, true, true, 0)));
            Assert.AreEqual(RegularSpots, Tracker.Capture().TotalSpots, "a plan of 0 opens every regular spot");
        }

        [Test]
        public void PlanLimitsTotalSpots()
        {
            Bus.Publish(new ShiftPlanChanged(new ShiftPlan(3, 1f, 0f, 0f, true, true, 4)));
            var snap = Tracker.Capture();
            Assert.AreEqual(4, snap.TotalSpots);
            Assert.AreEqual(4, snap.FreeSpots);
        }

        [Test]
        public void RequestedPickupBeatsEveryOtherState()
        {
            Spawn();
            Bus.Publish(new CarArrived(Car));
            Bus.Publish(new CarParked(Car, 2));
            Bus.Publish(new PickupRequested(Ticket, Car));
            Bus.Publish(new CarEntered(Car));
            Assert.AreEqual(CarLotState.PickupRequested, Only().State);
        }

        [Test]
        public void PatienceFollowsTheRunningTimer()
        {
            Spawn();
            Bus.Publish(new CarArrived(Car));
            Assert.AreEqual(1f, Only().Patience01, 0.001f);
            Bus.Publish(new CarParked(Car, 1));
            Assert.AreEqual(1f, Only().Patience01, 0.001f);
            Bus.Publish(new PickupRequested(Ticket, Car));
            Bus.Publish(new TicketTimerChanged(Ticket, Car, TimerKind.Pickup, 0.9f));
            Assert.AreEqual(0.1f, Only().Patience01, 0.001f);
        }

        [Test]
        public void SecondsToNextPickupCountsDownFromTheStay()
        {
            Spawn(stay: 30f);
            Bus.Publish(new CarArrived(Car));
            Clock.SetElapsed(10f);
            Bus.Publish(new CarParked(Car, 1));
            Clock.SetElapsed(20f);
            Assert.AreEqual(20f, Tracker.Capture().SecondsToNextPickup, 0.01f);
            Assert.AreEqual(20f, Only().SecondsToPickup, 0.01f);
        }

        [Test]
        public void AMovedReturnShowsInTheCountdownAndTheShiftedAmount()
        {
            Spawn(stay: 30f);
            Bus.Publish(new CarArrived(Car));
            Clock.SetElapsed(10f);
            Bus.Publish(new CarParked(Car, 1));
            Bus.Publish(new PatronReturnShifted(Ticket, Car, -6f));
            Clock.SetElapsed(20f);
            Assert.AreEqual(14f, Only().SecondsToPickup, 0.01f);
            Assert.AreEqual(-6f, Only().ReturnShiftSeconds, 0.01f);
            Assert.AreEqual(30f, Only().PlannedStaySeconds, 0.01f, "the planned stay is not changed by the move");
        }

        [Test]
        public void AngryPatronsAreCountedInsideTheWindow()
        {
            Clock.SetElapsed(10f);
            Bus.Publish(new ComplaintFiled(Ticket, 15));
            Clock.SetElapsed(50f);
            Bus.Publish(new ComplaintFiled(Ticket, 15));
            var snap = Tracker.Capture();
            Assert.AreEqual(1, snap.CountAngry(30f));
            Assert.AreEqual(2, snap.CountAngry(60f));
        }

        [Test]
        public void NewShiftClearsEverything()
        {
            Spawn();
            Bus.Publish(new ComplaintFiled(Ticket, 15));
            Bus.Publish(new ShiftStarted());
            var snap = Tracker.Capture();
            Assert.AreEqual(0, snap.Cars.Count);
            Assert.AreEqual(0, snap.CountAngry(1000f));
        }

        [Test]
        public void EveryCarIsInExactlyOneState()
        {
            Spawn();
            Assert.AreEqual(1, Tracker.Capture().CountCars(CarLotState.Arriving));
            Bus.Publish(new CarArrived(Car));
            var snap = Tracker.Capture();
            var total = 0;
            foreach (CarLotState state in System.Enum.GetValues(typeof(CarLotState))) total += snap.CountCars(state);
            Assert.AreEqual(1, total);
        }
    }
}
