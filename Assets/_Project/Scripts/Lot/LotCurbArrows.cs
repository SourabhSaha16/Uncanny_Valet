using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>
    /// Paints the facing arrows of the pickup zone: one on each side of the vertical text, pointing the way the facing
    /// rule requires. Outlined, above the curb paint and below the cars.
    /// </summary>
    public static class LotCurbArrows
    {
        private const int OutlineOrder = -5;
        private const int FillOrder = -4;

        /// <summary>Builds both arrows under <paramref name="parent"/> and returns the component that tints them.</summary>
        public static CurbArrow Build(RectFactory rects, Transform parent, LotConfig config)
        {
            var style = config.CurbArrow;
            var root = new GameObject("Curb Arrows");
            root.transform.SetParent(parent, false);
            var arrow = root.AddComponent<CurbArrow>();
            arrow.Init(style);
            var words = config.CurbLabelWords.Length;
            var letter = config.LabelCharWidth * config.CurbLabelSize;
            var half = CurbArrowLayout.TextHalfExtent(words, config.CurbLabelColumnSpacing, letter);
            var x = CurbArrowLayout.SideX(config.CurbSize.x, half, style.EdgeInset);
            Paint(rects, root.transform, arrow, style, config.CurbPosition + new Vector2(-x, 0f), "W");
            Paint(rects, root.transform, arrow, style, config.CurbPosition + new Vector2(x, 0f), "E");
            return arrow;
        }

        private static void Paint(RectFactory rects, Transform parent, CurbArrow owner, CurbArrowStyle s, Vector2 centre, string side)
        {
            var half = CurbFacingRule.RequiredDirection * (s.Length * 0.5f);
            var arrow = new PaintedArrow(centre - half, centre + half);
            Strokes(rects, parent, null, arrow, s, "Outline " + side, s.Outline, s.OutlineColor, OutlineOrder);
            Strokes(rects, parent, owner, arrow, s, "Fill " + side, 0f, s.ColorOf(CurbArrowState.Idle), FillOrder);
        }

        // Shaft and the two head strokes, each grown by <grow> on every side (the outline pass).
        private static void Strokes(RectFactory rects, Transform parent, CurbArrow owner, PaintedArrow a, CurbArrowStyle s,
            string tag, float grow, Color color, int order)
        {
            var thick = s.Thickness + grow * 2f;
            var head = s.Head + grow;
            Add(owner, rects.Segment(parent, tag + " Shaft", a.Tail, a.Tip, thick, color, order));
            Add(owner, rects.Segment(parent, tag + " Head A", a.Tip, a.HeadEnd(head, s.HeadDegrees), thick, color, order));
            Add(owner, rects.Segment(parent, tag + " Head B", a.Tip, a.HeadEnd(head, -s.HeadDegrees), thick, color, order));
        }

        private static void Add(CurbArrow owner, GameObject stroke)
        {
            if (owner != null) owner.AddFill(stroke.GetComponent<SpriteRenderer>());
        }
    }
}
