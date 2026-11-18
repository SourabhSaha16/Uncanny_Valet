using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>Dresses the curb zone as a cobbled sidewalk with a yellow painted edge and a plate behind the curb text.</summary>
    public static class LotCurbArt
    {
        private const int TileOrder = -7;
        private const int PaintOrder = -6;
        private const int LabelOrder = -5;

        public static void Build(RectFactory rects, Transform parent, LotConfig config, LotArtConfig art)
        {
            var area = LotGeometry.Box(config.CurbPosition, config.CurbSize);
            SpriteTiler.Fill(parent, "Curb Tiles", art.SidewalkTile, area, TileOrder);
            var plate = Vector2.Scale(config.CurbSize, art.CurbPlateShare);
            rects.Make(parent, "Curb Plate", config.CurbPosition, plate, art.CurbPlateColor, PaintOrder);
            Edge(rects, parent, area, art);
        }

        /// <summary>The curb zone's text: each configured word written vertically in its own column (VALET PICKUP).</summary>
        public static void BuildLabel(Transform parent, LotConfig config)
        {
            var words = config.CurbLabelWords;
            for (var i = 0; i < words.Length; i++)
            {
                var x = StackedText.ColumnOffset(i, words.Length, config.CurbLabelColumnSpacing);
                var at = config.CurbPosition + new Vector2(x, 0f);
                SpotLabel.Create(parent, StackedText.Stack(words[i]), at, config, LabelOrder, config.CurbLabelSize);
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
