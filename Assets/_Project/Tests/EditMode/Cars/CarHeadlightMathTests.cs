using NUnit.Framework;
using UncannyValet.Cars;
using UnityEngine;

namespace UncannyValet.Tests.Cars
{
    public class CarHeadlightMathTests
    {
        private static readonly Vector2 Car = new Vector2(1f, 2f);
        private static readonly Vector2 Light = new Vector2(0.2f, 0.2f);

        [Test]
        public void LightsSitAtTheFrontNotTheBack()
        {
            var left = CarHeadlightMath.LocalCentre(Car, Light, 0.1f, 0.1f, -1);
            var right = CarHeadlightMath.LocalCentre(Car, Light, 0.1f, 0.1f, 1);
            Assert.Greater(left.y, 0f);
            Assert.AreEqual(left.y, right.y, 1e-5f);
        }

        [Test]
        public void LightsAreSymmetricAboutTheCentreLine()
        {
            var left = CarHeadlightMath.LocalCentre(Car, Light, 0.1f, 0.1f, -1);
            var right = CarHeadlightMath.LocalCentre(Car, Light, 0.1f, 0.1f, 1);
            Assert.AreEqual(-left.x, right.x, 1e-5f);
            Assert.Less(left.x, 0f);
        }

        [Test]
        public void NoseLightEdgeIsTheFrontInsetFromTheNose()
        {
            var centre = CarHeadlightMath.LocalCentre(Car, Light, 0.1f, 0.1f, 1);
            var topWorld = (centre.y + Light.y * 0.5f / Car.y) * Car.y;
            Assert.AreEqual(Car.y * 0.5f - 0.1f, topWorld, 1e-5f);
        }

        [Test]
        public void ScaleCompensatesForTheCarScale()
        {
            var scale = CarHeadlightMath.LocalScale(Car, Light);
            Assert.AreEqual(Light.x, scale.x * Car.x, 1e-5f);
            Assert.AreEqual(Light.y, scale.y * Car.y, 1e-5f);
        }
    }
}
