using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>Paints the walkway bands (a faint fill with solid edge lines) from plain rectangles. Purely visual.</summary>
    public static class LotWalkwayArt
    {
        private const int FillOrder = -9;
        private const int EdgeOrder = -8;

        /// <summary>Builds every band of <paramref name="layout"/> under <paramref name="parent"/>.</summary>
        public static void Build(Transform parent, RectFactory rects, WalkwayLayout layout, WalkwayStyle style, Color color)
        {
            for (var i = 0; i < layout.Bands.Length; i++) Band(parent, rects, "Walkway " + i, layout.Bands[i], style, color);
        }

        /// <summary>One band: the fill, and an edge line along each long side.</summary>
        public static void Band(Transform parent, RectFactory rects, string name, Rect area, WalkwayStyle style, Color color)
        {
            var fill = new Color(color.r, color.g, color.b, color.a * style.BandFillAlpha);
            rects.Make(parent, name + " Fill", area.center, area.size, fill, FillOrder);
            var t = style.BandEdge;
            if (area.width >= area.height) Edges(parent, rects, name, area, new Vector2(area.width, t), Vector2.up * (area.height - t) * 0.5f, color);
            else Edges(parent, rects, name, area, new Vector2(t, area.height), Vector2.right * (area.width - t) * 0.5f, color);
        }

        private static void Edges(Transform parent, RectFactory rects, string name, Rect area, Vector2 size, Vector2 offset, Color color)
        {
            rects.Make(parent, name + " Edge A", area.center + offset, size, color, EdgeOrder);
            rects.Make(parent, name + " Edge B", area.center - offset, size, color, EdgeOrder);
        }
    }
}
