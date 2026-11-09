using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>
    /// Pure placement of the scenery around the largest lot frame: grass everywhere, a sidewalk, a road and a
    /// far sidewalk to the south (cars arrive from there), a sidewalk and the diner to the north. Lot-local coordinates.
    /// </summary>
    public readonly struct DecorLayout
    {
        private const float TreeSideDistance = 3f;
        private const float TreeNorthSpread = 10f;
        private const int TuftBands = 5;
        private const float TuftBandDepth = 7f;
        private const float BuildingClearance = 3f;

        public readonly Rect Lot;
        public readonly Rect Grass;
        public readonly Rect SouthSidewalk;
        public readonly Rect Road;
        public readonly Rect FarSidewalk;
        public readonly Rect NorthSidewalk;
        /// <summary>Bottom centre of the diner, standing on the north sidewalk.</summary>
        public readonly Vector2 BuildingBase;

        public DecorLayout(Rect lot, float wall, float sidewalk, float roadHeight, float sideReach, float grassReach)
        {
            Lot = lot;
            Grass = Rect.MinMaxRect(lot.xMin - grassReach, lot.yMin - grassReach, lot.xMax + grassReach, lot.yMax + grassReach);
            var x0 = lot.xMin - sideReach;
            var w = lot.width + sideReach * 2f;
            SouthSidewalk = new Rect(x0, lot.yMin - wall - sidewalk, w, sidewalk);
            Road = new Rect(x0, SouthSidewalk.yMin - roadHeight, w, roadHeight);
            FarSidewalk = new Rect(x0, Road.yMin - sidewalk, w, sidewalk);
            NorthSidewalk = new Rect(x0, lot.yMax + wall, w, sidewalk);
            BuildingBase = new Vector2(lot.center.x, NorthSidewalk.yMax);
        }

        /// <summary>Bottom centres of the trees: along both sides of the lot and either side of the diner.</summary>
        public Vector2[] TreeBases()
        {
            var side = new[] { 0.2f, 0.5f, 0.8f };
            var bases = new Vector2[side.Length * 2 + 2];
            for (var i = 0; i < side.Length; i++)
            {
                var y = Lot.yMin + Lot.height * side[i];
                bases[i * 2] = new Vector2(Lot.xMin - TreeSideDistance, y);
                bases[i * 2 + 1] = new Vector2(Lot.xMax + TreeSideDistance, y);
            }
            bases[bases.Length - 2] = new Vector2(BuildingBase.x - TreeNorthSpread, BuildingBase.y);
            bases[bases.Length - 1] = new Vector2(BuildingBase.x + TreeNorthSpread, BuildingBase.y);
            return bases;
        }

        /// <summary>Deterministic grass-tuft positions on the grass beside and above the lot, never inside the lot.</summary>
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
            var x0 = SouthSidewalk.xMin;
            var x1 = SouthSidewalk.xMax;
            var top = NorthSidewalk.yMax;
            switch (index)
            {
                case 0: return new Rect(x0, Lot.yMin, TuftBandDepth, Lot.height); // west, between the sidewalks
                case 1: return new Rect(x1 - TuftBandDepth, Lot.yMin, TuftBandDepth, Lot.height);
                case 2: return new Rect(x0 - TuftBandDepth, FarSidewalk.yMin, TuftBandDepth, top - FarSidewalk.yMin);
                case 3: return new Rect(x1, FarSidewalk.yMin, TuftBandDepth, top - FarSidewalk.yMin);
                default: return new Rect(BuildingBase.x + BuildingClearance, top, x1 - BuildingBase.x - BuildingClearance, TuftBandDepth);
            }
        }
    }
}
