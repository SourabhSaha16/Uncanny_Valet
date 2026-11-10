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
            Assert.AreEqual(10f, CameraFit.Size(View, View, Aspect, 1f, 1f, 8.5f, 10f), 1e-4f);
            Assert.AreEqual(30f, CameraFit.Size(new Rect(0, 0, 4, 4), new Rect(0, 0, 4, 4), Aspect, 1f, 1f, 30f, 12.5f), 1e-4f);
        }
    }
}
