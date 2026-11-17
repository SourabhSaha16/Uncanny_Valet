using NUnit.Framework;
using UncannyValet.Cars;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UnityEngine;

namespace UncannyValet.Tests.Cars
{
    public class DriftStateTests
    {
        private const float Dt = 0.02f;
        private static readonly DriftKickSettings Kick = new DriftKickSettings(3f, 0.1f, 1.3f, 0.25f, 0.2f, 1f);
        private static readonly DriftSettings Spec = new DriftSettings(8f, 0.6f, 3f, 6f, 0.75f, 0.2f, 1f);

        private static void Run(DriftState state, bool drift, float steer, float speed, int steps)
        {
            for (var i = 0; i < steps; i++) state.Advance(new DriveInput(1f, steer, drift), speed, speed, Kick, Dt);
        }

        [Test]
        public void EntryKickPointsInTheSteerDirectionAndFades()
        {
            var right = new DriftState();
            right.Advance(new DriveInput(1f, 1f, true), 5f, 5f, Kick, Dt);
            Assert.Less(right.KickYaw, 0f, "right steer is clockwise, so negative yaw");
            var left = new DriftState();
            left.Advance(new DriveInput(1f, -1f, true), 5f, 5f, Kick, Dt);
            Assert.Greater(left.KickYaw, 0f);
            Run(right, true, 1f, 5f, 6);
            Assert.AreEqual(0f, right.KickYaw, 1e-5f);
        }

        [Test]
        public void KickOnlyOnTheFrameTheButtonStartsAndAboveMinSpeed()
        {
            var held = new DriftState();
            Run(held, true, 1f, 5f, 10);
            Assert.AreEqual(0f, held.KickYaw, 1e-5f);
            var slow = new DriftState();
            slow.Advance(new DriveInput(1f, 1f, true), 0.5f, 0.5f, Kick, Dt);
            Assert.AreEqual(0f, slow.KickYaw, 1e-5f);
            Assert.AreEqual(0f, slow.GripBlend, 1e-5f);
            var straight = new DriftState();
            straight.Advance(new DriveInput(1f, 0f, true), 5f, 5f, Kick, Dt);
            Assert.AreEqual(0f, straight.KickYaw, 1e-5f);
        }

        [Test]
        public void KickFlipsInReverse()
        {
            var state = new DriftState();
            state.Advance(new DriveInput(-1f, 1f, true), 5f, -5f, Kick, Dt);
            Assert.Greater(state.KickYaw, 0f);
        }

        [Test]
        public void TurnBoostOnlyWhileHeld()
        {
            var state = new DriftState();
            Run(state, true, 1f, 5f, 2);
            Assert.AreEqual(1.3f, state.TurnScale, 1e-5f);
            Run(state, false, 1f, 5f, 1);
            Assert.AreEqual(1f, state.TurnScale, 1e-5f);
        }

        [Test]
        public void GripStaysLooseForTheHoldThenBlendsBack()
        {
            var state = new DriftState();
            Run(state, true, 1f, 5f, 3);
            Assert.AreEqual(1f, state.GripBlend, 1e-5f);
            Run(state, false, 1f, 5f, 10);
            Assert.AreEqual(1f, state.GripBlend, 1e-5f, "0.2 s after release is inside the 0.25 s hold");
            Run(state, false, 1f, 5f, 7);
            Assert.That(state.GripBlend, Is.GreaterThan(0f).And.LessThan(1f), "0.34 s: mid blend");
            Run(state, false, 1f, 5f, 20);
            Assert.AreEqual(0f, state.GripBlend, 1e-5f);
        }

        [Test]
        public void NeverDriftedMeansNormalGrip()
        {
            var state = new DriftState();
            Run(state, false, 1f, 5f, 3);
            Assert.AreEqual(0f, state.GripBlend, 1e-5f);
        }

        [Test]
        public void GripBlendMixesNormalAndDriftGrip()
        {
            var normal = DriftModel.Grip(9f, 0f, Spec);
            var drift = DriftModel.Grip(9f, 1f, Spec);
            Assert.AreEqual(0.75f, normal, 1e-5f);
            Assert.AreEqual(0.15f, drift, 1e-5f);
            Assert.AreEqual((normal + drift) * 0.5f, DriftModel.Grip(9f, 0.5f, Spec), 1e-5f);
            Assert.AreEqual(drift, DriftModel.Grip(9f, 3f, Spec), 1e-5f, "blend is clamped");
        }

        [Test]
        public void BoolStepStillMatchesFullBlendAboveMinSpeed()
        {
            var v = new Vector2(1f, 5f);
            Assert.AreEqual(DriftModel.Step(v, Vector2.up, 1f, Spec, Dt), DriftModel.Step(v, Vector2.up, true, Spec, Dt));
            Assert.AreEqual(DriftModel.Step(v, Vector2.up, 0f, Spec, Dt), DriftModel.Step(v, Vector2.up, false, Spec, Dt));
        }

        [Test]
        public void FalloffIsOneUpToFullSpeedThenEasesToTheFloor()
        {
            Assert.AreEqual(1f, SteeringModel.Falloff(6f, 6f, 0.12f, 0.55f), 1e-5f);
            Assert.AreEqual(0.76f, SteeringModel.Falloff(8f, 6f, 0.12f, 0.55f), 1e-5f);
            Assert.AreEqual(0.55f, SteeringModel.Falloff(30f, 6f, 0.12f, 0.55f), 1e-5f);
            Assert.AreEqual(0.76f, SteeringModel.Falloff(-8f, 6f, 0.12f, 0.55f), 1e-5f, "reverse falls off too");
        }

        [Test]
        public void FastCarsTurnLessButRadiusStillGrows()
        {
            var max = TurningRadius.MaxTurnRate(6f, 2.2f);
            var at6 = Mathf.Abs(SteeringModel.TurnRate(1f, 6f, max, 6f, 0.12f, 0.55f));
            var at9 = Mathf.Abs(SteeringModel.TurnRate(1f, 9f, max, 6f, 0.12f, 0.55f));
            Assert.AreEqual(max, at6, 1e-4f);
            Assert.Less(at9, at6);
            Assert.Greater(at9, max * 0.5f);
            Assert.Greater(SteeringModel.Radius(9f, 2.2f, max, 6f, 0.12f, 0.55f), SteeringModel.Radius(9f, 2.2f, max));
            Assert.AreEqual(2.2f, SteeringModel.Radius(3f, 2.2f, max, 6f, 0.12f, 0.55f), 1e-3f);
        }

        [Test]
        public void NewKeysAreSeededAndFollowTheLiveValues()
        {
            var config = ScriptableObject.CreateInstance<CarHandlingConfig>();
            var live = new CarHandlingLive(config);
            var kick = live.DriftKick;
            Assert.AreEqual(1.3f, kick.TurnScale);
            Assert.AreEqual(0.25f, kick.HoldSeconds);
            Assert.AreEqual(0.2f, kick.BlendSeconds);
            Assert.AreEqual(0.12f, live.TurnFalloff, 1e-5f);
            Assert.IsTrue(live.TrySet(TuningKey.HandlingDriftTurnScale, 1.6f));
            Assert.IsTrue(live.TrySet(TuningKey.HandlingTurnFalloff, 0.2f));
            Assert.AreEqual(1.6f, live.DriftKick.TurnScale, 1e-5f);
            Assert.AreEqual(0.2f, live.TurnFalloff, 1e-5f);
            Object.DestroyImmediate(config);
        }
    }
}
