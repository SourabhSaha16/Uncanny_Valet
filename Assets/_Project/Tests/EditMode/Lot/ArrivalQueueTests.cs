using NUnit.Framework;
using UncannyValet.Lot;
using UnityEngine;

namespace UncannyValet.Tests.Lot
{
    public class ArrivalQueueTests
    {
        [Test]
        public void SlotZero_IsTheFirstPoint()
        {
            var slots = ArrivalQueue.Slots(new Vector2(-12f, -6.5f), 3, 5.5f);
            Assert.AreEqual(3, slots.Length);
            Assert.AreEqual(new Vector2(-12f, -6.5f), slots[0]);
        }

        [Test]
        public void Slots_LineUpBehindEachOther_WithConfiguredSpacing()
        {
            var slots = ArrivalQueue.Slots(Vector2.zero, 3, 5.5f);
            Assert.AreEqual(-5.5f, slots[1].y, 0.0001f);
            Assert.AreEqual(-11f, slots[2].y, 0.0001f);
            Assert.AreEqual(0f, slots[2].x, 0.0001f);
        }

        [Test]
        public void DefaultSpacing_KeepsFourPointFiveLongCarsApart()
        {
            var spacing = ScriptableObject.CreateInstance<LotConfig>().QueueSlotSpacing;
            var slots = ArrivalQueue.Slots(Vector2.zero, 3, spacing);
            Assert.Greater(Mathf.Abs(slots[0].y - slots[1].y), 4.5f);
        }

        [Test]
        public void Count_IsAtLeastOne()
        {
            Assert.AreEqual(1, ArrivalQueue.Slots(Vector2.zero, 0, 5f).Length);
        }
    }
}
