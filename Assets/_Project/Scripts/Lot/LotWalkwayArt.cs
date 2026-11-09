using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>Paints zebra crossings and aisle arrows from plain rectangles. Purely visual.</summary>
    public static class LotWalkwayArt
    {
        private const int MarkingOrder = -8;

        /// <summary>Stripes running along the walking direction (up and down), spaced across <paramref name="zone"/>.</summary>
        public static void Crossing(Transform parent, RectFactory rects, string name, Rect zone, int bars,
            float barWidth, float barGap, Color color, int order)
        {
            var pitch = barWidth + barGap;
            var first = zone.center.x - pitch * (bars - 1) * 0.5f;
            var size = new Vector2(barWidth, Mathf.Max(0.1f, zone.height - barGap));
            for (var i = 0; i < bars; i++)
                rects.Make(parent, name + " Bar " + i, new Vector2(first + i * pitch, zone.center.y), size, color, order);
        }

        /// <summary>Builds the crossings and arrows of <paramref name="layout"/> under <paramref name="parent"/>.</summary>
        public static void Build(Transform parent, RectFactory rects, WalkwayLayout layout, WalkwayStyle style, Color crossingColor)
        {
            for (var i = 0; i < layout.Crossings.Length; i++)
                Crossing(parent, rects, "Aisle Crossing " + i, layout.Crossings[i], style.CrossingBars, style.BarWidth,
                    style.BarGap, crossingColor, MarkingOrder);
            for (var i = 0; i < layout.Arrows.Length; i++) Arrow(parent, rects, "Aisle Arrow " + i, layout.Arrows[i], style);
        }

        /// <summary>A shaft with two head strokes at the tip.</summary>
        public static void Arrow(Transform parent, RectFactory rects, string name, PaintedArrow arrow, WalkwayStyle style)
        {
            var t = style.ArrowThickness;
            rects.Segment(parent, name + " Shaft", arrow.Tail, arrow.Tip, t, style.ArrowColor, MarkingOrder);
            rects.Segment(parent, name + " Head A", arrow.Tip,
                arrow.HeadEnd(style.ArrowHeadLength, style.ArrowHeadDegrees), t, style.ArrowColor, MarkingOrder);
            rects.Segment(parent, name + " Head B", arrow.Tip,
                arrow.HeadEnd(style.ArrowHeadLength, -style.ArrowHeadDegrees), t, style.ArrowColor, MarkingOrder);
        }

        /// <summary>A "V" direction marking pointing down from <paramref name="apex"/>: two strokes meeting at the apex.</summary>
        public static void ChevronDown(Transform parent, RectFactory rects, string name, Vector2 apex, WalkwayStyle style)
        {
            var arrow = new PaintedArrow(apex + Vector2.up, apex);
            var t = style.ArrowThickness;
            rects.Segment(parent, name + " A", apex, arrow.HeadEnd(style.ArrowHeadLength, style.ArrowHeadDegrees), t, style.ArrowColor, MarkingOrder);
            rects.Segment(parent, name + " B", apex, arrow.HeadEnd(style.ArrowHeadLength, -style.ArrowHeadDegrees), t, style.ArrowColor, MarkingOrder);
        }
    }
}
