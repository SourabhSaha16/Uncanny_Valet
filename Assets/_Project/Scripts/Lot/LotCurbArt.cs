using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>Dresses the curb zone as a cobbled sidewalk with a yellow painted edge and a plate behind the CURB text.</summary>
    public static class LotCurbArt
    {
        private const int TileOrder = -7;
        private const int PaintOrder = -6;

        public static void Build(RectFactory rects, Transform parent, LotConfig config, LotArtConfig art)
        {
            var area = LotGeometry.Box(config.CurbPosition, config.CurbSize);
            SpriteTiler.Fill(parent, "Curb Tiles", art.SidewalkTile, area, TileOrder);
            var plate = Vector2.Scale(config.CurbSize, art.CurbPlateShare);
            rects.Make(parent, "Curb Plate", config.CurbPosition, plate, art.CurbPlateColor, PaintOrder);
            Edge(rects, parent, area, art);
            Direction(rects, parent, area, art.Walkway);
        }

        // Two big outlined down arrows with a chevron under each tip beside the CURB text: the nose must point this way.
        private static void Direction(RectFactory rects, Transform parent, Rect area, WalkwayStyle style)
        {
            for (var side = -1; side <= 1; side += 2)
            {
                var x = area.center.x + side * area.width * style.CurbArrowSideShare;
                LotArrowMarks.Build(parent, rects, "Curb Arrow " + side, new Vector2(x, area.center.y), Vector2.down,
                    style.CurbArrowLength, style);
            }
        }

        private static void Edge(RectFactory rects, Transform parent, Rect area, LotArtConfig art)
        {
            var t = art.CurbEdgeWidth;
            var color = art.CurbEdgeColor;
            rects.Make(parent, "Curb Edge N", new Vector2(area.center.x, area.yMax - t * 0.5f), new Vector2(area.width, t), color, PaintOrder);
            rects.Make(parent, "Curb Edge S", new Vector2(area.center.x, area.yMin + t * 0.5f), new Vector2(area.width, t), color, PaintOrder);
            rects.Make(parent, "Curb Edge W", new Vector2(area.xMin + t * 0.5f, area.center.y), new Vector2(t, area.height), color, PaintOrder);
            rects.Make(parent, "Curb Edge E", new Vector2(area.xMax - t * 0.5f, area.center.y), new Vector2(t, area.height), color, PaintOrder);
        }
    }
}
