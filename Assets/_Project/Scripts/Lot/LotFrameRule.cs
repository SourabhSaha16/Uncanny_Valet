using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>Pure rule: the frame of the lot for whatever is open right now.</summary>
    public static class LotFrameRule
    {
        /// <summary>Frame around the curb, the queue, the columns holding an open spot, and open extras.</summary>
        public static Rect Frame(LotGeometry geometry, SpotOccupancy occupancy, bool washOpen)
        {
            var highest = 0;
            for (var id = 1; id <= geometry.Spots.Length; id++)
                if (occupancy.IsOpen(id)) highest = id;
            var charging = false;
            for (var i = 0; i < geometry.ChargingSpots.Length; i++)
                charging |= occupancy.IsOpen(geometry.FirstChargingId + i);
            return geometry.FrameFor(geometry.ColumnsFor(highest), charging, washOpen);
        }
    }
}
