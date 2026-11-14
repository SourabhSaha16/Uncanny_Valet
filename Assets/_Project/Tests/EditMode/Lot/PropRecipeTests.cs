using NUnit.Framework;
using UncannyValet.Core.Art;
using UnityEngine;

namespace UncannyValet.Tests.Lot
{
    /// <summary>Prop recipe and world palette validation, and the shipped Midgard and Vanaheim assets.</summary>
    public sealed class PropRecipeTests
    {
        private static readonly string[] Expected =
        {
            "MidgardOpenSign", "MidgardTree", "MidgardStallRed", "MidgardStallGreen", "MidgardStallGold", "MidgardFerrisWheel",
            "VanaheimBarn", "VanaheimWheat", "VanaheimHayBale", "VanaheimFence", "VanaheimTrough", "VanaheimBanner", "VanaheimStall",
        };

        private static PropShape Fill(int x, int y, int w, int h) => new PropShape(PropShapeKind.Rect, new RectInt(x, y, w, h), PaletteSlot.Wall);

        private static PropRecipe Recipe(Vector2Int size, params PropShape[] shapes)
            => PropRecipe.Create("Test", size, new[] { PropLayer.Create("L", shapes) });

        [Test]
        public void AGoodRecipe_Validates_AndSizesOnTheGrid()
        {
            var recipe = Recipe(new Vector2Int(2, 1), Fill(0, 0, 64, 32));
            Assert.IsTrue(recipe.TryValidate(out var problem), problem);
            Assert.AreEqual(new Vector2Int(64, 32), recipe.SizePixels);
        }

        [Test]
        public void ABadSizeOrShape_IsRejected()
        {
            Assert.IsFalse(Recipe(new Vector2Int(0, 1), Fill(0, 0, 1, 1)).TryValidate(out _), "no size");
            Assert.IsFalse(Recipe(new Vector2Int(PropRecipe.MaxUnits + 1, 1), Fill(0, 0, 1, 1)).TryValidate(out _), "too big");
            Assert.IsFalse(Recipe(Vector2Int.one, Fill(20, 0, 20, 4)).TryValidate(out _), "shape leaves the picture");
            Assert.IsFalse(Recipe(Vector2Int.one, Fill(0, 0, 0, 4)).TryValidate(out _), "empty shape");
            Assert.IsFalse(PropRecipe.Create("Bare", Vector2Int.one, new PropLayer[0]).TryValidate(out _), "no layers and no override");
        }

        [Test]
        public void ASolidRecipeNeedsACollider_AndAMovingLayerNeedsASpeed()
        {
            var layer = PropLayer.Create("L", new[] { Fill(0, 0, 4, 4) });
            Assert.IsFalse(PropRecipe.Create("Solid", Vector2Int.one, new[] { layer }, true).TryValidate(out _));
            var still = PropLayer.Create("Spin", new[] { Fill(0, 0, 4, 4) }, true, false, PropAnimation.Spin, 0f);
            Assert.IsFalse(PropRecipe.Create("Still", Vector2Int.one, new[] { still }).TryValidate(out _));
        }

        [Test]
        public void ASpriteOverride_ReplacesTheDrawing_AndMustMatchTheGridSize()
        {
            var texture = new Texture2D(32, 32);
            var good = Sprite.Create(texture, new Rect(0, 0, 32, 32), Vector2.one * 0.5f, PropRecipe.PixelsPerUnit);
            var bad = Sprite.Create(texture, new Rect(0, 0, 16, 16), Vector2.one * 0.5f, PropRecipe.PixelsPerUnit);
            Assert.IsTrue(PropRecipe.Create("Override", Vector2Int.one, new PropLayer[0], false, default, null, good).TryValidate(out var problem), problem);
            Assert.IsFalse(PropRecipe.Create("Wrong", Vector2Int.one, new PropLayer[0], false, default, null, bad).TryValidate(out _));
            Object.DestroyImmediate(texture);
        }

        [Test]
        public void ThePalette_NeedsAnOpaqueColourPerSlot()
        {
            var colours = new Color[WorldPalette.SlotCount];
            for (var i = 0; i < colours.Length; i++) colours[i] = Color.white;
            Assert.IsTrue(WorldPalette.Create("Full", colours).TryValidate(out var problem), problem);
            colours[(int)PaletteSlot.Roof] = Color.clear;
            Assert.IsFalse(WorldPalette.Create("Hole", colours).TryValidate(out _));
            Assert.IsFalse(WorldPalette.Create("Short", new Color[3]).TryValidate(out _));
        }

        [Test]
        public void ShippedAssets_AllExistAndValidate()
        {
            foreach (var name in Expected)
            {
                var recipe = ShippedArt.Named<PropRecipe>(name);
                Assert.IsNotNull(recipe, name + " is missing; run Setup/Build Everything");
                Assert.IsTrue(recipe.TryValidate(out var problem), problem);
            }
            foreach (var palette in ShippedArt.All<WorldPalette>()) Assert.IsTrue(palette.TryValidate(out var problem), problem);
            Assert.AreEqual(2, ShippedArt.All<WorldPalette>().Count, "one palette each for Midgard and Vanaheim");
        }

        [Test]
        public void TheFerrisWheel_IsSolidAndTurns_TheOpenSignBlinks_TheTroughTurnsSlowly()
        {
            var ferris = ShippedArt.Named<PropRecipe>("MidgardFerrisWheel");
            Assert.IsTrue(ferris.Solid);
            Assert.IsTrue(ferris.Layers.Count > 1 && System.Linq.Enumerable.Any(ferris.Layers, l => l.Animation == PropAnimation.Spin));
            Assert.IsTrue(System.Linq.Enumerable.Any(ShippedArt.Named<PropRecipe>("MidgardOpenSign").Layers, l => l.Animation == PropAnimation.Blink));
            var trough = System.Linq.Enumerable.First(ShippedArt.Named<PropRecipe>("VanaheimTrough").Layers, l => l.Animation == PropAnimation.Spin);
            var wheel = System.Linq.Enumerable.First(ferris.Layers, l => l.Animation == PropAnimation.Spin);
            Assert.Less(Mathf.Abs(trough.Speed), 45f, "slowly turning");
            Assert.Greater(Mathf.Abs(wheel.Speed), 0f);
        }
    }
}
