using NUnit.Framework;
using UncannyValet.Cars;
using UnityEngine;

namespace UncannyValet.Tests.Cars
{
    public class DriftModelTests
    {
        private const float Dt = 0.02f;
        private static readonly DriftSettings Spec = new DriftSettings(8f, 0.6f, 3f, 6f, 0.55f, 0.25f, 1f);
        private static readonly Vector2 Up = Vector2.up;

        [Test]
        public void Grip_IsFullBelowLowSpeed() => Assert.AreEqual(1f, DriftModel.Grip(2.9f, false, Spec), 1e-4f);

        [Test]
        public void Grip_BlendsBetweenLowAndHighSpeed() => Assert.AreEqual(0.775f, DriftModel.Grip(4.5f, false, Spec), 1e-4f);

        [Test]
        public void Grip_IsGripAtSpeedAboveHighSpeed() => Assert.AreEqual(0.55f, DriftModel.Grip(9f, false, Spec), 1e-4f);

        [Test]
        public void Grip_DriftScalesToAQuarter() => Assert.AreEqual(0.25f, DriftModel.Grip(2f, true, Spec), 1e-4f);

        [Test]
        public void Grip_DriftHasNoEffectAtOrBelowMinSpeed() => Assert.AreEqual(1f, DriftModel.Grip(1f, true, Spec), 1e-4f);

        [Test]
        public void FullGrip_RemovesSidewaysSpeedQuickly()
        {
            var v = new Vector2(1.5f, 1f); // 1.5 sideways at 1.8 u/s
            for (var i = 0; i < 50; i++) v = DriftModel.Step(v, Up, false, Spec, Dt); // one second
            Assert.Less(Mathf.Abs(v.x), 0.01f);
        }

        [Test]
        public void FullForwardGrip_KeepsTheSpeedExactly()
        {
            var settings = new DriftSettings(8f, 1f, 3f, 6f, 0.55f, 0.25f, 1f);
            var v = DriftModel.Step(new Vector2(1f, 2f), Up, false, settings, Dt);
            Assert.AreEqual(new Vector2(1f, 2f).magnitude, v.magnitude, 1e-4f);
            Assert.Less(v.x, 1f);
        }

        [Test]
        public void ZeroForwardGrip_LeavesForwardSpeedAndScrubsTheRest()
        {
            var settings = new DriftSettings(8f, 0f, 3f, 6f, 0.55f, 0.25f, 1f);
            var v = DriftModel.Step(new Vector2(1f, 2f), Up, false, settings, Dt);
            Assert.AreEqual(2f, v.y, 1e-4f);
            Assert.Less(v.magnitude, new Vector2(1f, 2f).magnitude);
        }

        [Test]
        public void PartialForwardGrip_KeepsAShareOfTheLostSpeed()
        {
            var none = DriftModel.Step(new Vector2(1f, 2f), Up, false, new DriftSettings(8f, 0f, 3f, 6f, 0.55f, 0.25f, 1f), Dt);
            var some = DriftModel.Step(new Vector2(1f, 2f), Up, false, Spec, Dt);
            var all = DriftModel.Step(new Vector2(1f, 2f), Up, false, new DriftSettings(8f, 1f, 3f, 6f, 0.55f, 0.25f, 1f), Dt);
            Assert.Greater(some.magnitude, none.magnitude);
            Assert.Less(some.magnitude, all.magnitude);
        }

        [Test]
        public void ForwardGrip_AlsoHoldsInReverse()
        {
            var settings = new DriftSettings(8f, 1f, 3f, 6f, 0.55f, 0.25f, 1f);
            var v = DriftModel.Step(new Vector2(1f, -2f), Up, false, settings, Dt);
            Assert.AreEqual(new Vector2(1f, -2f).magnitude, v.magnitude, 1e-4f);
            Assert.Less(v.y, 0f);
        }

        [Test]
        public void SidewaysSlideWithNoForwardSpeed_DoesNotCreepForward()
        {
            var v = DriftModel.Step(new Vector2(2f, 0f), Up, false, Spec, Dt);
            Assert.AreEqual(0f, v.y, 1e-5f);
        }

        [Test]
        public void HandbrakeKeepsMoreSidewaysSpeedThanFullGrip()
        {
            var grip = DriftModel.Step(new Vector2(1.5f, 2f), Up, false, Spec, Dt);
            var drift = DriftModel.Step(new Vector2(1.5f, 2f), Up, true, Spec, Dt);
            Assert.Greater(drift.x, grip.x);
        }

        [Test]
        public void StepIsRotationInvariant()
        {
            var up = DriftModel.Step(new Vector2(1f, 2f), Up, true, Spec, Dt);
            var right = DriftModel.Step(new Vector2(2f, -1f), Vector2.right, true, Spec, Dt); // the same velocity turned 90 degrees clockwise
            Assert.AreEqual(up.x, -right.y, 1e-4f);
            Assert.AreEqual(up.y, right.x, 1e-4f);
        }

        [Test]
        public void SlipAngle_IsZeroWhenDrivingStraight() => Assert.AreEqual(0f, DriftModel.SlipAngleDegrees(new Vector2(0f, 5f), Up), 1e-4f);

        [Test]
        public void SlipAngle_IsZeroInReverseStraight() => Assert.AreEqual(0f, DriftModel.SlipAngleDegrees(new Vector2(0f, -3f), Up), 1e-4f);

        [Test]
        public void SlipAngle_IsFortyFiveWhenSidewaysEqualsForward() => Assert.AreEqual(45f, DriftModel.SlipAngleDegrees(new Vector2(2f, 2f), Up), 1e-3f);

        [Test]
        public void SlipAngle_AtSixUnitsPerSecondFullLock_IsAFewDegreesWithoutHandbrakeAndLargerWithIt()
        {
            var normal = SteadySlip(false);
            var drift = SteadySlip(true);
            Assert.Greater(normal, 5f);
            Assert.Greater(drift, normal * 2f);
        }

        // Rotates the facing at the full-lock turn rate (R_min 3.3 at 6 u/s: 1.212 rad/s) for two seconds.
        private static float SteadySlip(bool drift)
        {
            var facing = Up;
            var v = Up * 6f;
            var rate = 1.212f;
            for (var i = 0; i < 100; i++)
            {
                facing = Quaternion.Euler(0f, 0f, rate * Mathf.Rad2Deg * Dt) * facing;
                v = DriftModel.Step(v, facing, drift, Spec, Dt);
            }
            return DriftModel.SlipAngleDegrees(v, facing);
        }
    }
}
