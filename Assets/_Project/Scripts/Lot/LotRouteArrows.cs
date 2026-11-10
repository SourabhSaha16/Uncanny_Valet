using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>
    /// Paints small outlined arrows along the route a delivered car takes when it leaves (curb, down the corridor, the
    /// turn east onto the road), so the valet knows which way to keep clear. Purely visual.
    /// </summary>
    public static class LotRouteArrows
    {
        private const int OutlineOrder = -5;
        private const int FillOrder = -4;

        /// <summary>Builds the arrows of <see cref="IRoadLayout.CurbExitRoute"/> under <paramref name="parent"/> (the lot, at <paramref name="origin"/> in world space).</summary>
        public static void Build(Transform parent, RectFactory rects, IRoadLayout road, Vector2 origin, WalkwayStyle style)
        {
            var route = new List<Vector2>();
            road.CurbExitRoute(route);
            for (var i = 0; i < route.Count; i++) route[i] -= origin;
            var marks = RouteMarks.Along(route, style.RouteArrowStart, style.RouteArrowSpacing);
            for (var i = 0; i < marks.Count; i++) Arrow(parent, rects, i, marks[i], style);
        }

        private static void Arrow(Transform parent, RectFactory rects, int index, RouteMark mark, WalkwayStyle s)
        {
            var half = mark.Direction * (s.RouteArrowLength * 0.5f);
            var arrow = new PaintedArrow(mark.Position - half, mark.Position + half);
            var name = "Exit Arrow " + index;
            Paint(parent, rects, name, arrow, s, s.MarkOutline, s.MarkOutlineColor, OutlineOrder);
            Paint(parent, rects, name, arrow, s, 0f, s.MarkColor, FillOrder);
        }

        // Shaft and the two head strokes, each grown by <grow> on every side (the outline pass).
        private static void Paint(Transform parent, RectFactory rects, string name, PaintedArrow arrow, WalkwayStyle s,
            float grow, Color color, int order)
        {
            var thick = s.RouteArrowThickness + grow * 2f;
            var head = s.RouteArrowHead + grow;
            var tag = name + (grow > 0f ? " Outline" : string.Empty);
            rects.Segment(parent, tag + " Shaft", arrow.Tail, arrow.Tip, thick, color, order);
            rects.Segment(parent, tag + " Head A", arrow.Tip, arrow.HeadEnd(head, s.ArrowHeadDegrees), thick, color, order);
            rects.Segment(parent, tag + " Head B", arrow.Tip, arrow.HeadEnd(head, -s.ArrowHeadDegrees), thick, color, order);
        }
    }
}
