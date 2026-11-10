using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Patrons;

namespace UncannyValet.Tests.Patrons
{
    public class ArrivalReachTests
    {
        [Test]
        public void ClearQueueStaysClear()
        {
            var blocked = new List<bool> { false, false, false };
            ArrivalReach.BlockBehind(blocked);
            CollectionAssert.AreEqual(new[] { false, false, false }, blocked);
        }

        [Test]
        public void ACarNearTheRoadCutsOffTheSlotsBehindIt()
        {
            var blocked = new List<bool> { false, true, false };
            ArrivalReach.BlockBehind(blocked);
            CollectionAssert.AreEqual(new[] { true, true, false }, blocked);
        }

        [Test]
        public void ACarInTheDeepestSlotBlocksNothingElse()
        {
            var blocked = new List<bool> { true, false, false };
            ArrivalReach.BlockBehind(blocked);
            CollectionAssert.AreEqual(new[] { true, false, false }, blocked);
        }

        [Test]
        public void AFreedDeepSlotIsUnreachableWhileALaterSlotIsTaken()
        {
            var blocked = new List<bool> { false, true, true };
            ArrivalReach.BlockBehind(blocked);
            Assert.AreEqual(ArrivalGate.NoSlot, ArrivalGate.FirstFreeSlot(2, 3, blocked));
        }
    }
}
