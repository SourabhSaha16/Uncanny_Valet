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
            var pitch = ArrivalQueue.Pitch(config.QueueCarSize.x, config.QueueSlotGap);
            return ArrivalQueue.Slots(new Vector2(-12f, -6f), config.QueueSlotCount, config.QueueDirection, pitch);
        }

        [Test]
        public void SlotZero_IsTheFirstPoint()
        {
            var slots = ArrivalQueue.Slots(new Vector2(-12f, -6.5f), 3, Vector2.left, 2.8f);
            Assert.AreEqual(3, slots.Length);
            Assert.AreEqual(new Vector2(-12f, -6.5f), slots[0]);
        }

        [Test]
        public void Slots_SitSideBySide_AtOnePitch()
        {
            var slots = ArrivalQueue.Slots(Vector2.zero, 3, Vector2.left, 2.8f);
            Assert.AreEqual(-2.8f, slots[1].x, 0.0001f);
            Assert.AreEqual(-5.6f, slots[2].x, 0.0001f);
            Assert.AreEqual(0f, slots[2].y, 0.0001f);
        }

        [Test]
        public void Pitch_IsCarWidthPlusGap()
        {
            Assert.AreEqual(2.8f, ArrivalQueue.Pitch(2.2f, 0.6f), 0.0001f);
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
        public void AllSlotsLieWithinTheCameraHalfExtent()
        {
            // Camera centred on the valet start (x = -9), orthographic size 8.5, 4:3 width 22.6.
            var view = new Rect(-9f - 11.3f, -8.5f, 22.6f, 17f);
            var bounds = ArrivalQueue.Bounds(DefaultSlots(), CarSize, 0f);
            Assert.GreaterOrEqual(bounds.xMin, view.xMin);
            Assert.LessOrEqual(bounds.xMax, view.xMax);
            Assert.GreaterOrEqual(bounds.yMin, view.yMin);
            Assert.LessOrEqual(bounds.yMax, view.yMax);
        }

        [Test]
        public void Footprint_SwapsSides_WhenRotatedNinetyDegrees()
        {
            var r = ArrivalQueue.Footprint(Vector2.zero, CarSize, 90f);
            Assert.AreEqual(4.5f, r.width, 0.001f);
            Assert.AreEqual(2.2f, r.height, 0.001f);
        }

        [Test]
        public void Count_IsAtLeastOne_AndZeroDirectionFallsBack()
        {
            Assert.AreEqual(1, ArrivalQueue.Slots(Vector2.zero, 0, Vector2.zero, 5f).Length);
            Assert.Less(ArrivalQueue.Slots(Vector2.zero, 2, Vector2.zero, 5f)[1].x, 0f);
        }
    }
}
