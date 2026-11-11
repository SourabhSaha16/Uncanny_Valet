using NUnit.Framework;
using UncannyValet.Cars;
using UncannyValet.Cars.Fx;
using UnityEngine;

namespace UncannyValet.Tests.Cars
{
    public class SkidRuleTests
    {
        private static readonly SkidSettings S = new SkidSettings(1.2f, 2f, 0.8f, 0.75f, 40f, 0.3f, 4f);

        private static SlipReading Slip(float forward, float sideways, float throttle = 0f, bool drift = false)
            => new SlipReading(forward, sideways, throttle, drift);

        [Test]
        public void Straight_NoSlide()
        {
            var state = new SkidRule().Step(Slip(6f, 0f), S);
            Assert.IsFalse(state.Sliding);
            Assert.AreEqual(0f, state.Opacity);
            Assert.AreEqual(0f, state.Intensity01);
        }

        [Test]
        public void SidewaysSpeedAboveThreshold_StartsOnceAndKeepsSliding()
        {
            var rule = new SkidRule();
            var first = rule.Step(Slip(5f, 1.3f), S);
            var second = rule.Step(Slip(5f, 1.3f), S);
            Assert.IsTrue(first.Sliding);
            Assert.IsTrue(first.SlideStarted);
            Assert.IsTrue(second.Sliding);
            Assert.IsFalse(second.SlideStarted);
        }

        [Test]
        public void ExactlyAtThreshold_DoesNotStart() => Assert.IsFalse(new SkidRule().Step(Slip(5f, 1.2f), S).Sliding);

        [Test]
        public void Hysteresis_SlideEndsOnlyBelowTheExitShare()
        {
            var rule = new SkidRule();
            rule.Step(Slip(5f, 2f), S);
            Assert.IsTrue(rule.Step(Slip(5f, 1.0f), S).Sliding, "between exit (0.9) and start (1.2) keeps sliding");
            var ended = rule.Step(Slip(5f, 0.8f), S);
            Assert.IsFalse(ended.Sliding);
            Assert.IsTrue(ended.SlideEnded);
            Assert.IsFalse(rule.Step(Slip(5f, 1.0f), S).Sliding, "and a gentle wobble does not restart it");
        }

        [Test]
        public void SlideDirectionDoesNotMatter() => Assert.IsTrue(new SkidRule().Step(Slip(5f, -2f), S).Sliding);

        [Test]
        public void ShiftHeldAboveTwo_Slides_BelowItDoesNot()
        {
            Assert.IsTrue(new SkidRule().Step(Slip(2.5f, 0f, 0f, true), S).Sliding);
            Assert.IsFalse(new SkidRule().Step(Slip(1.9f, 0f, 0f, true), S).Sliding);
            Assert.IsFalse(new SkidRule().Step(Slip(5f, 0f, 0f, false), S).Sliding);
        }

        [Test]
        public void Opacity_GentleIsFaintAndFullDriftIsDarkAndCapped()
        {
            var gentle = new SkidRule().Step(Slip(10f, 1.3f), S).Opacity;
            var full = new SkidRule().Step(Slip(5f, 6f), S).Opacity;
            var huge = new SkidRule().Step(Slip(0.1f, 6f), S).Opacity;
            Assert.Greater(gentle, 0f);
            Assert.Less(gentle, full);
            Assert.AreEqual(0.8f, full, 1e-4f);
            Assert.AreEqual(0.8f, huge, 1e-4f, "never above the maximum");
        }

        [Test]
        public void Opacity_FollowsTheSlipAngle()
        {
            var state = new SkidRule().Step(Slip(5f, 5f * Mathf.Tan(20f * Mathf.Deg2Rad)), S);
            Assert.AreEqual(0.5f, state.Intensity01, 0.01f);
            Assert.AreEqual(0.8f * (0.3f + 0.7f * 0.5f), state.Opacity, 0.01f);
        }

        [Test]
        public void HardBraking_StartsOnceAboveFourWhileBrakingAgainstTravel()
        {
            var rule = new SkidRule();
            Assert.IsFalse(rule.Step(Slip(3.9f, 0f, -1f), S).HardBrakeStarted, "too slow");
            Assert.IsFalse(rule.Step(Slip(6f, 0f, 1f), S).HardBrakeStarted, "accelerating is not braking");
            Assert.IsTrue(rule.Step(Slip(6f, 0f, -1f), S).HardBrakeStarted);
            Assert.IsFalse(rule.Step(Slip(5f, 0f, -1f), S).HardBrakeStarted, "only the start");
            Assert.IsFalse(rule.Step(Slip(2f, 0f, -1f), S).HardBrakeStarted);
            Assert.IsTrue(rule.Step(Slip(6f, 0f, -1f), S).HardBrakeStarted, "a new brake after slowing down");
        }

        [Test]
        public void HardBraking_ReversingAndPressingForwardCounts()
            => Assert.IsTrue(new SkidRule().Step(Slip(-5f, 0f, 1f), S).HardBrakeStarted);

        [Test]
        public void Reset_EndsARunningSlideOnce()
        {
            var rule = new SkidRule();
            rule.Step(Slip(5f, 3f), S);
            Assert.IsTrue(rule.Reset().SlideEnded);
            Assert.IsFalse(rule.Reset().SlideEnded);
            Assert.IsFalse(rule.Sliding);
        }
    }
}
