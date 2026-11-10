using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>
    /// Big painted direction marks with a dark outline: down arrows on the curb zone (point the nose at the road) and
    /// up arrows at the arrival slots (cars drive in from the road). Drawn above the cobbles and the CURB plate.
    /// </summary>
    public static class LotArrowMarks
    {
        private const int OutlineOrder = -5;
        private const int FillOrder = -4;

        /// <summary>
        /// An arrow of the given length plus a chevron beyond its tip, the whole mark centred on <paramref name="center"/>.
        /// </summary>
        /// <param name="direction">Unit direction the arrow points (down: (0,-1); up: (0,1)).</param>
        public static void Build(Transform parent, RectFactory rects, string name, Vector2 center, Vector2 direction,
            float length, WalkwayStyle style)
        {
            var reach = style.CurbChevronGap + style.MarkHeadLength; // tip to chevron apex
            var middle = center - direction * (reach * 0.5f);
            var half = direction * (length * 0.5f);
            var arrow = new PaintedArrow(middle - half, middle + half);
            var apex = arrow.Tip + direction * reach;
            Paint(parent, rects, name, arrow, apex, style, style.MarkOutline, style.MarkOutlineColor, OutlineOrder);
            Paint(parent, rects, name, arrow, apex, style, 0f, style.MarkColor, FillOrder);
        }

        /// <summary>An up arrow (along the cars' heading) in every arrival slot, where cars drive in from the road.</summary>
        public static void Arrivals(Transform parent, RectFactory rects, Vector2[] slots, float rotationDegrees, WalkwayStyle style)
        {
            Vector2 heading = Quaternion.Euler(0f, 0f, rotationDegrees) * Vector2.up;
            for (var i = 0; i < slots.Length; i++)
                Build(parent, rects, "Arrival Arrow " + i, slots[i], heading, style.ArrivalArrowLength, style);
        }

        // Shaft, head strokes and the chevron at the apex, each grown by <grow> on every side (the outline pass).
        private static void Paint(Transform parent, RectFactory rects, string name, PaintedArrow arrow, Vector2 apex,
            WalkwayStyle s, float grow, Color color, int order)
        {
            var thick = s.MarkThickness + grow * 2f;
            var head = s.MarkHeadLength + grow;
            var tag = name + (grow > 0f ? " Outline" : string.Empty);
            rects.Segment(parent, tag + " Shaft", arrow.Tail, arrow.Tip, thick, color, order);
            rects.Segment(parent, tag + " Head A", arrow.Tip, arrow.HeadEnd(head, s.ArrowHeadDegrees), thick, color, order);
            rects.Segment(parent, tag + " Head B", arrow.Tip, arrow.HeadEnd(head, -s.ArrowHeadDegrees), thick, color, order);
            var chevron = new PaintedArrow(apex - (arrow.Tip - arrow.Tail).normalized, apex);
            rects.Segment(parent, tag + " Chevron A", apex, chevron.HeadEnd(head, s.ArrowHeadDegrees), thick, color, order);
            rects.Segment(parent, tag + " Chevron B", apex, chevron.HeadEnd(head, -s.ArrowHeadDegrees), thick, color, order);
        }
    }
}
