using NUnit.Framework;
using UncannyValet.Cars;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UnityEngine;

namespace UncannyValet.Tests.Cars
{
    /// <summary>The pure parts of the mud effect: the blend, the modifier on the drive input, the in-mud rule and the grip loss scale.</summary>
    public sealed class MudEffectTests
    {
        private sealed class Patches : IMudPatches
        {
            private readonly MudPatch _patch;
            public Patches(MudPatch patch) { _patch = patch; }
            public bool IsInside(Vector2 point) => _patch.Contains(point);
        }

        private static readonly MudSettings Mud = new MudSettings(0.6f, 0.5f, 0.4f, 0.3f);

        [Test]
        public void Blend_IsFullInsideAndFadesLinearlyOverTheReleaseTime()
        {
            var blend = new MudBlend();
            blend.Step(true, 0.02f, 0.3f);
            Assert.AreEqual(1f, blend.Weight);
            blend.Step(false, 0.15f, 0.3f);
            Assert.AreEqual(0.5f, blend.Weight, 1e-5f);
            blend.Step(false, 0.15f, 0.3f);
            Assert.AreEqual(0f, blend.Weight, 1e-5f);
            blend.Step(false, 0.15f, 0.3f);
            Assert.AreEqual(0f, blend.Weight, "never below zero");
        }

        [Test]
        public void Blend_ZeroReleaseEndsAtOnce_AndReentryIsInstant()
        {
            var blend = new MudBlend();
            blend.Step(true, 0.02f, 0f);
            blend.Step(false, 0.02f, 0f);
            Assert.AreEqual(0f, blend.Weight);
            blend.Step(false, 0.1f, 0.3f);
            blend.Step(true, 0.02f, 0.3f);
            Assert.AreEqual(1f, blend.Weight);
        }

        [Test]
        public void Modifier_InMud_ScalesSpeedAccelerationAndGripLossButNotTheDriversInput()
        {
            var blend = new MudBlend();
            blend.Step(true, 0.02f, 0.3f);
            var result = new MudModifier(blend, Mud).Modify(new DriveInput(0.7f, -0.4f, true));
            Assert.AreEqual(0.6f, result.TopSpeedScale, 1e-5f);
            Assert.AreEqual(0.5f, result.AccelerationScale, 1e-5f);
            Assert.AreEqual(0.4f, result.GripLossScale, 1e-5f);
            Assert.AreEqual(0.7f, result.Throttle);
            Assert.AreEqual(-0.4f, result.Steer);
            Assert.IsTrue(result.Drift);
        }

        [Test]
        public void Modifier_HalfwayThroughTheRelease_IsHalfwayBetweenMudAndClear()
        {
            var blend = new MudBlend();
            blend.Step(true, 0.02f, 0.3f);
            blend.Step(false, 0.15f, 0.3f);
            var result = new MudModifier(blend, Mud).Modify(new DriveInput(1f, 0f, false));
            Assert.AreEqual(0.8f, result.TopSpeedScale, 1e-5f);
            Assert.AreEqual(0.7f, result.GripLossScale, 1e-5f);
        }

        [Test]
        public void Modifier_ClearOfMud_ChangesNothing_AndStacksWithABoost()
        {
            var input = new DriveInput(1f, 0f, false, 2f, 3f);
            var clear = new MudModifier(new MudBlend(), Mud).Modify(input);
            Assert.AreEqual(2f, clear.TopSpeedScale);
            Assert.AreEqual(3f, clear.AccelerationScale);
            var blend = new MudBlend();
            blend.Step(true, 0.02f, 0.3f);
            Assert.AreEqual(1.2f, new MudModifier(blend, Mud).Modify(input).TopSpeedScale, 1e-5f);
        }

        [Test]
        public void Probe_CentreInside_EdgeCounts_FarOutsideDoesNot()
        {
            var patches = new Patches(new MudPatch(Vector2.zero, 4f));
            var size = new Vector2(1f, 2f);
            Assert.IsTrue(CarMudProbe.IsInMud(patches, Vector2.zero, Vector2.up, size));
            Assert.IsTrue(CarMudProbe.IsInMud(patches, new Vector2(2f, 0f), Vector2.up, size), "the rim counts");
            Assert.IsFalse(CarMudProbe.IsInMud(patches, new Vector2(6f, 0f), Vector2.up, size));
        }

        [Test]
        public void Probe_AWheelPointInsideIsEnough_EvenWithTheCentreOutside()
        {
            var patches = new Patches(new MudPatch(Vector2.zero, 2f));
            var size = new Vector2(1f, 2f);
            // Centre 1.6 above the patch edge-line; the rear wheels (0.8 back) are 0.8 from the centre, inside the radius 1.
            var nose = new Vector2(0f, 1.6f);
            Assert.IsFalse(patches.IsInside(nose));
            Assert.IsTrue(CarMudProbe.IsInMud(patches, nose, Vector2.up, size));
        }

        [Test]
        public void Probe_NoPatches_IsNeverInMud() => Assert.IsFalse(CarMudProbe.IsInMud(null, Vector2.zero, Vector2.up, Vector2.one));

        [Test]
        public void GripLossScale_KeepsAShareOfTheLoss_AndOneChangesNothing()
        {
            var s = new DriftSettings(5f, 0.5f, 5f, 15f, 0.6f, 0.3f, 2f);
            var normal = DriftModel.Grip(10f, true, s);
            Assert.AreEqual(normal, DriftModel.Grip(10f, true, s, 1f));
            Assert.AreEqual(1f - (1f - normal) * 0.4f, DriftModel.Grip(10f, true, s, 0.4f), 1e-5f);
            Assert.AreEqual(1f, DriftModel.Grip(0f, false, s, 0.4f), "no loss, nothing to scale");
        }

        [Test]
        public void ReducedGripLoss_SlidesLess()
        {
            var s = new DriftSettings(5f, 0.5f, 5f, 15f, 0.6f, 0.3f, 2f);
            var velocity = new Vector2(3f, 10f); // facing up, moving up and to the right
            var normal = DriftModel.Step(velocity, Vector2.up, true, s, 0.02f);
            var muddy = DriftModel.Step(velocity, Vector2.up, true, s, 0.02f, 0.4f);
            Assert.Less(muddy.x, normal.x, "more of the sideways speed is damped away");
        }

        [Test]
        public void Settings_ReadsTheSeededValues_AndFallsBackToDefaultsWhenUnseeded()
        {
            var tuning = new TuningStore(new EventBus());
            var unseeded = MudSettings.Read(tuning);
            Assert.AreEqual(MudRows.DefaultTopSpeedScale, unseeded.TopSpeedScale);
            Assert.AreEqual(MudRows.DefaultReleaseSeconds, unseeded.ReleaseSeconds);
            MudRows.Seed(tuning, 0.5f, 0.4f, 0.3f, 0.2f);
            var read = MudSettings.Read(tuning);
            Assert.AreEqual(0.5f, read.TopSpeedScale);
            Assert.AreEqual(0.4f, read.AccelerationScale);
            Assert.AreEqual(0.3f, read.GripLossKept);
            Assert.AreEqual(0.2f, read.ReleaseSeconds);
        }

        [Test]
        public void Rows_AreRegisteredAdvancedAndInRange()
        {
            var found = 0;
            foreach (var info in TuningCatalog.All)
            {
                if (info.Group != MudRows.Group) continue;
                found++;
                Assert.IsTrue(info.Advanced, info.Label);
                Assert.IsNotEmpty(info.Tooltip, info.Label);
                Assert.Less(info.Min, info.Max, info.Label);
            }
            Assert.AreEqual(4, found);
        }
    }
}
