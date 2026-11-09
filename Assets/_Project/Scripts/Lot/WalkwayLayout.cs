using System.Collections.Generic;
using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>
    /// Pure placement of the zebra crossings inside the lot (across each drive aisle), the painted aisle arrows and the
    /// routes pedestrians walk: across the aisles, across the road at its crosswalk and along the sidewalks.
    /// Lot-local coordinates. Every route is an open polyline that pedestrians walk to the end of and back.
    /// </summary>
    public sealed class WalkwayLayout
    {
        /// <summary>Crossing zones: the aisle's height by the crossing width, one per aisle and crossing position.</summary>
        public Rect[] Crossings { get; }
        public PaintedArrow[] Arrows { get; }
        public Vector2[][] Routes { get; }

        public WalkwayLayout(LotConfig c, DecorLayout decor, WalkwayStyle style, float roadCrossOffset)
        {
            var crossings = new List<Rect>();
            var arrows = new List<PaintedArrow>();
            var routes = new List<Vector2[]>();
            AddAisles(c, style, crossings, arrows, routes);
            AddRoad(c, decor, style, roadCrossOffset, routes);
            routes.Add(new[]
            {
                new Vector2(decor.Lot.xMin + style.NorthMargin, decor.NorthSidewalk.center.y),
                new Vector2(decor.Lot.xMax - style.NorthMargin, decor.NorthSidewalk.center.y)
            });
            Crossings = crossings.ToArray();
            Arrows = arrows.ToArray();
            Routes = routes.ToArray();
        }

        /// <summary>X of the open strip between the curb and the first bay column, where people cross the aisles.</summary>
        public static float StripX(LotConfig c)
            => c.CurbPosition.x + c.CurbSize.x * 0.5f + c.CurbToSpotsGap * 0.5f;

        private static void AddAisles(LotConfig c, WalkwayStyle s, List<Rect> crossings, List<PaintedArrow> arrows,
            List<Vector2[]> routes)
        {
            var spotsLeft = c.CurbPosition.x + c.CurbSize.x * 0.5f + c.CurbToSpotsGap;
            var stripX = StripX(c);
            var bayX = spotsLeft + (c.SpotSize.x + c.SpotGap) - c.SpotGap * 0.5f; // between the first two bay columns
            var arrowX = (stripX + bayX) * 0.5f; // in the gap between the two crossings
            var top = (c.Rows * c.SpotSize.y + (c.Rows - 1) * c.AisleWidth) * 0.5f;
            for (var r = 0; r < c.Rows - 1; r++)
            {
                var yTop = top - (r + 1) * c.SpotSize.y - r * c.AisleWidth;
                var yBottom = yTop - c.AisleWidth;
                foreach (var x in new[] { stripX, bayX })
                {
                    crossings.Add(new Rect(x - s.CrossingWidth * 0.5f, yBottom, s.CrossingWidth, c.AisleWidth));
                    routes.Add(new[] { new Vector2(x, yTop + s.BayEntry), new Vector2(x, yBottom - s.BayEntry) });
                }
                var mid = (yTop + yBottom) * 0.5f;
                var half = s.ArrowLength * 0.5f;
                arrows.Add(new PaintedArrow(new Vector2(arrowX - half, mid + s.LaneInset), new Vector2(arrowX + half, mid + s.LaneInset)));
                arrows.Add(new PaintedArrow(new Vector2(arrowX + half, mid - s.LaneInset), new Vector2(arrowX - half, mid - s.LaneInset)));
            }
        }

        private static void AddRoad(LotConfig c, DecorLayout d, WalkwayStyle s, float crossOffset, List<Vector2[]> routes)
        {
            var x = c.CurbPosition.x + crossOffset;
            var near = d.SouthSidewalk.center.y;
            var far = d.FarSidewalk.center.y;
            routes.Add(new[] { new Vector2(x - s.RoadReach, near), new Vector2(x, near), new Vector2(x, far), new Vector2(x + s.RoadReach, far) });
            routes.Add(new[] { new Vector2(x + s.RoadReach, near), new Vector2(x, near), new Vector2(x, far), new Vector2(x - s.RoadReach, far) });
        }
    }
}
