using NUnit.Framework;
using UncannyValet.Cars;
using UnityEngine;

namespace UncannyValet.Tests.Cars
{
    public class CarArtMathTests
    {
        [Test]
        public void PickIndexIsStableAndInRange()
        {
            for (var id = -5; id < 200; id++)
            {
                var index = CarArtMath.PickIndex(id, 7);
                Assert.That(index, Is.InRange(0, 6));
                Assert.AreEqual(index, CarArtMath.PickIndex(id, 7));
            }
        }

        [Test]
        public void PickIndexWithoutEntriesIsMinusOne()
        {
            Assert.AreEqual(-1, CarArtMath.PickIndex(3, 0));
        }

        [Test]
        public void PickIndexUsesSeveralEntries()
        {
            var seen = new System.Collections.Generic.HashSet<int>();
            for (var id = 1; id <= 40; id++) seen.Add(CarArtMath.PickIndex(id, 4));
            Assert.Greater(seen.Count, 1);
        }

        [Test]
        public void UniformScaleMatchesCarLength()
        {
            Assert.AreEqual(1.1f, CarArtMath.UniformScale(new Vector2(1.2f, 1.8f), new Vector2(1f, 1.98f)), 1e-4f);
        }

        [Test]
        public void LocalScaleCancelsTheNonUniformParent()
        {
            var carSize = new Vector2(1f, 2f);
            var local = CarArtMath.LocalScale(1.1f, carSize);
            Assert.AreEqual(1.1f, local.x * carSize.x, 1e-4f);
            Assert.AreEqual(1.1f, local.y * carSize.y, 1e-4f);
        }

        [Test]
        public void LocalOffsetPutsTheOpaqueCentreOnTheCarCentre()
        {
            var offset = CarArtMath.LocalOffset(new Vector2(0f, 0.1f), 1f, new Vector2(1f, 2f), 0f);
            Assert.AreEqual(0f, offset.x, 1e-4f);
            Assert.AreEqual(-0.05f, offset.y, 1e-4f);
            var turned = CarArtMath.LocalOffset(new Vector2(0f, 0.1f), 1f, new Vector2(1f, 2f), 180f);
            Assert.AreEqual(0.05f, turned.y, 1e-4f);
        }

        [Test]
        public void FlipIsStableAndUsesBothLooks()
        {
            var flipped = 0;
            for (var id = 1; id <= 200; id++)
            {
                Assert.AreEqual(CarArtMath.IsFlipped(id), CarArtMath.IsFlipped(id));
                if (CarArtMath.IsFlipped(id)) flipped++;
            }
            Assert.That(flipped, Is.InRange(60, 140));
        }

        [Test]
        public void FlipIsIndependentOfTheSpritePick()
        {
            var seen = new System.Collections.Generic.HashSet<int>();
            for (var id = 1; id <= 400; id++)
                if (CarArtMath.IsFlipped(id)) seen.Add(CarArtMath.PickIndex(id, 14));
            Assert.AreEqual(14, seen.Count);
        }

        [Test]
        public void EveryOneOfFourteenSpritesIsUsed()
        {
            var seen = new System.Collections.Generic.HashSet<int>();
            for (var id = 1; id <= 100; id++) seen.Add(CarArtMath.PickIndex(id, 14));
            Assert.AreEqual(14, seen.Count);
        }

        [Test]
        public void MirrorCenterOnlyFlipsX()
        {
            Assert.AreEqual(new Vector2(-0.2f, 0.1f), CarArtMath.MirrorCenter(new Vector2(0.2f, 0.1f), true));
            Assert.AreEqual(new Vector2(0.2f, 0.1f), CarArtMath.MirrorCenter(new Vector2(0.2f, 0.1f), false));
        }

        [Test]
        public void OnlyHalfTurnsAreAllowedAsRotationOffsets()
        {
            Assert.IsTrue(CarArtMath.IsHalfTurn(0f));
            Assert.IsTrue(CarArtMath.IsHalfTurn(180f));
            Assert.IsFalse(CarArtMath.IsHalfTurn(90f));
        }
    }
}
