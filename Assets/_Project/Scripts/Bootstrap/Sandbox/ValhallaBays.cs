using UncannyValet.Lot;
using UnityEngine;

namespace UncannyValet.Bootstrap.Sandbox
{
    /// <summary>The painted gameplay places of the Valhalla lot: the numbered bays, the DROP OFF pad on the curb zone and the VALET PICKUP pad and dashed bays of the arrival queue.</summary>
    public static class ValhallaBays
    {
        private const int PaintOrder = -20;

        /// <summary>Draws the pieces under <paramref name="root"/>, each stretched over the rectangle the lot logic uses for it.</summary>
        public static void Build(Transform root, ValhallaArt art, LotGeometry g)
        {
            for (var i = 0; i < g.Spots.Length && i < BayCount; i++)
                ValhallaDraw.Fit(root, "Bay " + (i + 1), art.Get(ValhallaPiece.Bay1 + i), g.Spots[i], PaintOrder);
            ValhallaDraw.Fit(root, "Drop Off Pad", art.Get(ValhallaPiece.PadDropOff), g.Curb, PaintOrder);
            Queue(root, art, g);
        }

        private static int BayCount => ValhallaPiece.Bay8 - ValhallaPiece.Bay1 + 1;

        // Slot 0 is where the valet picks the arriving car up: the two halves of the VALET PICKUP pad. The rest are dashed waiting bays.
        private static void Queue(Transform root, ValhallaArt art, LotGeometry g)
        {
            for (var i = 0; i < g.QueueCars.Length; i++)
            {
                var slot = g.QueueCars[i];
                if (i > 0)
                {
                    ValhallaDraw.Fit(root, "Queue Bay " + i, art.Get(ValhallaPiece.BayDashedA), slot, PaintOrder);
                    continue;
                }
                var half = slot.width * 0.5f;
                ValhallaDraw.Fit(root, "Pickup Left", art.Get(ValhallaPiece.PadValetPickupLeft), new Rect(slot.xMin, slot.yMin, half, slot.height), PaintOrder);
                ValhallaDraw.Fit(root, "Pickup Right", art.Get(ValhallaPiece.PadValetPickupRight), new Rect(slot.center.x, slot.yMin, half, slot.height), PaintOrder);
            }
        }
    }
}
