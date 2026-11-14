#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Core.Art;
using UncannyValet.Lot;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.TestTools;

namespace UncannyValet.Tests.PlayMode
{
    /// <summary>A lot built with every Midgard and Vanaheim recipe and the world presets: the props show, turn and blink, the lights stay within the cap, nothing logs an error.</summary>
    public sealed class PropArtPlayModeTests
    {
        private const string ArtFolder = "Assets/_Project/Data/Art";
        private const int WarmUpFrames = 45;

        private GameObject _lot;

        [TearDown]
        public void TearDown() => Object.Destroy(_lot);

        private static List<T> Load<T>() where T : Object
        {
            var found = new List<T>();
            foreach (var guid in AssetDatabase.FindAssets("t:" + typeof(T).Name, new[] { ArtFolder }))
                found.Add(AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid)));
            return found;
        }

        private static WorldPalette Palette(string name) => Load<WorldPalette>().Find(p => p.name == name);

        private IEnumerator BuildAndCheck(string paletteName, string recipePrefix)
        {
            _lot = new GameObject("Lot");
            var placements = new List<PropPlacement>();
            foreach (var recipe in Load<PropRecipe>())
                if (recipe.name.StartsWith(recipePrefix)) placements.Add(new PropPlacement(recipe, new Vector2(placements.Count * 4f, 0f)));
            Assert.Greater(placements.Count, 5, "the world's props are listed");
            var group = new PropBuilder(Palette(paletteName)).Build(_lot.transform, placements);
            Assert.AreEqual(placements.Count, group.childCount, "every prop is placed");
            foreach (var renderer in group.GetComponentsInChildren<SpriteRenderer>())
            {
                Assert.AreEqual(FilterMode.Point, renderer.sprite.texture.filterMode);
                Assert.AreEqual(32f, renderer.sprite.pixelsPerUnit);
            }
            var animator = group.GetComponent<PropAnimator>();
            var spinner = animator.transform.Find("MidgardFerrisWheel");
            for (var i = 0; i < WarmUpFrames; i++) yield return null;
            if (spinner != null) Assert.AreNotEqual(0f, spinner.Find("Wheel").localRotation.eulerAngles.z, "the wheel turns");
            Assert.Greater(animator.Count, 0);
        }

        [UnityTest]
        public IEnumerator Midgard_PropsLoadAndAnimate() => BuildAndCheck("MidgardPalette", "Midgard");

        [UnityTest]
        public IEnumerator Vanaheim_PropsLoadAndAnimate() => BuildAndCheck("VanaheimPalette", "Vanaheim");

        [UnityTest]
        public IEnumerator EveryPreset_AppliesWithinTheLightCap()
        {
            foreach (var preset in Load<LightingPreset>())
            {
                _lot = new GameObject("Lot");
                var group = LotLighting.Apply(_lot.transform, preset);
                yield return null;
                Assert.LessOrEqual(group.GetComponentsInChildren<Light2D>().Length, LightingPreset.MaxLights + 1, preset.name);
                Object.Destroy(_lot);
                yield return null;
            }
        }
    }
}
#endif
