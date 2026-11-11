using NUnit.Framework;
using UncannyValet.Cars.Fx;
using UncannyValet.Core.Ids;
using UnityEngine;

namespace UncannyValet.Tests.Cars
{
    public class TyreMarkTests
    {
        [Test]
        public void RearTyres_SitBehindTheCentreOnBothSides()
        {
            var size = new Vector2(1f, 2f);
            var left = TyreMarkMath.RearTyre(Vector2.zero, 0f, size, 0.38f, 0.32f, false);
            var right = TyreMarkMath.RearTyre(Vector2.zero, 0f, size, 0.38f, 0.32f, true);
            Assert.AreEqual(-0.38f, left.x, 1e-5f);
            Assert.AreEqual(0.38f, right.x, 1e-5f);
            Assert.AreEqual(-0.64f, left.y, 1e-5f);
        }

        [Test]
        public void RearTyres_TurnWithTheCar()
        {
            var tyre = TyreMarkMath.RearTyre(new Vector2(5f, 5f), 90f, new Vector2(1f, 2f), 0.38f, 0.32f, true);
            // Facing left (+90): the rear points right, the car's right-hand side points up.
            Assert.AreEqual(5.64f, tyre.x, 1e-4f);
            Assert.AreEqual(5.38f, tyre.y, 1e-4f);
        }

        [Test]
        public void Trail_FirstPointOnlyStartsAndMarksComeEverySpacing()
        {
            var trail = new TyreTrail();
            Assert.IsFalse(trail.TryAdvance(Vector2.zero, 1f, 0.05f, out _));
            Assert.IsFalse(trail.TryAdvance(new Vector2(0f, 0.6f), 1f, 0.05f, out _));
            Assert.IsTrue(trail.TryAdvance(new Vector2(0f, 1.1f), 1f, 0.05f, out var mark));
            Assert.AreEqual(0.55f, mark.Center.y, 1e-5f);
            Assert.AreEqual(1.15f, mark.Length, 1e-5f);
            Assert.IsFalse(trail.TryAdvance(new Vector2(0f, 1.5f), 1f, 0.05f, out _), "distance since the last mark, not frames");
        }

        [Test]
        public void Trail_StrokeLiesAlongTheTravelDirection()
        {
            var trail = new TyreTrail();
            trail.TryAdvance(Vector2.zero, 1f, 0f, out _);
            trail.TryAdvance(new Vector2(2f, 0f), 1f, 0f, out var east);
            Assert.AreEqual(-90f, east.AngleDegrees, 1e-3f, "local up turned to point east");
            trail.TryAdvance(new Vector2(2f, 2f), 1f, 0f, out var north);
            Assert.AreEqual(0f, north.AngleDegrees, 1e-3f);
        }

        [Test]
        public void Trail_BreakStartsAFreshTrailWithoutBridgingTheGap()
        {
            var trail = new TyreTrail();
            trail.TryAdvance(Vector2.zero, 1f, 0f, out _);
            trail.Break();
            Assert.IsFalse(trail.TryAdvance(new Vector2(30f, 0f), 1f, 0f, out _));
        }

        [Test]
        public void SlipMeter_LatestReporterOwnsTheValueAndOnlyItCanClear()
        {
            var meter = new SlipMeter();
            meter.Report(new CarId(1), true, 0.6f);
            Assert.IsTrue(meter.IsSliding);
            Assert.AreEqual(0.6f, meter.Intensity01, 1e-5f);
            meter.Clear(new CarId(2));
            Assert.IsTrue(meter.IsSliding);
            meter.Clear(new CarId(1));
            Assert.IsFalse(meter.IsSliding);
            Assert.AreEqual(0f, meter.Intensity01);
            meter.Report(new CarId(3), false, 0.9f);
            Assert.AreEqual(0f, meter.Intensity01, "not sliding reads as 0");
        }
    }
}
