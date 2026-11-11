using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>
    /// Pure placement of the scenery around the lot (lot v3): the dark asphalt ground everywhere outside the lot, the
    /// one-way entry road below the south wall (with a sidewalk below it), the one-way exit road above the top wall (with a
    /// sidewalk above it), and the bus stop on the right strip. Lot-local coordinates.
    /// </summary>
    public readonly struct DecorLayout
    {
        public readonly Rect Lot;
        /// <summary>The ground outside the lot walls, behind the roads and everything else.</summary>
        public readonly Rect Outside;
        /// <summary>The entry road below the lot: one lane, left to right.</summary>
        public readonly Rect EastLane;
        /// <summary>The exit road above the lot: one lane, left to right.</summary>
        public readonly Rect TopLane;
        /// <summary>The sidewalk below the entry road.</summary>
        public readonly Rect FarSidewalk;
        /// <summary>The sidewalk above the exit road.</summary>
        public readonly Rect TopSidewalk;
        /// <summary>The bus stop shelter on the right strip.</summary>
        public readonly Rect BusStop;

        public DecorLayout(LotGeometry g, float wall, float sidewalk, float outsideReach, Vector2 busStopSize, float busStopFromBottom)
        {
            Lot = g.Bounds;
            var x0 = g.RoadSpan.x;
            var w = g.RoadSpan.y - g.RoadSpan.x;
            EastLane = new Rect(x0, g.EastLaneY - g.LaneWidth * 0.5f, w, g.LaneWidth);
            TopLane = new Rect(x0, g.TopLaneY - g.LaneWidth * 0.5f, w, g.LaneWidth);
            FarSidewalk = new Rect(x0, EastLane.yMin - sidewalk, w, sidewalk);
            TopSidewalk = new Rect(x0, TopLane.yMax, w, sidewalk);
            Outside = Rect.MinMaxRect(x0 - outsideReach, FarSidewalk.yMin - outsideReach, g.RoadSpan.y + outsideReach, TopSidewalk.yMax + outsideReach);
            var shelter = new Vector2(g.Strip.center.x, g.Strip.yMin + busStopFromBottom + busStopSize.y * 0.5f);
            BusStop = LotGeometry.Box(shelter, busStopSize);
        }
    }
}
