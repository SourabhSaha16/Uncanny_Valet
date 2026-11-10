using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>Builds one parking spot object (lines, pad, number, trigger); charging spots get a tint and a plug icon under the number.</summary>
    public static class SpotBuilder
    {
        private static readonly Color Line = new Color(0.92f, 0.92f, 0.85f);
        private static readonly Color Pad = new Color(0.30f, 0.32f, 0.37f);
        private static readonly Color ChargeLine = new Color(0.30f, 0.60f, 1f);
        private static readonly Color ChargePad = new Color(0.14f, 0.24f, 0.46f);
        private const float LineWidth = 0.1f;
        private const float PlugPixel = 0.16f;
        private const float PlugYOffsetShare = -0.33f;
        private const int PlugOrder = -4;

        /// <summary>Creates the spot inactive; the caller switches it on when it opens.</summary>
        /// <param name="regularPad">Fill of a regular bay; pass the asphalt colour for painted lines on the asphalt.</param>
        public static ParkingSpot Build(RectFactory rects, Transform parent, LotConfig c, int id, Rect area,
            bool charging, SpotOccupancy occupancy, Color? regularPad = null)
        {
            var root = new GameObject((charging ? "Charging Spot " : "Spot ") + id);
            root.transform.SetParent(parent, false);
            root.transform.localPosition = area.center;
            rects.Make(root.transform, "Line", Vector2.zero, c.SpotSize, charging ? ChargeLine : Line, -7);
            rects.Make(root.transform, "Pad", Vector2.zero, c.SpotSize - Vector2.one * LineWidth * 2f,
                charging ? ChargePad : regularPad ?? Pad, -6);
            SpotLabel.Create(root.transform, id.ToString(), Vector2.zero, c, -5);
            if (charging) PlugDecal.Build(rects, root.transform, new Vector2(0f, c.SpotSize.y * PlugYOffsetShare), PlugPixel, PlugOrder);
            root.AddComponent<BoxCollider2D>();
            var spot = root.AddComponent<ParkingSpot>();
            spot.Init(id, c, occupancy, charging);
            root.SetActive(false);
            return spot;
        }
    }
}
