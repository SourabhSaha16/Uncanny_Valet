using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>
    /// Pure placement maths: every rectangle of the lot (spots, curb, waiting cars, stations) and the
    /// bounds that contain them, computed from a <see cref="LotConfig"/>. Local lot coordinates.
    /// </summary>
    public sealed class LotGeometry
    {
        public Rect[] Spots { get; }
        public Rect Curb { get; }
        public Vector2[] QueueSlots { get; }
        public Rect[] QueueCars { get; }
        /// <summary>Charging bays first, then the wash slot (last).</summary>
        public Rect[] Stations { get; }
        public Rect Bounds { get; }

        public LotGeometry(LotConfig c)
        {
            Curb = Box(c.CurbPosition, c.CurbSize);
            var spotsLeft = Curb.xMax + c.CurbToSpotsGap;
            var spotsHeight = c.Rows * c.SpotSize.y + (c.Rows - 1) * c.AisleWidth;
            Spots = BuildSpots(c, spotsLeft, spotsHeight * 0.5f);

            var rowWidth = c.SpotsPerRow * (c.SpotSize.x + c.SpotGap) - c.SpotGap;
            var half = spotsHeight * 0.5f + c.Margin;
            var lot = Rect.MinMaxRect(Curb.xMin - c.Margin, -half, spotsLeft + rowWidth + c.Margin, half);

            QueueSlots = QueueSlotsFor(c);
            QueueCars = Boxes(QueueSlots, c.QueueCarSize, c.QueueRotationDegrees);
            var stationCentres = ArrivalQueue.Slots(c.StationsFirstCenter, c.ChargingBays + 1, Vector2.right,
                ArrivalQueue.Pitch(c.StationSize.x, c.StationGap));
            Stations = Boxes(stationCentres, c.StationSize, 0f);

            Bounds = Grow(Grow(lot, QueueCars, c.QueuePadding), Stations, c.QueuePadding);
        }

        public static Rect Box(Vector2 center, Vector2 size) => new Rect(center - size * 0.5f, size);

        private static Rect[] BuildSpots(LotConfig c, float left, float top)
        {
            var spots = new Rect[c.Rows * c.SpotsPerRow];
            for (var r = 0; r < c.Rows; r++)
            for (var i = 0; i < c.SpotsPerRow; i++)
            {
                var x = left + c.SpotSize.x * 0.5f + i * (c.SpotSize.x + c.SpotGap);
                var y = top - c.SpotSize.y * 0.5f - r * (c.SpotSize.y + c.AisleWidth);
                spots[r * c.SpotsPerRow + i] = Box(new Vector2(x, y), c.SpotSize);
            }
            return spots;
        }

        private Vector2[] QueueSlotsFor(LotConfig c)
        {
            var noseToCentre = ArrivalQueue.Footprint(Vector2.zero, c.QueueCarSize, c.QueueRotationDegrees).height * 0.5f;
            var first = new Vector2(c.CurbPosition.x, Curb.yMin - c.QueueStartGap - noseToCentre);
            return ArrivalQueue.Slots(first, c.QueueSlotCount, c.QueueDirection, ArrivalQueue.Pitch(c.QueueCarSize.x, c.QueueSlotGap));
        }

        private static Rect[] Boxes(Vector2[] centres, Vector2 size, float degrees)
        {
            var rects = new Rect[centres.Length];
            for (var i = 0; i < rects.Length; i++) rects[i] = ArrivalQueue.Footprint(centres[i], size, degrees);
            return rects;
        }

        private static Rect Grow(Rect lot, Rect[] rects, float pad)
        {
            foreach (var r in rects)
                lot = Rect.MinMaxRect(Mathf.Min(lot.xMin, r.xMin - pad), Mathf.Min(lot.yMin, r.yMin - pad),
                    Mathf.Max(lot.xMax, r.xMax + pad), Mathf.Max(lot.yMax, r.yMax + pad));
            return lot;
        }
    }
}
