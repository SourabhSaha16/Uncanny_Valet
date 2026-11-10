using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>
    /// Pure placement of the scenery around the lot (lot v3): the dark asphalt ground everywhere outside the lot, the
    /// two-lane road below the south wall (eastbound lane next to the lot, westbound lane below it) with a far sidewalk,
    /// and the bus stop on the right strip. Lot-local coordinates.
    /// </summary>
    public readonly struct DecorLayout
    {
        public readonly Rect Lot;
        /// <summary>The ground outside the lot walls, behind the road and everything else.</summary>
        public readonly Rect Outside;
        public readonly Rect EastLane;
        public readonly Rect WestLane;
        public readonly Rect FarSidewalk;
        /// <summary>The bus stop shelter on the right strip.</summary>
        public readonly Rect BusStop;

        public DecorLayout(LotGeometry g, float wall, float sidewalk, float outsideReach, Vector2 busStopSize, float busStopFromBottom)
        {
            Lot = g.Bounds;
            var x0 = g.RoadSpan.x;
            var w = g.RoadSpan.y - g.RoadSpan.x;
            EastLane = new Rect(x0, g.EastLaneY - g.LaneWidth * 0.5f, w, g.LaneWidth);
            WestLane = new Rect(x0, g.WestLaneY - g.LaneWidth * 0.5f, w, g.LaneWidth);
            FarSidewalk = new Rect(x0, WestLane.yMin - sidewalk, w, sidewalk);
            Outside = Rect.MinMaxRect(x0 - outsideReach, FarSidewalk.yMin - outsideReach, g.RoadSpan.y + outsideReach, Lot.yMax + wall + outsideReach);
            var shelter = new Vector2(g.Strip.center.x, g.Strip.yMin + busStopFromBottom + busStopSize.y * 0.5f);
            BusStop = LotGeometry.Box(shelter, busStopSize);
        }

        /// <summary>Both road lanes together.</summary>
        public Rect Road => Rect.MinMaxRect(EastLane.xMin, WestLane.yMin, EastLane.xMax, EastLane.yMax);
    }
}
