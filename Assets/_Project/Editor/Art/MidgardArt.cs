using System.Collections.Generic;
using UncannyValet.Core.Art;
using UnityEngine;
using static UncannyValet.EditorTools.Art.RecipeKit;

namespace UncannyValet.EditorTools.Art
{
    /// <summary>The Midgard art assets: warm tans, greens, neon red and night blues; trees, the OPEN sign, three fair stalls, the Ferris wheel, day and night lighting.</summary>
    internal static class MidgardArt
    {
        public const string Folder = ArtAssets.Root + "Midgard/";
        public const string PalettePath = Folder + "MidgardPalette.asset";
        public const string OpenSignPath = Folder + "MidgardOpenSign.asset";
        public const string TreePath = Folder + "MidgardTree.asset";
        public const string DayPath = Folder + "MidgardDay.asset";
        public const string NightPath = Folder + "MidgardNight.asset";
        public static readonly string[] StallPaths = { Folder + "MidgardStallRed.asset", Folder + "MidgardStallGreen.asset", Folder + "MidgardStallGold.asset" };
        public const string FerrisPath = Folder + "MidgardFerrisWheel.asset";

        private const float StringLightRadius = 4.5f;
        private const float StringLightIntensity = 0.9f;

        /// <summary>Creates whatever is missing. <paramref name="lotBounds"/> is the inside of the fair night lot, where the string lights hang.</summary>
        public static void CreateIfMissing(Material glow, Rect lotBounds)
        {
            ArtAssets.Ensure(PalettePath, Palette);
            ArtAssets.Ensure(OpenSignPath, () => OpenSign(glow));
            ArtAssets.Ensure(TreePath, Tree);
            ArtAssets.Ensure(StallPaths[0], () => Stall("MidgardStallRed", PaletteSlot.Accent, PaletteSlot.Trim, PaletteSlot.Light));
            ArtAssets.Ensure(StallPaths[1], () => Stall("MidgardStallGreen", PaletteSlot.Roof, PaletteSlot.Trim, PaletteSlot.Accent));
            ArtAssets.Ensure(StallPaths[2], () => Stall("MidgardStallGold", PaletteSlot.Light, PaletteSlot.Sky, PaletteSlot.Accent));
            ArtAssets.Ensure(FerrisPath, () => FerrisWheel(glow));
            ArtAssets.Ensure(DayPath, () => LightingPreset.Create("MidgardDay", Hex("#fff4e0"), 1f, new PointLightSpec[0]));
            ArtAssets.Ensure(NightPath, () => Night(lotBounds));
        }

        private static WorldPalette Palette() => WorldPalette.Create("MidgardPalette", new[]
        {
            Hex("#2a2230"), Hex("#b8a47a"), Hex("#d8b98a"), Hex("#3f8a4a"), Hex("#ff3b4e"), Hex("#ffd86a"),
            Hex("#6e5a46"), Hex("#f2e6c9"), Hex("#3c7d3a"), Hex("#8a5a34"), Hex("#8c93a3"), Hex("#2f3e7a"),
        });

        // A dark board with a blinking neon OPEN in a red frame.
        private static PropRecipe OpenSign(Material glow)
        {
            var board = PropLayer.Create("Board", new[] { Rect(0, 4, 64, 24, PaletteSlot.Shadow) });
            var neon = PropLayer.Create("Neon", new[]
            {
                Rect(3, 7, 58, 1, PaletteSlot.Accent), Rect(3, 24, 58, 1, PaletteSlot.Accent),
                Rect(3, 7, 1, 18, PaletteSlot.Accent), Rect(60, 7, 1, 18, PaletteSlot.Accent),
                Text(20, 13, 23, 7, "OPEN", PaletteSlot.Accent),
            }, false, true, PropAnimation.Blink, 1.2f, default, 1);
            return PropRecipe.Create("MidgardOpenSign", new Vector2Int(2, 1), new[] { board, neon }, false, default, glow);
        }

        private static PropRecipe Tree()
        {
            var shapes = new[]
            {
                Rect(28, 0, 8, 22, PaletteSlot.Wood),
                Ellipse(6, 14, 52, 48, PaletteSlot.Plant),
                Ellipse(12, 24, 20, 16, PaletteSlot.Roof),
            };
            return PropRecipe.Create("MidgardTree", new Vector2Int(2, 2), new[] { PropLayer.Create("Tree", shapes) });
        }

        // A fair stall: posts, striped awning, counter with goods. Solid so cars keep out.
        private static PropRecipe Stall(string name, PaletteSlot stripeA, PaletteSlot stripeB, PaletteSlot goods)
        {
            var shapes = new List<PropShape>
            {
                Rect(4, 0, 88, 22, PaletteSlot.Wood), Rect(6, 22, 4, 34, PaletteSlot.Wood), Rect(86, 22, 4, 34, PaletteSlot.Wood),
            };
            for (var i = 0; i < 11; i++) shapes.Add(Rect(4 + i * 8, 36, 8, 24, i % 2 == 0 ? stripeA : stripeB));
            for (var i = 0; i < 5; i++) shapes.Add(Rect(14 + i * 14, 14, 8, 6, goods));
            return PropRecipe.Create(name, new Vector2Int(3, 2), new[] { PropLayer.Create("Stall", shapes.ToArray()) },
                true, new Vector2(2.7f, 1.7f));
        }

        // Stand, turning wheel with baskets and a ring of glowing bulbs. Solid.
        private static PropRecipe FerrisWheel(Material glow)
        {
            var hub = new Vector2Int(96, 104);
            var stand = new List<PropShape> { Rect(24, 0, 144, 6, PaletteSlot.Metal) };
            for (var k = 0; k < 3; k++)
            {
                stand.Add(LineUp(36 + k, 0, 61, 105, PaletteSlot.Metal));
                stand.Add(LineDown(96 - k, 0, 61, 105, PaletteSlot.Metal));
            }
            var wheel = new List<PropShape>();
            Wheel(wheel, hub, 80, PaletteSlot.Metal, PaletteSlot.Accent);
            var bulbs = new List<PropShape>();
            var colours = new[] { PaletteSlot.Accent, PaletteSlot.Light, PaletteSlot.Roof, PaletteSlot.Sky };
            for (var i = 0; i < 8; i++) Basket(wheel, OnCircle(hub, 80, i * 45f), colours[i % colours.Length]);
            for (var i = 0; i < 16; i++) bulbs.Add(Rect(OnCircle(hub, 74, i * 22.5f).x - 1, OnCircle(hub, 74, i * 22.5f).y - 1, 3, 3, PaletteSlot.Light));
            var layers = new[]
            {
                PropLayer.Create("Stand", stand.ToArray(), false),
                PropLayer.Create("Wheel", wheel.ToArray(), false, false, PropAnimation.Spin, 12f, hub, 1),
                PropLayer.Create("Bulbs", bulbs.ToArray(), false, true, PropAnimation.Spin, 12f, hub, 2),
            };
            return PropRecipe.Create("MidgardFerrisWheel", new Vector2Int(6, 6), layers, true, new Vector2(4.5f, 4.5f), glow);
        }

        // Blue dusk with warm flickering string lights along the top and bottom of the lot.
        private static LightingPreset Night(Rect bounds)
        {
            var lights = new List<PointLightSpec>();
            AddRow(lights, bounds, bounds.yMax - 0.8f, 9);
            AddRow(lights, bounds, bounds.yMin + 0.8f, 7);
            return LightingPreset.Create("MidgardNight", Hex("#7f8cd8"), 0.7f, lights.ToArray());
        }

        private static void AddRow(List<PointLightSpec> lights, Rect bounds, float y, int count)
        {
            for (var i = 0; i < count; i++)
            {
                var x = Mathf.Lerp(bounds.xMin + 1.5f, bounds.xMax - 1.5f, i / (float)(count - 1));
                lights.Add(new PointLightSpec(new Vector2(x, y), StringLightRadius, Hex("#ffc870"), StringLightIntensity, 0.2f, 1.5f + i * 0.2f));
            }
        }
    }
}
