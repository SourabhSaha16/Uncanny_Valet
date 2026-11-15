using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Patrons;

namespace UncannyValet.Tests.Patrons
{
    public class ArrivalPacingTests
    {
        private const int Cap = 4;

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
            public readonly List<bool> Occupied = new List<bool> { false, false, false, false };
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
            _slots = new QueueSlotAllocator(_bus, _registry, _scan, Cap);
        }

        [TearDown]
        public void TearDown() => _slots.Dispose();

        // The spawner's top-up loop: send a car into every free slot while the pacing says one is due.
        private int TopUp(IArrivalPacing pacing, float minute)
        {
            var sent = 0;
            while (pacing.TopsUpQueue && pacing.Due(minute) && _slots.ChooseForSpawn() != ArrivalGate.NoSlot)
            {
                var slot = _slots.ChooseForSpawn();
                var car = new CarId(_nextCar++);
                _registry.Open(car, new PatronRequest(30f, 20, false));
                _slots.Reserve(car, slot);
                pacing.Spawned(minute);
                sent++;
            }
            return sent;
        }

        [Test]
        public void CrazyLot_FillsTheWholeQueueAtTheStartWithNoSpacing()
        {
            var pacing = ArrivalPacings.For(LotKind.Crazy, 0f);
            Assert.AreEqual(Cap, TopUp(pacing, 0f));
            Assert.AreEqual(Cap, _slots.ReservedCount);
            Assert.AreEqual(0, TopUp(pacing, 0f), "a full queue takes no more cars");
        }

        [Test]
        public void CrazyLot_SpacingStaggersTheStartFill()
        {
            var pacing = ArrivalPacings.For(LotKind.Crazy, 2f);
            Assert.AreEqual(1, TopUp(pacing, 0f));
            Assert.AreEqual(0, TopUp(pacing, 1f));
            Assert.AreEqual(1, TopUp(pacing, 2f));
            Assert.AreEqual(1, TopUp(pacing, 4f));
            Assert.AreEqual(1, TopUp(pacing, 6f));
            Assert.AreEqual(0, TopUp(pacing, 8f), "queue full");
        }

        [Test]
        public void CrazyLot_RefillsAFreedSlotAtOnce()
        {
            var pacing = ArrivalPacings.For(LotKind.Crazy, 0f);
            TopUp(pacing, 0f);
            _bus.Publish(new CarArrived(new CarId(1)));
            _scan.Occupied[0] = true;
            _scan.Occupied[1] = true;
            _scan.Occupied[2] = true;
            _scan.Occupied[3] = false;
            _bus.Publish(new CarArrived(new CarId(2)));
            _bus.Publish(new CarArrived(new CarId(3)));
            _bus.Publish(new CarArrived(new CarId(4)));
            Assert.AreEqual(1, TopUp(pacing, 5f), "the slot the queue freed is refilled on the next tick");
        }

        [Test]
        public void ShiftRestart_ResetsTheSpacing()
        {
            var pacing = ArrivalPacings.For(LotKind.Crazy, 5f);
            TopUp(pacing, 100f);
            pacing.Reset();
            Assert.IsTrue(pacing.Due(0f));
        }

        [TestCase(LotKind.Training)]
        [TestCase(LotKind.Finale)]
        [TestCase(LotKind.None)]
        public void OtherLots_KeepTheDirectorsPacing(LotKind kind)
        {
            var pacing = ArrivalPacings.For(kind, 0f);
            Assert.IsFalse(pacing.TopsUpQueue);
            Assert.AreEqual(0, TopUp(pacing, 0f), "nothing is sent on its own");
            Assert.IsFalse(pacing.Due(1000f));
        }

        [Test]
        public void NegativeSpacing_IsTreatedAsZero()
        {
            var pacing = ArrivalPacings.For(LotKind.Crazy, -3f);
            Assert.AreEqual(Cap, TopUp(pacing, 0f));
        }
    }
}
