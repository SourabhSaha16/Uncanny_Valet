using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>
    /// Paints the two one-way roads of <see cref="DecorLayout"/>: asphalt, white edge lines, a sidewalk on the far side,
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
            Road(parent, rects, "Entry", l.EastLane, art);
            Road(parent, rects, "Exit", l.TopLane, art);
            SpriteTiler.Fill(parent, "Entry Sidewalk", art.SidewalkTile, l.FarSidewalk, GroundOrder);
            SpriteTiler.Fill(parent, "Exit Sidewalk", art.SidewalkTile, l.TopSidewalk, GroundOrder);
            var x = lot.xMin + lot.width * art.RoadLabelShare;
            Label(parent, rects, art, art.EntryWord, new Vector2(x, l.EastLane.center.y));
            Label(parent, rects, art, art.ExitWord, new Vector2(x, l.TopLane.center.y));
        }

        private static void Road(Transform parent, RectFactory rects, string name, Rect lane, LotArtConfig art)
        {
            rects.Make(parent, name + " Road", lane.center, lane.size, art.AsphaltColor, GroundOrder);
            var line = new Vector2(lane.width, EdgeLineWidth);
            rects.Make(parent, name + " Road Edge N", new Vector2(lane.center.x, lane.yMax - EdgeLineWidth * 0.5f), line, art.CrosswalkColor, MarkingOrder);
            rects.Make(parent, name + " Road Edge S", new Vector2(lane.center.x, lane.yMin + EdgeLineWidth * 0.5f), line, art.CrosswalkColor, MarkingOrder);
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
