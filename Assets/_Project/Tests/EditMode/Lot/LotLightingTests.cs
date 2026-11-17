using System;
using NUnit.Framework;
using UncannyValet.Core.Art;
using UncannyValet.Lot;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.TestTools;

namespace UncannyValet.Tests.Lot
{
    /// <summary>The lighting preset rules (at most 16 point lights, positive radii) and applying a preset to a lot.</summary>
    public sealed class LotLightingTests
    {
        private GameObject _lot;

        [SetUp]
        public void SetUp() => _lot = new GameObject("Lot");

        [TearDown]
        public void TearDown() => UnityEngine.Object.DestroyImmediate(_lot);

        private static PointLightSpec[] Lights(int count, float radius = 3f)
        {
            var lights = new PointLightSpec[count];
            for (var i = 0; i < count; i++) lights[i] = new PointLightSpec(new Vector2(i, 0), radius, Color.yellow, 1f);
            return lights;
        }

        [Test]
        public void SixteenLights_AreFine_SeventeenAreNot()
        {
            Assert.IsTrue(LightingPreset.Create("Sixteen", Color.white, 1f, Lights(LightingPreset.MaxLights)).TryValidate(out var problem), problem);
            Assert.IsFalse(LightingPreset.Create("Seventeen", Color.white, 1f, Lights(LightingPreset.MaxLights + 1)).TryValidate(out _));
        }

        [Test]
        public void ALightNeedsAPositiveRadius_AndTheGlobalLightACleanIntensity()
        {
            Assert.IsFalse(LightingPreset.Create("NoRadius", Color.white, 1f, Lights(2, 0f)).TryValidate(out _));
            Assert.IsFalse(LightingPreset.Create("Negative", Color.white, -1f, Lights(0)).TryValidate(out _));
        }

        [Test]
        public void ShippedPresets_AreValid_WithinTheCap()
        {
            var presets = ShippedArt.All<LightingPreset>();
            Assert.AreEqual(4, presets.Count, "Midgard day and night, Vanaheim sunny afternoon and stormy evening");
            foreach (var preset in presets)
            {
                Assert.IsTrue(preset.TryValidate(out var problem), problem);
                Assert.LessOrEqual(preset.Lights.Count, LightingPreset.MaxLights);
            }
            Assert.Greater(ShippedArt.Named<LightingPreset>("MidgardNight").Lights.Count, 0, "the string lights");
            Assert.Less(ShippedArt.Named<LightingPreset>("MidgardNight").GlobalIntensity, ShippedArt.Named<LightingPreset>("MidgardDay").GlobalIntensity);
        }

        [Test]
        public void Apply_BuildsOneGlobalAndThePointLights_WithoutShadows()
        {
            var preset = LightingPreset.Create("Test", new Color(0.5f, 0.6f, 1f), 0.6f, Lights(5));
            var group = LotLighting.Apply(_lot.transform, preset);
            var lights = group.GetComponentsInChildren<Light2D>();
            Assert.AreEqual(6, lights.Length);
            var global = 0;
            foreach (var light in lights)
            {
                Assert.IsFalse(light.shadowsEnabled);
                if (light.lightType == Light2D.LightType.Global) global++;
            }
            Assert.AreEqual(1, global);
            UnityEngine.Object.DestroyImmediate(preset);
        }

        [Test]
        public void NoPreset_BuildsNothing_AndAnInvalidOneIsRefused()
        {
            Assert.IsNull(LotLighting.Apply(_lot.transform, null));
            var invalid = LightingPreset.Create("Invalid", Color.white, 1f, Lights(LightingPreset.MaxLights + 1));
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("not applied"));
            Assert.IsNull(LotLighting.Apply(_lot.transform, invalid));
            Assert.AreEqual(0, _lot.transform.childCount);
            UnityEngine.Object.DestroyImmediate(invalid);
        }

        [Test]
        public void FlickeringLights_DimWithinTheirDepth_AndAllocateNothing()
        {
            var specs = new[] { new PointLightSpec(Vector2.zero, 3f, Color.yellow, 1f, 0.4f, 2f), new PointLightSpec(Vector2.one, 3f, Color.yellow, 1f) };
            var preset = LightingPreset.Create("Flicker", Color.white, 1f, specs);
            var group = LotLighting.Apply(_lot.transform, preset);
            var flicker = group.GetComponent<LightFlicker>();
            Assert.AreEqual(1, flicker.Count, "only the light with a flicker is driven");
            flicker.Tick(0.3f);
            GC.Collect();
            var before = GC.GetTotalMemory(false);
            for (var i = 0; i < 2000; i++) flicker.Tick(i * 0.016f);
            Assert.Less(GC.GetTotalMemory(false) - before, 4096, "the flicker update must not allocate");
            foreach (var light in group.GetComponentsInChildren<Light2D>())
                if (light.lightType == Light2D.LightType.Point) Assert.GreaterOrEqual(light.intensity, 0f);
            UnityEngine.Object.DestroyImmediate(preset);
        }
    }
}
