using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Director;

namespace UncannyValet.Tests.Director
{
    public class WorkloadEstimatorTests
    {
        // Default cost table: park 8, pickup 10, wash trip 6 + 15 s of wash, charge trip 6, evict 6, soon 5 within 20 s;
        // horizon 30 s.
        private readonly WorkloadSettings _s = new WorkloadSettings();
        private SnapshotBuilderForTests _b;

        [SetUp]
        public void SetUp() => _b = new SnapshotBuilderForTests();

        private WorkloadReading Estimate() => WorkloadEstimator.Estimate(_b.Build(), _s);

        [Test]
        public void EmptyLotHasNoWorkAndNoPressure()
        {
            var r = Estimate();
            Assert.AreEqual(0f, r.W);
            Assert.AreEqual(0f, r.P);
        }

        [Test]
        public void WaitingCarCostsAParkingTrip()
        {
            _b.Car(CarLotState.WaitingToPark, awaitingPark: true);
            Assert.AreEqual(8f, Estimate().Seconds, 0.001f);
        }

        [Test]
        public void RequestedPickupCostsADelivery()
        {
            _b.Car(CarLotState.PickupRequested);
            Assert.AreEqual(10f, Estimate().Seconds, 0.001f);
        }

        [Test]
        public void ParkedCarWithPickupDueSoonCountsAsWorkAboutToLand()
        {
            _b.Car(CarLotState.Parked, toPickup: 15f);
            _b.Car(CarLotState.Parked, toPickup: 25f);
            Assert.AreEqual(5f, Estimate().Seconds, 0.001f);
        }

        [Test]
        public void DirtyCarOwesTheTripAndTheWashLeft()
        {
            _b.Car(CarLotState.Parked, dirty: true, washLeft: 1f);
            Assert.AreEqual(6f + 15f, Estimate().Seconds, 0.001f);
        }

        [Test]
        public void CarAlreadyInTheWashSlotOwesOnlyTheWashLeft()
        {
            var car = _b.Car(CarLotState.InStation, dirty: true, washLeft: 0.5f);
            _b.Slot(StationKind.Wash, StationSlotState.Working, car);
            Assert.AreEqual(7.5f, Estimate().Seconds, 0.001f);
        }

        [Test]
        public void EvOwesTheTripToABayButNotOnceItIsCharging()
        {
            _b.Car(CarLotState.Parked, ev: true);
            Assert.AreEqual(6f, Estimate().Seconds, 0.001f);
            _b = new SnapshotBuilderForTests();
            var car = _b.Car(CarLotState.InStation, ev: true);
            _b.Slot(StationKind.Charging, StationSlotState.Working, car);
            Assert.AreEqual(0f, Estimate().Seconds, 0.001f);
        }

        [Test]
        public void FinishedCarBlockingAStationSomeoneNeedsCostsAnEviction()
        {
            var clean = _b.Car(CarLotState.InStation, dirty: true, washDone: true);
            _b.Slot(StationKind.Wash, StationSlotState.Blocked, clean);
            _b.Car(CarLotState.Parked, dirty: true, washLeft: 1f);
            Assert.AreEqual(6f + 6f + 15f, Estimate().Seconds, 0.001f);
        }

        [Test]
        public void BlockedStationNobodyNeedsCostsNothing()
        {
            var clean = _b.Car(CarLotState.InStation, dirty: true, washDone: true);
            _b.Slot(StationKind.Wash, StationSlotState.Blocked, clean);
            Assert.AreEqual(0f, Estimate().Seconds, 0.001f);
        }

        [Test]
        public void SquattingCarCostsAnEvictionWhenAnEvNeedsTheOnlyBay()
        {
            var squatter = _b.Car(CarLotState.InStation);
            _b.Slot(StationKind.Charging, StationSlotState.Squatted, squatter);
            _b.Car(CarLotState.Parked, ev: true);
            Assert.AreEqual(6f + 6f, Estimate().Seconds, 0.001f);
        }

        [Test]
        public void WorkloadIsOwedSecondsOverTheHorizon()
        {
            _b.Car(CarLotState.PickupRequested);
            _b.Car(CarLotState.PickupRequested);
            _b.Car(CarLotState.PickupRequested);
            Assert.AreEqual(1f, Estimate().W, 0.001f);
        }

        [Test]
        public void PressureIsOneMinusTheLeastPatienceAmongWaitingAndPickupCars()
        {
            _b.Car(CarLotState.WaitingToPark, awaitingPark: true, patience: 0.6f);
            _b.Car(CarLotState.PickupRequested, patience: 0.2f);
            _b.Car(CarLotState.Parked, patience: 0.05f);
            Assert.AreEqual(0.8f, Estimate().P, 0.001f);
        }

        [Test]
        public void SlotRuleClassifiesOccupants()
        {
            Assert.AreEqual(StationSlotState.Working, SlotStateRule.Occupied(StationKind.Wash, true, false, false, false));
            Assert.AreEqual(StationSlotState.Blocked, SlotStateRule.Occupied(StationKind.Wash, true, true, false, false));
            Assert.AreEqual(StationSlotState.Squatted, SlotStateRule.Occupied(StationKind.Wash, false, false, true, false));
            Assert.AreEqual(StationSlotState.Working, SlotStateRule.Occupied(StationKind.Charging, false, false, true, false));
            Assert.AreEqual(StationSlotState.Blocked, SlotStateRule.Occupied(StationKind.Charging, false, false, true, true));
            Assert.AreEqual(StationSlotState.Squatted, SlotStateRule.Occupied(StationKind.Charging, true, false, false, false));
            Assert.AreEqual(StationSlotState.Reserved, SlotStateRule.Empty(0, 1));
            Assert.AreEqual(StationSlotState.Free, SlotStateRule.Empty(1, 1));
        }
    }
}
