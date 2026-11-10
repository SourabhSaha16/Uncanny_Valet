using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>
    /// Pure placement of the walkway bands (the crosswalks of lot v2), lot-local: from the side door east, the
    /// north-south crosswalk in the corridor, the long crossing along the drive aisle to the right strip, and the open
    /// crossing below the aisle beside the queue column. Everything people walk on shares these centre lines.
    /// </summary>
    public sealed class WalkwayLayout
    {
        /// <summary>Painted bands, in the order: side door leg, north crosswalk, aisle crossing, south crossing.</summary>
        public Rect[] Bands { get; }

        /// <summary>
        /// The same crossings cut so they never overlap, for painting zebra bars: the side-door leg stops at the north crosswalk,
        /// the aisle crossing starts beside the corridor, and the corridor column runs unbroken from the side door to the south wall.
        /// </summary>
        public Rect[] Crosswalks { get; }

        public WalkwayLayout(LotGeometry g, float width)
        {
            var half = width * 0.5f;
            var sideY = g.SideDoor.center.y;
            var x = g.CrossingX;
            Bands = new[]
            {
                Rect.MinMaxRect(g.Diner.xMax, sideY - half, x + half, sideY + half),
                Rect.MinMaxRect(x - half, g.AisleY - half, x + half, sideY + half),
                Rect.MinMaxRect(x - half, g.AisleY - half, g.Strip.xMin, g.AisleY + half),
                Rect.MinMaxRect(x - half, g.Bounds.yMin, x + half, g.AisleY + half)
            };
            Crosswalks = new[]
            {
                Rect.MinMaxRect(g.Diner.xMax, sideY - half, x - half, sideY + half),
                Rect.MinMaxRect(x - half, g.AisleY + half, x + half, sideY + half),
                Rect.MinMaxRect(x + half, g.AisleY - half, g.Strip.xMin, g.AisleY + half),
                Rect.MinMaxRect(x - half, g.Bounds.yMin, x + half, g.AisleY + half)
            };
        }
    }
}
