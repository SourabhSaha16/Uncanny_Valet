using System.Collections.Generic;
using UncannyValet.Core.Art;
using UncannyValet.EditorTools.Art;
using UncannyValet.Lot;
using UnityEditor;
using UnityEngine;

namespace UncannyValet.EditorTools
{
    /// <summary>
    /// Where the props of the two Vanaheim lots stand (task 107), computed from the lot's geometry: the barn on the building, a wheat field,
    /// fences and hay bales above the bays of Freyr's Harvest Barn; the barn, feast stalls and banners above the bays of the storm feast; and
    /// a waterwheel trough on every wash trough. All of it stays off the bays, the aisle crossing, the queue and the curb.
    /// </summary>
    internal static class VanaheimProps
    {
        private const float BarnAboveCurb = 3f; // the barn's centre above the curb's top edge
        private const float WheatRowsStart = 2.5f; // above the top edge of the top bay row
        private const int WheatRows = 4;
        private const float WheatFromCrossing = 1.5f;
        private const float WheatToStrip = 1f;
        private const float FenceY = 1.4f;
        private const float FenceSpacing = 2f;
        private const int FenceCount = 6;
        private const float HayY = 1f;
        private const float StallY = 4.7f;
        internal const float StallSpacing = 4.5f;
        private const int StallCount = 6;
        internal const float BannerY = 2.1f;
        internal const float BannerOffset = 2.25f;
        internal const float StallFromCrossing = 3.5f;
        internal const int BannerCount = 5;
        private const int BarnOrder = 6;

        public static List<PropPlacement> Training(LotGeometry g)
        {
            var list = new List<PropPlacement> { Barn(g) };
            var wheat = Load(VanaheimArt.WheatPath);
            var x0 = g.CrossingX + WheatFromCrossing;
            var columns = Mathf.FloorToInt(g.Strip.xMin - WheatToStrip - x0) + 1;
            var top = g.Curb.yMax;
            for (var row = 0; row < WheatRows; row++)
                for (var column = 0; column < columns; column++)
                    list.Add(new PropPlacement(wheat, new Vector2(x0 + column, top + WheatRowsStart + row), 1));
            var fence = Load(VanaheimArt.FencePath);
            for (var i = 0; i < FenceCount; i++) list.Add(new PropPlacement(fence, new Vector2(x0 + 1f + i * FenceSpacing, top + FenceY)));
            var hay = Load(VanaheimArt.HayPath);
            var trough = g.Troughs[0];
            list.Add(new PropPlacement(hay, new Vector2(trough.center.x - 0.5f, top + HayY)));
            list.Add(new PropPlacement(hay, new Vector2(trough.center.x + 0.6f, top + HayY)));
            AddTroughs(list, g);
            return list;
        }

        public static List<PropPlacement> Crazy(LotGeometry g)
        {
            var list = new List<PropPlacement> { Barn(g) };
            var stall = Load(VanaheimArt.StallPath);
            var banner = Load(VanaheimArt.BannerPath);
            var x0 = g.CrossingX + StallFromCrossing;
            var top = g.Curb.yMax;
            for (var i = 0; i < StallCount; i++) list.Add(new PropPlacement(stall, new Vector2(x0 + i * StallSpacing, top + StallY)));
            for (var i = 0; i < BannerCount; i++) list.Add(new PropPlacement(banner, new Vector2(x0 + BannerOffset + i * StallSpacing, top + BannerY)));
            AddTroughs(list, g);
            return list;
        }

        // The barn stands on the re-skinned building, facing the lot.
        private static PropPlacement Barn(LotGeometry g)
            => new PropPlacement(Load(VanaheimArt.BarnPath), new Vector2(g.Diner.center.x, g.Curb.yMax + BarnAboveCurb), BarnOrder);

        private static void AddTroughs(List<PropPlacement> list, LotGeometry g)
        {
            var trough = Load(VanaheimArt.TroughPath);
            foreach (var rect in g.Troughs) list.Add(new PropPlacement(trough, rect.center));
        }

        private static PropRecipe Load(string path) => AssetDatabase.LoadAssetAtPath<PropRecipe>(path);
    }
}
