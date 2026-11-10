using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>Pure rule: the frame of the lot for whatever is open right now.</summary>
    public static class LotFrameRule
    {
        /// <summary>Frame around the curb, the queue and the grid columns holding an open regular spot (the stations come first in the grid).</summary>
        public static Rect Frame(LotGeometry geometry, SpotOccupancy occupancy)
        {
            var highest = 0;
            for (var id = 1; id <= geometry.Spots.Length; id++)
                if (occupancy.IsOpen(id)) highest = id;
            return geometry.FrameFor(geometry.ColumnsFor(highest));
        }
    }
}
