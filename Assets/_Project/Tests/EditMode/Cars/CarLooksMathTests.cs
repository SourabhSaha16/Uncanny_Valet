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
        public void MudWipesLeftToRightKeepingRightEdge()
        {
            Assert.AreEqual(1f, CarMudMath.Width(0f), 1e-6f);
            Assert.AreEqual(0f, CarMudMath.CenterX(0f), 1e-6f);
            Assert.AreEqual(0.5f, CarMudMath.Width(0.5f), 1e-6f);
            Assert.AreEqual(0.25f, CarMudMath.CenterX(0.5f), 1e-6f);
            Assert.AreEqual(0f, CarMudMath.Width(1f), 1e-6f);
            Assert.AreEqual(0f, CarMudMath.Width(5f), 1e-6f);
            Assert.AreEqual(0.5f, CarMudMath.CenterX(5f), 1e-6f);
        }

        [Test]
        public void MudRightEdgeNeverMoves()
        {
            for (float c = 0f; c <= 1f; c += 0.1f)
                Assert.AreEqual(0.5f, CarMudMath.CenterX(c) + CarMudMath.Width(c) * 0.5f, 1e-5f);
        }
    }
}
