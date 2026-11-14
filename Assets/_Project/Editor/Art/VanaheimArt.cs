using System.Collections.Generic;
using UncannyValet.Core.Art;
using UnityEngine;
using static UncannyValet.EditorTools.Art.RecipeKit;

namespace UncannyValet.EditorTools.Art
{
    /// <summary>The Vanaheim art assets: golds, barn reds, earth browns and storm greys; barn, wheat tiles, hay bales, fences, the waterwheel trough, feast banners and stalls, sunny and stormy lighting.</summary>
    internal static class VanaheimArt
    {
        public const string Folder = ArtAssets.Root + "Vanaheim/";
        public const string PalettePath = Folder + "VanaheimPalette.asset";
        public const string BarnPath = Folder + "VanaheimBarn.asset";
        public const string WheatPath = Folder + "VanaheimWheat.asset";
        public const string HayPath = Folder + "VanaheimHayBale.asset";
        public const string FencePath = Folder + "VanaheimFence.asset";
        public const string TroughPath = Folder + "VanaheimTrough.asset";
        public const string BannerPath = Folder + "VanaheimBanner.asset";
        public const string StallPath = Folder + "VanaheimStall.asset";
        public const string SunnyPath = Folder + "VanaheimSunnyAfternoon.asset";
        public const string StormyPath = Folder + "VanaheimStormyEvening.asset";

        private const float LanternRadius = 4f;
        private const float LanternIntensity = 0.8f;

        /// <summary>Creates whatever is missing. <paramref name="lotBounds"/> is the inside of a crazy lot, where the stormy evening hangs its lanterns.</summary>
        public static void CreateIfMissing(Rect lotBounds)
        {
            ArtAssets.Ensure(PalettePath, Palette);
            ArtAssets.Ensure(BarnPath, Barn);
            ArtAssets.Ensure(WheatPath, Wheat);
            ArtAssets.Ensure(HayPath, HayBale);
            ArtAssets.Ensure(FencePath, Fence);
            ArtAssets.Ensure(TroughPath, Trough);
            ArtAssets.Ensure(BannerPath, Banner);
            ArtAssets.Ensure(StallPath, Stall);
            ArtAssets.Ensure(SunnyPath, () => LightingPreset.Create("VanaheimSunnyAfternoon", Hex("#ffe2a8"), 1.05f, new PointLightSpec[0]));
            ArtAssets.Ensure(StormyPath, () => Stormy(lotBounds));
        }

        private static WorldPalette Palette() => WorldPalette.Create("VanaheimPalette", new[]
        {
            Hex("#2b1d17"), Hex("#d6b04a"), Hex("#b23a2e"), Hex("#d8a63c"), Hex("#e8c54f"), Hex("#ffe9a8"),
            Hex("#5a4636"), Hex("#f0e0b8"), Hex("#7e8f3a"), Hex("#7a5232"), Hex("#6d7480"), Hex("#56627a"),
        });

        // Red walls, a gold gable roof, big double doors with cream braces and a hayloft window. Solid.
        private static PropRecipe Barn()
        {
            var shapes = new List<PropShape>
            {
                Rect(8, 0, 176, 84, PaletteSlot.Wall), Triangle(0, 70, 192, 58, PaletteSlot.Roof),
                Rect(72, 0, 48, 52, PaletteSlot.Wood), LineUp(72, 0, 48, 52, PaletteSlot.Trim), LineDown(72, 0, 48, 52, PaletteSlot.Trim),
                Rect(95, 0, 2, 52, PaletteSlot.Trim), Rect(86, 70, 20, 18, PaletteSlot.Shadow), Rect(95, 70, 2, 18, PaletteSlot.Trim),
            };
            return PropRecipe.Create("VanaheimBarn", new Vector2Int(6, 4), new[] { PropLayer.Create("Barn", shapes.ToArray()) },
                true, new Vector2(5.5f, 3.4f));
        }

        // A seamless gold tile of wheat stalks (no outline, so tiles join).
        private static PropRecipe Wheat()
        {
            var shapes = new List<PropShape> { Rect(0, 0, 32, 32, PaletteSlot.Ground) };
            for (var i = 0; i < 8; i++)
            {
                var x = 2 + i * 4;
                shapes.Add(Rect(x, 2 + (i % 2) * 3, 1, 18, PaletteSlot.Shadow));
                shapes.Add(Rect(x - 1, 18 + (i % 2) * 3, 3, 8, PaletteSlot.Accent));
                shapes.Add(Rect(x, 22 + (i % 2) * 3, 1, 4, PaletteSlot.Light));
            }
            return PropRecipe.Create("VanaheimWheat", Vector2Int.one, new[] { PropLayer.Create("Wheat", shapes.ToArray(), false) });
        }

        private static PropRecipe HayBale()
        {
            var shapes = new[]
            {
                Rect(2, 4, 28, 22, PaletteSlot.Accent), Rect(2, 20, 28, 6, PaletteSlot.Light),
                Rect(9, 4, 2, 22, PaletteSlot.Wood), Rect(21, 4, 2, 22, PaletteSlot.Wood),
            };
            return PropRecipe.Create("VanaheimHayBale", Vector2Int.one, new[] { PropLayer.Create("Hay", shapes) }, true, new Vector2(0.9f, 0.7f));
        }

        private static PropRecipe Fence()
        {
            var shapes = new[]
            {
                Rect(2, 8, 60, 3, PaletteSlot.Wood), Rect(2, 18, 60, 3, PaletteSlot.Wood),
                Rect(2, 2, 4, 26, PaletteSlot.Wood), Rect(30, 2, 4, 26, PaletteSlot.Wood), Rect(58, 2, 4, 26, PaletteSlot.Wood),
            };
            return PropRecipe.Create("VanaheimFence", new Vector2Int(2, 1), new[] { PropLayer.Create("Fence", shapes) }, true, new Vector2(2f, 0.5f));
        }

        // A wooden basin of water beside a slowly turning wooden waterwheel.
        private static PropRecipe Trough()
        {
            var basin = new[]
            {
                Rect(4, 0, 84, 40, PaletteSlot.Wood), Rect(10, 26, 72, 10, PaletteSlot.Sky), Rect(10, 40, 4, 12, PaletteSlot.Wood), Rect(78, 40, 4, 12, PaletteSlot.Wood),
            };
            var wheel = new List<PropShape>();
            var hub = new Vector2Int(92, 56);
            Wheel(wheel, hub, 30, PaletteSlot.Wood, PaletteSlot.Metal);
            for (var i = 0; i < 8; i++) wheel.Add(Rect(OnCircle(hub, 30, i * 45f).x - 3, OnCircle(hub, 30, i * 45f).y - 3, 6, 6, PaletteSlot.Wood));
            var layers = new[]
            {
                PropLayer.Create("Basin", basin),
                PropLayer.Create("Wheel", wheel.ToArray(), false, false, PropAnimation.Spin, 18f, hub, 1),
            };
            return PropRecipe.Create("VanaheimTrough", new Vector2Int(4, 3), layers);
        }

        private static PropRecipe Banner()
        {
            var shapes = new[]
            {
                Rect(14, 0, 3, 64, PaletteSlot.Wood), Rect(4, 18, 24, 40, PaletteSlot.Wall), Rect(6, 20, 20, 4, PaletteSlot.Accent),
                Ellipse(10, 30, 12, 12, PaletteSlot.Light), Rect(4, 54, 24, 6, PaletteSlot.Accent),
            };
            return PropRecipe.Create("VanaheimBanner", new Vector2Int(1, 2), new[] { PropLayer.Create("Banner", shapes) });
        }

        private static PropRecipe Stall()
        {
            var shapes = new List<PropShape>
            {
                Rect(4, 0, 88, 22, PaletteSlot.Wood), Rect(6, 22, 4, 34, PaletteSlot.Wood), Rect(86, 22, 4, 34, PaletteSlot.Wood),
            };
            for (var i = 0; i < 11; i++) shapes.Add(Rect(4 + i * 8, 36, 8, 24, i % 2 == 0 ? PaletteSlot.Wall : PaletteSlot.Trim));
            for (var i = 0; i < 5; i++) shapes.Add(Rect(14 + i * 14, 14, 8, 6, i % 2 == 0 ? PaletteSlot.Accent : PaletteSlot.Plant));
            return PropRecipe.Create("VanaheimStall", new Vector2Int(3, 2), new[] { PropLayer.Create("Stall", shapes.ToArray()) },
                true, new Vector2(2.7f, 1.7f));
        }

        // Storm-grey blue evening with a few swaying lanterns along the top of the lot.
        private static LightingPreset Stormy(Rect bounds)
        {
            var lights = new List<PointLightSpec>();
            for (var i = 0; i < 5; i++)
            {
                var x = Mathf.Lerp(bounds.xMin + 2f, bounds.xMax - 2f, i / 4f);
                lights.Add(new PointLightSpec(new Vector2(x, bounds.yMax - 1f), LanternRadius, Hex("#ffb55a"), LanternIntensity, 0.35f, 2f + i * 0.3f));
            }
            return LightingPreset.Create("VanaheimStormyEvening", Hex("#8d9bb8"), 0.62f, lights.ToArray());
        }
    }
}
