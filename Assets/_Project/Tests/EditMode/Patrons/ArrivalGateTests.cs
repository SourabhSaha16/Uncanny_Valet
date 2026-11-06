using NUnit.Framework;
using UncannyValet.Patrons;

namespace UncannyValet.Tests.Patrons
{
    public class ArrivalGateTests
    {
        private const int MaxWaiting = 3;
        private static readonly bool[] AllClear = { false, false, false, false };

        [Test]
        public void EmptyQueue_UsesFirstSlot() =>
            Assert.AreEqual(0, ArrivalGate.FirstFreeSlot(0, MaxWaiting, AllClear));

        [Test]
        public void FirstSlotBlocked_UsesSecond() =>
            Assert.AreEqual(1, ArrivalGate.FirstFreeSlot(1, MaxWaiting, new[] { true, false, false, false }));

        [Test]
        public void CapReached_NoSlot() =>
            Assert.AreEqual(ArrivalGate.NoSlot, ArrivalGate.FirstFreeSlot(3, MaxWaiting, AllClear));

        [Test]
        public void ParkedCarsDoNotCount_OnlyWaitingPassedIn() =>
            Assert.AreEqual(0, ArrivalGate.FirstFreeSlot(0, MaxWaiting, AllClear));

        [Test]
        public void AllUsableSlotsBlocked_NoSlot() =>
            Assert.AreEqual(ArrivalGate.NoSlot,
                ArrivalGate.FirstFreeSlot(2, MaxWaiting, new[] { true, true, true }));

        [Test]
        public void SlotsBeyondCap_AreNeverUsed() =>
            Assert.AreEqual(ArrivalGate.NoSlot,
                ArrivalGate.FirstFreeSlot(1, MaxWaiting, new[] { true, true, true, false }));

        [Test]
        public void FewerSlotsThanCap_NoSlotWhenAllBlocked() =>
            Assert.AreEqual(ArrivalGate.NoSlot, ArrivalGate.FirstFreeSlot(0, MaxWaiting, new[] { true }));
    }
}
