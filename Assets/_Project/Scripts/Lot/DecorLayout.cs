using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>
    /// Pure placement of the scenery around the lot (lot v2): grass everywhere, the two-lane road below the south wall
    /// (eastbound lane next to the lot, westbound lane below it) with a far sidewalk, the bus stop on the right strip and
    /// the trees and tufts on the grass. Lot-local coordinates.
    /// </summary>
    public readonly struct DecorLayout
    {
        private const float TreeSideDistance = 3f;
        private const int TreesAlongTop = 5;
        private const int TreesAlongSide = 3;
        private const int TuftBands = 4;
        private const float TuftBandDepth = 7f;

        public readonly Rect Lot;
        public readonly Rect Grass;
        public readonly Rect EastLane;
        public readonly Rect WestLane;
        public readonly Rect FarSidewalk;
        /// <summary>The bus stop shelter on the right strip.</summary>
        public readonly Rect BusStop;
        private readonly float _wall;

        public DecorLayout(LotGeometry g, float wall, float sidewalk, float grassReach, Vector2 busStopSize, float busStopFromBottom)
        {
            _wall = wall;
            Lot = g.Bounds;
            var x0 = g.RoadSpan.x;
            var w = g.RoadSpan.y - g.RoadSpan.x;
            EastLane = new Rect(x0, g.EastLaneY - g.LaneWidth * 0.5f, w, g.LaneWidth);
            WestLane = new Rect(x0, g.WestLaneY - g.LaneWidth * 0.5f, w, g.LaneWidth);
            FarSidewalk = new Rect(x0, WestLane.yMin - sidewalk, w, sidewalk);
            Grass = Rect.MinMaxRect(x0 - grassReach, FarSidewalk.yMin - grassReach, g.RoadSpan.y + grassReach, Lot.yMax + wall + grassReach);
            var shelter = new Vector2(g.Strip.center.x, g.Strip.yMin + busStopFromBottom + busStopSize.y * 0.5f);
            BusStop = LotGeometry.Box(shelter, busStopSize);
        }

        /// <summary>Both road lanes together.</summary>
        public Rect Road => Rect.MinMaxRect(EastLane.xMin, WestLane.yMin, EastLane.xMax, EastLane.yMax);

        /// <summary>Bottom centres of the trees: along the top wall and along both sides of the lot.</summary>
        public Vector2[] TreeBases()
        {
            var bases = new Vector2[TreesAlongTop + TreesAlongSide * 2];
            var top = Lot.yMax + _wall;
            for (var i = 0; i < TreesAlongTop; i++)
                bases[i] = new Vector2(Lot.xMin + Lot.width * (i + 0.5f) / TreesAlongTop, top + TreeSideDistance * 0.5f);
            for (var i = 0; i < TreesAlongSide; i++)
            {
                var y = Lot.yMin + Lot.height * (i + 1f) / (TreesAlongSide + 1f);
                bases[TreesAlongTop + i * 2] = new Vector2(Lot.xMin - _wall - TreeSideDistance, y);
                bases[TreesAlongTop + i * 2 + 1] = new Vector2(Lot.xMax + _wall + TreeSideDistance, y);
            }
            return bases;
        }

        /// <summary>Deterministic grass-tuft positions on the grass above, beside and below the lot, never inside the lot or on the road.</summary>
        public Vector2[] TuftSpots(int count)
        {
            var spots = new Vector2[Mathf.Max(0, count)];
            uint state = 12345u;
            for (var i = 0; i < spots.Length; i++)
            {
                var band = Band(i % TuftBands);
                state = unchecked(state * 1664525u + 1013904223u);
                var fx = ((state >> 8) & 0xFFFF) / 65535f;
                state = unchecked(state * 1664525u + 1013904223u);
                var fy = ((state >> 8) & 0xFFFF) / 65535f;
                spots[i] = new Vector2(band.xMin + band.width * fx, band.yMin + band.height * fy);
            }
            return spots;
        }

        private Rect Band(int index)
        {
            var x0 = EastLane.xMin;
            var x1 = EastLane.xMax;
            var top = Lot.yMax + _wall;
            switch (index)
            {
                case 0: return new Rect(x0, top, x1 - x0, TuftBandDepth);
                case 1: return new Rect(Lot.xMin - _wall - TuftBandDepth, Lot.yMin, TuftBandDepth, Lot.height + _wall);
                case 2: return new Rect(Lot.xMax + _wall, Lot.yMin, TuftBandDepth, Lot.height + _wall);
                default: return new Rect(x0, FarSidewalk.yMin - TuftBandDepth, x1 - x0, TuftBandDepth);
            }
        }
    }
}
