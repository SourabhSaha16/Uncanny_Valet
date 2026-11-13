using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Patrons;

namespace UncannyValet.Tests.Patrons
{
    public class QueueSlotAllocatorTests
    {
        private const int Cap = 3;

        private sealed class FakeClock : IGameClock
        {
            public float SecondsRemaining => 0f;
            public float ShiftLengthSeconds => 0f;
            public float ElapsedMinutes => 0f;
            public bool IsRunning => true;
        }

        private sealed class FakeBlessings : IBlessingCalculator
        {
            public int Calculate(in BlessingInput input) => input.BaseBlessing;
            public BlessingBreakdown Explain(in BlessingInput input) =>
                new BlessingBreakdown(input.BaseBlessing, 0, 0, input.BaseBlessing, BlessingZeroReason.None);
        }

        private sealed class FakeScan : IArrivalScan
        {
            public readonly List<bool> Occupied = new List<bool> { false, false, false };
            public IReadOnlyList<bool> Scan(int maxSlots) => Occupied;
        }

        private EventBus _bus;
        private TicketRegistry _registry;
        private FakeScan _scan;
        private QueueSlotAllocator _slots;
        private int _nextCar;

        [SetUp]
        public void SetUp()
        {
            _nextCar = 1;
            _bus = new EventBus();
            _registry = new TicketRegistry(_bus, new FakeClock(), new FakeBlessings());
            _scan = new FakeScan();
            _slots = new QueueSlotAllocator(_bus, _registry, _scan, new PatronPlan(Cap));
        }

        [TearDown]
        public void TearDown() => _slots.Dispose();

        // Sends a car the way the spawner does: choose, open its ticket, reserve.
        private CarId Send(out int slot)
        {
            slot = _slots.ChooseForSpawn();
            Assert.AreNotEqual(ArrivalGate.NoSlot, slot, "a slot was expected");
            var car = new CarId(_nextCar++);
            _registry.Open(car, new PatronRequest(30f, 20, false));
            _slots.Reserve(car, slot);
            return car;
        }

        [Test]
        public void EmptyQueue_SendsToSlotZero()
        {
            Send(out var slot);
            Assert.AreEqual(0, slot);
        }

        [Test]
        public void SlotFreedWhileTheCarDrivesIn_ItMovesUp()
        {
            _scan.Occupied[0] = true;
            Send(out var slot);
            Assert.AreEqual(1, slot);
            _scan.Occupied[0] = false;
            Assert.AreEqual(0, _slots.Reevaluate(1));
        }

        [Test]
        public void SlotStillOccupied_TheCarKeepsItsSlot()
        {
            _scan.Occupied[0] = true;
            Send(out _);
            Assert.AreEqual(1, _slots.Reevaluate(1));
        }

        [Test]
        public void SlotsAheadAllOccupied_TheCarStaysAtTheLastFreeSlot()
        {
            _scan.Occupied[0] = true;
            _scan.Occupied[1] = true;
            Send(out var slot);
            Assert.AreEqual(2, slot);
            Assert.AreEqual(2, _slots.Reevaluate(2));
        }

        [Test]
        public void TwoCarsDrivingIn_NeverShareASlotAndNeverLeaveAGap()
        {
            _scan.Occupied[0] = true;
            Send(out var a);
            Send(out var b);
            Assert.AreEqual(1, a);
            Assert.AreEqual(2, b);
            _scan.Occupied[0] = false;
            Assert.AreEqual(2, _slots.Reevaluate(2), "the car behind waits: the car ahead has not moved up yet");
            Assert.AreEqual(0, _slots.Reevaluate(1));
            Assert.AreEqual(1, _slots.Reevaluate(2));
        }

        [Test]
        public void ReservationIsReleasedWhenTheCarArrives()
        {
            var car = Send(out _);
            _bus.Publish(new CarArrived(car));
            Assert.AreEqual(0, _slots.ReservedCount);
        }

        [Test]
        public void CarDrivenOutOfSlotZeroAfterArriving_FreesTheSlotForTheNextCar()
        {
            var car = Send(out _);
            _scan.Occupied[0] = true;
            _bus.Publish(new CarArrived(car));
            _scan.Occupied[0] = false;
            Assert.AreEqual(0, _slots.ChooseForSpawn(), "the arrived car left slot 0 although its ticket still waits");
        }

        [Test]
        public void ReservationIsReleasedWhenTheTicketClosesBeforeArrival()
        {
            Send(out _);
            _bus.Publish(new ShiftEnded());
            Assert.AreEqual(0, _slots.ReservedCount);
        }

        [Test]
        public void ReservationOfACarWhoseTicketIsGone_IsPrunedNotLeaked()
        {
            _slots.Reserve(new CarId(99), 0);
            Assert.AreEqual(0, _slots.ChooseForSpawn());
            Assert.AreEqual(0, _slots.ReservedCount);
        }

        [Test]
        public void ReevaluatingAnUnknownSlot_ChangesNothing()
        {
            Assert.AreEqual(2, _slots.Reevaluate(2));
        }

        [Test]
        public void FourSlotsAndACapOfFour_SendFourCarsToFourSlots_ThenNoSlot()
        {
            _slots.Dispose();
            _scan.Occupied.Add(false);
            _slots = new QueueSlotAllocator(_bus, _registry, _scan, new PatronPlan(4));
            for (var expected = 0; expected < 4; expected++)
            {
                Send(out var slot);
                Assert.AreEqual(expected, slot);
            }
            Assert.AreEqual(ArrivalGate.NoSlot, _slots.ChooseForSpawn());
        }

        [Test]
        public void FullWaitingCap_GivesNoSlot()
        {
            Send(out _);
            Send(out _);
            Send(out _);
            Assert.AreEqual(ArrivalGate.NoSlot, _slots.ChooseForSpawn());
        }
    }
}
