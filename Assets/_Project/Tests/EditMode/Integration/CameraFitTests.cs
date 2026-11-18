using NUnit.Framework;
using UncannyValet.Bootstrap;
using UnityEngine;

namespace UncannyValet.Tests.Integration
{
    public class CameraFitTests
    {
        private static readonly Rect View = new Rect(-20f, -10f, 40f, 20f);
        private static readonly Rect Lot = new Rect(-20f, -6f, 40f, 12f);
        private const float Aspect = 16f / 9f;

        [Test]
        public void NoTighteningMatchesTheOldFit()
        {
            var size = CameraFit.Size(View, View, Aspect, 1f, 1f, 1f, 50f);
            Assert.AreEqual(Mathf.Max(11f, 21f / Aspect), size, 1e-4f);
        }

        [Test]
        public void TighteningZoomsInButNeverCropsThePlayArea()
        {
            var loose = CameraFit.Size(View, Lot, Aspect, 1f, 1f, 1f, 50f);
            var tight = CameraFit.Size(View, Lot, Aspect, 1f, 0.78f, 1f, 50f);
            Assert.Less(tight, loose);
            Assert.GreaterOrEqual(tight * Aspect, Lot.width * 0.5f - 1e-4f);
            Assert.GreaterOrEqual(tight, Lot.height * 0.5f);
        }

        [Test]
        public void PlayAreaFloorWinsOverTightness()
        {
            var size = CameraFit.Size(View, View, Aspect, 1f, 0.5f, 1f, 50f);
            Assert.AreEqual(Mathf.Max(10f, 20f / Aspect), size, 1e-4f);
        }

        [Test]
        public void ResultIsClampedToTheLimits()
        {
            Assert.AreEqual(11.5f, CameraFit.Size(View, Lot, Aspect, 1f, 1f, 8.5f, 11.5f), 1e-4f);
            Assert.AreEqual(30f, CameraFit.Size(new Rect(0, 0, 4, 4), new Rect(0, 0, 4, 4), Aspect, 1f, 1f, 30f, 12.5f), 1e-4f);
        }

        [TestCase(21f / 9f)]
        [TestCase(16f / 9f)]
        [TestCase(4f / 3f)]
        [TestCase(9f / 16f)]
        [TestCase(1f)]
        public void WholeLotIsVisibleAtAnyWindowShape(float aspect)
        {
            var size = CameraFit.Size(View, Lot, aspect, 1f, 0.78f, 1f, 50f);
            Assert.GreaterOrEqual(size, Lot.height * 0.5f - 1e-4f);
            Assert.GreaterOrEqual(size * aspect, Lot.width * 0.5f - 1e-4f);
        }

        [Test]
        public void LotTallerThanTheCapStillFits()
        {
            var tall = new Rect(-12f, -17f, 24f, 34f);
            var size = CameraFit.Size(tall, tall, Aspect, 1f, 0.83f, 8.5f, 15f);
            Assert.AreEqual(17f, size, 1e-4f);
        }
    }
}
