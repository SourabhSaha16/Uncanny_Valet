using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>Builds one parking spot object (lines, pad, number, trigger); charging spots get a tint and a bolt.</summary>
    public static class SpotBuilder
    {
        private static readonly Color Line = new Color(0.92f, 0.92f, 0.85f);
        private static readonly Color Pad = new Color(0.30f, 0.32f, 0.37f);
        private static readonly Color ChargeLine = new Color(0.30f, 0.60f, 1f);
        private static readonly Color ChargePad = new Color(0.14f, 0.24f, 0.46f);
        private static readonly Color Bolt = new Color(1f, 0.88f, 0.2f);
        private const float BoltHeight = 1.3f;
        private const float BoltThickness = 0.22f;
        private const float BoltYOffsetShare = 0.3f;

        /// <summary>Creates the spot inactive; the caller switches it on when it opens.</summary>
        public static ParkingSpot Build(RectFactory rects, Transform parent, LotConfig c, int id, Rect area,
            bool charging, SpotOccupancy occupancy)
        {
            var root = new GameObject((charging ? "Charging Spot " : "Spot ") + id);
            root.transform.SetParent(parent, false);
            root.transform.localPosition = area.center;
            rects.Make(root.transform, "Line", Vector2.zero, c.SpotSize, charging ? ChargeLine : Line, -7);
            rects.Make(root.transform, "Pad", Vector2.zero, c.SpotSize - Vector2.one * 0.16f, charging ? ChargePad : Pad, -6);
            SpotLabel.Create(root.transform, id.ToString(), Vector2.zero, c, -5);
            if (charging) AddBolt(rects, root.transform, c.SpotSize.y * BoltYOffsetShare);
            root.AddComponent<BoxCollider2D>();
            var spot = root.AddComponent<ParkingSpot>();
            spot.Init(id, c, occupancy, charging);
            root.SetActive(false);
            return spot;
        }

        private static void AddBolt(RectFactory rects, Transform parent, float y)
        {
            var h = BoltHeight;
            var top = new Vector2(0.25f * h, y + 0.5f * h);
            var left = new Vector2(-0.15f * h, y);
            var right = new Vector2(0.15f * h, y);
            var bottom = new Vector2(-0.25f * h, y - 0.5f * h);
            rects.Segment(parent, "Bolt 1", top, left, BoltThickness, Bolt, -4);
            rects.Segment(parent, "Bolt 2", left, right, BoltThickness, Bolt, -4);
            rects.Segment(parent, "Bolt 3", right, bottom, BoltThickness, Bolt, -4);
        }
    }
}
