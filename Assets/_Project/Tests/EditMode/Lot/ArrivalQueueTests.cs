using NUnit.Framework;
using UncannyValet.Lot;
using UnityEngine;

namespace UncannyValet.Tests.Lot
{
    public class ArrivalQueueTests
    {
        private static readonly Vector2 CarSize = new Vector2(2.2f, 4.5f);

        private static Vector2[] DefaultSlots()
        {
            var config = ScriptableObject.CreateInstance<LotConfig>();
            var pitch = ArrivalQueue.Pitch(config.QueueCarSize.y, config.QueueSlotGap);
            return ArrivalQueue.Slots(new Vector2(-10f, -6f), config.QueueSlotCount, config.QueueDirection, pitch);
        }

        [Test]
        public void SlotZero_IsTheFirstPoint()
        {
            var slots = ArrivalQueue.Slots(new Vector2(-12f, -6.5f), 3, Vector2.left, 2.8f);
            Assert.AreEqual(3, slots.Length);
            Assert.AreEqual(new Vector2(-12f, -6.5f), slots[0]);
        }

        [Test]
        public void Slots_SitInALine_AtOnePitch()
        {
            var slots = ArrivalQueue.Slots(Vector2.zero, 3, Vector2.left, 2.8f);
            Assert.AreEqual(-2.8f, slots[1].x, 0.0001f);
            Assert.AreEqual(-5.6f, slots[2].x, 0.0001f);
            Assert.AreEqual(0f, slots[2].y, 0.0001f);
        }

        [Test]
        public void Pitch_IsTheExtentAlongTheLinePlusGap()
        {
            Assert.AreEqual(5.1f, ArrivalQueue.Pitch(4.5f, 0.6f), 0.0001f);
        }

        [Test]
        public void NoTwoCarRectanglesOverlap()
        {
            var slots = DefaultSlots();
            for (var i = 0; i < slots.Length; i++)
            for (var j = i + 1; j < slots.Length; j++)
                Assert.IsFalse(ArrivalQueue.Footprint(slots[i], CarSize, 0f).Overlaps(ArrivalQueue.Footprint(slots[j], CarSize, 0f)));
        }

        [Test]
        public void TheDefaultQueueIsAColumnOfThreeThatGrowsDownTheScreen()
        {
            var slots = DefaultSlots();
            Assert.AreEqual(3, slots.Length);
            Assert.AreEqual(slots[0].x, slots[2].x, 0.0001f);
            Assert.Less(slots[1].y, slots[0].y);
            Assert.Less(slots[2].y, slots[1].y);
        }

        [Test]
        public void Footprint_SwapsSides_WhenRotatedNinetyDegrees()
        {
            var r = ArrivalQueue.Footprint(Vector2.zero, CarSize, 90f);
            Assert.AreEqual(4.5f, r.width, 0.001f);
            Assert.AreEqual(2.2f, r.height, 0.001f);
        }

        [Test]
        public void Count_IsAtLeastOne_AndZeroDirectionFallsBackToDown()
        {
            Assert.AreEqual(1, ArrivalQueue.Slots(Vector2.zero, 0, Vector2.zero, 5f).Length);
            Assert.Less(ArrivalQueue.Slots(Vector2.zero, 2, Vector2.zero, 5f)[1].y, 0f);
        }
    }
}
