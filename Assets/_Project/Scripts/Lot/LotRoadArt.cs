using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>
    /// Paints the two two-lane roads of <see cref="DecorLayout"/>: asphalt, white edge lines, a dashed centre line between the lanes, a sidewalk on the far side,
    /// and a pixel-style word on each lane (ENTRY below the lot, EXIT above it). Purely visual.
    /// </summary>
    public static class LotRoadArt
    {
        private const int GroundOrder = -25;
        private const int MarkingOrder = -24;
        private const float EdgeLineWidth = 0.12f;

        /// <summary>Builds both roads under <paramref name="parent"/>; <paramref name="lot"/> places the labels along the lot width.</summary>
        public static void Build(Transform parent, RectFactory rects, DecorLayout l, Rect lot, LotArtConfig art)
        {
            Road(parent, rects, "Entry", l.EastLane, l.EastBackLane, art);
            Road(parent, rects, "Exit", l.TopLane, l.TopBackLane, art);
            SpriteTiler.Fill(parent, "Entry Sidewalk", art.SidewalkTile, l.FarSidewalk, GroundOrder);
            SpriteTiler.Fill(parent, "Exit Sidewalk", art.SidewalkTile, l.TopSidewalk, GroundOrder);
            var x = lot.xMin + lot.width * art.RoadLabelShare;
            Label(parent, rects, art, art.EntryWord, new Vector2(x, l.EastLane.center.y));
            Label(parent, rects, art, art.ExitWord, new Vector2(x, l.TopLane.center.y));
        }

        private static void Road(Transform parent, RectFactory rects, string name, Rect lane, Rect backLane, LotArtConfig art)
        {
            var road = Rect.MinMaxRect(lane.xMin, Mathf.Min(lane.yMin, backLane.yMin), lane.xMax, Mathf.Max(lane.yMax, backLane.yMax));
            rects.Make(parent, name + " Road", road.center, road.size, art.AsphaltColor, GroundOrder);
            var line = new Vector2(road.width, EdgeLineWidth);
            rects.Make(parent, name + " Road Edge N", new Vector2(road.center.x, road.yMax - EdgeLineWidth * 0.5f), line, art.CrosswalkColor, MarkingOrder);
            rects.Make(parent, name + " Road Edge S", new Vector2(road.center.x, road.yMin + EdgeLineWidth * 0.5f), line, art.CrosswalkColor, MarkingOrder);
            CentreDashes(parent, rects, name, road, lane, backLane, art);
        }

        // The dashed line between the two lanes: the shared edge of the lane rects.
        private static void CentreDashes(Transform parent, RectFactory rects, string name, Rect road, Rect lane, Rect backLane, LotArtConfig art)
        {
            var y = lane.yMin > backLane.yMin ? lane.yMin : lane.yMax;
            var holder = new GameObject(name + " Road Centre Dashes").transform;
            holder.SetParent(parent, false);
            var size = new Vector2(art.LaneDashLength, art.LaneDashWidth);
            var step = art.LaneDashLength + art.LaneDashGap;
            for (var x = road.xMin + art.LaneDashLength * 0.5f; x < road.xMax; x += step)
                rects.Make(holder, "Dash", new Vector2(x, y), size, art.CrosswalkColor, MarkingOrder);
        }

        private static void Label(Transform parent, RectFactory rects, LotArtConfig art, string word, Vector2 centre)
        {
            var color = art.RoadLabelColor;
            var root = new GameObject(word + " Road Label").transform;
            root.SetParent(parent, false);
            PixelDecal.Build(rects, root, centre, PixelFont.Compose(word), (char key, out Color32 c) =>
            {
                c = color;
                return key == PixelFont.Ink;
            }, art.RoadLabelPixel);
        }
    }
}
