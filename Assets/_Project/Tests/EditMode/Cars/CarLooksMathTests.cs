using NUnit.Framework;
using UncannyValet.Cars;
using UnityEngine;

namespace UncannyValet.Tests.Cars
{
    public class CarLooksMathTests
    {
        private static readonly Vector2 Car = new Vector2(1f, 2f);
        private static readonly Vector2 Mirror = new Vector2(0.2f, 0.2f);

        [Test]
        public void MirrorsAreSymmetric()
        {
            Vector2 left = CarMirrorMath.LocalCentre(Car, Mirror, 0.3f, 0.1f, -1);
            Vector2 right = CarMirrorMath.LocalCentre(Car, Mirror, 0.3f, 0.1f, 1);
            Assert.AreEqual(-left.x, right.x, 1e-6f);
            Assert.AreEqual(left.y, right.y, 1e-6f);
        }

        [Test]
        public void MirrorsSitAtTheFront()
        {
            Vector2 right = CarMirrorMath.LocalCentre(Car, Mirror, 0.3f, 0.1f, 1);
            Assert.Greater(right.y, 0f);
            Assert.Less(right.y + CarMirrorMath.LocalScale(Car, Mirror).y * 0.5f, 0.5f + 1e-6f);
        }

        [Test]
        public void MirrorsStickOutPastTheBodyWidth()
        {
            Vector2 right = CarMirrorMath.LocalCentre(Car, Mirror, 0.3f, 0.1f, 1);
            float outerEdge = right.x + CarMirrorMath.LocalScale(Car, Mirror).x * 0.5f;
            Assert.AreEqual(0.5f + 0.1f / Car.x, outerEdge, 1e-5f);
        }

        [Test]
        public void ChargeFillIsAnchoredToTheBottomBorder()
        {
            float usable = CarChargeBarMath.UsableFraction(1.3f, 0.05f);
            float bottom = CarChargeBarMath.FillCentreY(0.4f, usable) - CarChargeBarMath.FillHeight(0.4f, usable) * 0.5f;
            Assert.AreEqual(-usable * 0.5f, bottom, 1e-6f);
            Assert.AreEqual(usable, CarChargeBarMath.FillHeight(2f, usable), 1e-6f);
        }

        [Test]
        public void UsableFractionHandlesDegenerateSizes()
        {
            Assert.AreEqual(0f, CarChargeBarMath.UsableFraction(0f, 0.1f));
            Assert.AreEqual(0f, CarChargeBarMath.UsableFraction(0.1f, 0.2f));
            Assert.AreEqual(1f, CarChargeBarMath.UsableFraction(1f, 0f));
        }

        [Test]
        public void FullDetection()
        {
            Assert.IsTrue(CarChargeBarMath.IsFull(1f));
            Assert.IsFalse(CarChargeBarMath.IsFull(0.9f));
        }

        [Test]
        public void MudFadesWithCleanliness()
        {
            Assert.AreEqual(0.7f, CarMudMath.Alpha(0f, 0.7f), 1e-6f);
            Assert.AreEqual(0.35f, CarMudMath.Alpha(0.5f, 0.7f), 1e-6f);
            Assert.AreEqual(0f, CarMudMath.Alpha(1f, 0.7f), 1e-6f);
            Assert.AreEqual(0f, CarMudMath.Alpha(5f, 0.7f), 1e-6f);
        }
    }
}
