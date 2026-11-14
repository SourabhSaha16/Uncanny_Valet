using System;
using NUnit.Framework;
using UncannyValet.Core.Art;
using UncannyValet.Lot;
using UnityEngine;

namespace UncannyValet.Tests.Lot
{
    /// <summary>The sprite cache (each recipe drawn once per lot), the prop builder, the sprite override and the allocation-free animation.</summary>
    public sealed class PropBuilderTests
    {
        private WorldPalette _palette;
        private GameObject _lot;

        [SetUp]
        public void SetUp()
        {
            _palette = ShippedArt.Named<WorldPalette>("MidgardPalette");
            _lot = new GameObject("Lot");
        }

        [TearDown]
        public void TearDown() => UnityEngine.Object.DestroyImmediate(_lot);

        [Test]
        public void TheSameRecipe_IsDrawnOncePerLot_NoMatterHowManyPlacements()
        {
            var tree = ShippedArt.Named<PropRecipe>("MidgardTree");
            var builder = new PropBuilder(_palette);
            var group = builder.Build(_lot.transform, new[]
            {
                new PropPlacement(tree, new Vector2(1, 1)), new PropPlacement(tree, new Vector2(5, 1)), new PropPlacement(tree, new Vector2(9, 1)),
            });
            Assert.AreEqual(3, group.childCount);
            Assert.AreEqual(1, builder.GeneratedCount);
            var first = group.GetChild(0).GetComponentInChildren<SpriteRenderer>().sprite;
            Assert.AreSame(first, group.GetChild(2).GetComponentInChildren<SpriteRenderer>().sprite, "placements share one sprite");
        }

        [Test]
        public void ASprite_IsPointFiltered_AtThirtyTwoPixelsPerUnit_AndTheSizeOfTheRecipe()
        {
            var cache = new PropSpriteCache(_palette);
            var stall = ShippedArt.Named<PropRecipe>("MidgardStallRed");
            var sprite = cache.Get(stall)[0];
            Assert.AreEqual(FilterMode.Point, sprite.texture.filterMode);
            Assert.AreEqual(32f, sprite.pixelsPerUnit);
            Assert.AreEqual(3f, sprite.bounds.size.x, 0.001f);
            Assert.AreEqual(2f, sprite.bounds.size.y, 0.001f);
            Assert.AreSame(cache.Get(stall), cache.Get(stall));
        }

        [Test]
        public void ASpriteOverride_IsUsedInsteadOfTheDrawing()
        {
            var texture = new Texture2D(32, 32);
            var art = Sprite.Create(texture, new Rect(0, 0, 32, 32), Vector2.one * 0.5f, PropRecipe.PixelsPerUnit);
            var recipe = PropRecipe.Create("Swapped", Vector2Int.one, new[] { PropLayer.Create("L", new[] { new PropShape(PropShapeKind.Rect, new RectInt(0, 0, 4, 4), PaletteSlot.Wall) }) },
                false, default, null, art);
            var group = new PropBuilder(_palette).Build(_lot.transform, new[] { new PropPlacement(recipe, Vector2.zero) });
            var renderers = group.GetComponentsInChildren<SpriteRenderer>();
            Assert.AreEqual(1, renderers.Length);
            Assert.AreSame(art, renderers[0].sprite);
            UnityEngine.Object.DestroyImmediate(recipe);
            UnityEngine.Object.DestroyImmediate(texture);
        }

        [Test]
        public void TheFerrisWheel_IsPlacedSolid_WithOneRendererPerLayer_AndAnimatedLayers()
        {
            var ferris = ShippedArt.Named<PropRecipe>("MidgardFerrisWheel");
            var group = new PropBuilder(_palette).Build(_lot.transform, new[] { new PropPlacement(ferris, new Vector2(3, 4)) });
            var root = group.GetChild(0);
            Assert.AreEqual(new Vector3(3, 4, 0), root.localPosition);
            Assert.IsNotNull(root.GetComponent<BoxCollider2D>());
            Assert.AreEqual(ferris.Layers.Count, root.GetComponentsInChildren<SpriteRenderer>().Length);
            Assert.AreEqual(2, group.GetComponent<PropAnimator>().Count, "the wheel and its bulbs turn");
        }

        [Test]
        public void ABrokenRecipe_IsSkippedWithAnError()
        {
            var broken = PropRecipe.Create("Broken", Vector2Int.one, new PropLayer[0]);
            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("Prop skipped"));
            var group = new PropBuilder(_palette).Build(_lot.transform, new[] { new PropPlacement(broken, Vector2.zero) });
            Assert.AreEqual(0, group.childCount);
            UnityEngine.Object.DestroyImmediate(broken);
        }

        [Test]
        public void NoPlacements_BuildNothing()
        {
            Assert.IsNull(new PropBuilder(_palette).Build(_lot.transform, new PropPlacement[0]));
        }

        [Test]
        public void TheMotionRules_SpinBlinkAndFlicker()
        {
            Assert.AreEqual(90f, PropMotion.SpinAngle(10f, 9f), 0.001f);
            Assert.AreEqual(350f, PropMotion.SpinAngle(1f, -10f), 0.001f);
            Assert.IsTrue(PropMotion.BlinkOn(0.2f, 1f));
            Assert.IsFalse(PropMotion.BlinkOn(0.7f, 1f));
            Assert.IsTrue(PropMotion.BlinkOn(1.2f, 1f));
            var low = float.MaxValue;
            for (var t = 0f; t < 20f; t += 0.1f) low = Mathf.Min(low, PropMotion.Flicker(t, 2f, 0.4f, 1f));
            Assert.GreaterOrEqual(low, 0.6f - 0.001f);
            Assert.LessOrEqual(PropMotion.Flicker(3f, 2f, 0.4f, 1f), 1f);
        }

        [Test]
        public void TickingTheAnimation_AllocatesNothing()
        {
            var ferris = ShippedArt.Named<PropRecipe>("MidgardFerrisWheel");
            var sign = ShippedArt.Named<PropRecipe>("MidgardOpenSign");
            var group = new PropBuilder(_palette).Build(_lot.transform, new[] { new PropPlacement(ferris, Vector2.zero), new PropPlacement(sign, Vector2.one) });
            var animator = group.GetComponent<PropAnimator>();
            animator.Tick(0.5f);
            GC.Collect();
            var before = GC.GetTotalMemory(false);
            for (var i = 0; i < 2000; i++) animator.Tick(i * 0.016f);
            Assert.Less(GC.GetTotalMemory(false) - before, 4096, "the animation update must not allocate");
        }
    }
}
