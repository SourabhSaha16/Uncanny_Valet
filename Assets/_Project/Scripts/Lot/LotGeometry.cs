using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>
    /// Pure placement maths: every rectangle of the lot (spots, charging spots, curb, waiting cars, wash slot)
    /// and the frame that contains what is open, computed from a <see cref="LotConfig"/>. Local lot coordinates.
    /// </summary>
    public sealed class LotGeometry
    {
        private readonly Rect _lotWithoutSpots;
        private readonly float _spotsLeft;
        private readonly float _columnPitch;
        private readonly float _spotWidth;
        private readonly float _margin;
        private readonly float _padding;
        private readonly int _rows;
        private readonly int _columns;
        private readonly Rect[] _washOnly;

        /// <summary>Regular spots; index + 1 is the spot id, listed column by column from the curb.</summary>
        public Rect[] Spots { get; }
        /// <summary>Charging spots; their ids continue after the regular spots.</summary>
        public Rect[] ChargingSpots { get; }
        public Rect Wash { get; }
        public Rect Curb { get; }
        public Vector2[] QueueSlots { get; }
        public Rect[] QueueCars { get; }
        /// <summary>The frame with every spot, charging spot and the wash open: the largest the lot gets.</summary>
        public Rect Bounds { get; }
        /// <summary>Id of the first charging spot.</summary>
        public int FirstChargingId => Spots.Length + 1;

        public LotGeometry(LotConfig c)
        {
            Curb = Box(c.CurbPosition, c.CurbSize);
            _spotsLeft = Curb.xMax + c.CurbToSpotsGap;
            _rows = c.Rows;
            _columns = c.SpotsPerRow;
            _spotWidth = c.SpotSize.x;
            _columnPitch = c.SpotSize.x + c.SpotGap;
            _margin = c.Margin;
            _padding = c.QueuePadding;
            var spotsHeight = c.Rows * c.SpotSize.y + (c.Rows - 1) * c.AisleWidth;
            Spots = BuildSpots(c, _spotsLeft, spotsHeight * 0.5f);
            var half = spotsHeight * 0.5f + c.Margin;
            _lotWithoutSpots = Rect.MinMaxRect(Curb.xMin - c.Margin, -half, 0f, half);

            QueueSlots = QueueSlotsFor(c);
            QueueCars = Boxes(QueueSlots, c.QueueCarSize, c.QueueRotationDegrees);
            var centres = ArrivalQueue.Slots(c.StationsFirstCenter, c.ChargingBays + 1, Vector2.right,
                ArrivalQueue.Pitch(c.StationSize.x, c.StationGap));
            var boxes = Boxes(centres, c.StationSize, 0f);
            ChargingSpots = new Rect[c.ChargingBays];
            System.Array.Copy(boxes, ChargingSpots, ChargingSpots.Length);
            Wash = boxes[boxes.Length - 1];
            _washOnly = new[] { Wash };
            Bounds = FrameFor(_columns, true, true);
        }

        /// <summary>Number of spot columns needed to show spot ids 1..<paramref name="highestOpenId"/> (at least 1).</summary>
        public int ColumnsFor(int highestOpenId) => Mathf.Clamp((highestOpenId + _rows - 1) / _rows, 1, _columns);

        /// <summary>The frame (walls inside edge) around the curb, the queue, the given spot columns and the optional extras.</summary>
        public Rect FrameFor(int columns, bool charging, bool wash)
        {
            var cols = Mathf.Clamp(columns, 1, _columns);
            var right = _spotsLeft + cols * _columnPitch - (_columnPitch - _spotWidth) + _margin;
            var frame = Rect.MinMaxRect(_lotWithoutSpots.xMin, _lotWithoutSpots.yMin, right, _lotWithoutSpots.yMax);
            frame = Grow(frame, QueueCars, _padding);
            if (charging) frame = Grow(frame, ChargingSpots, _padding);
            return wash ? Grow(frame, _washOnly, _padding) : frame;
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
                spots[i * c.Rows + r] = Box(new Vector2(x, y), c.SpotSize);
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
