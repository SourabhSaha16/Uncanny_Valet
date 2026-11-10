using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>
    /// Pure placement maths: every rectangle of the lot (the spot grid, the curb, the waiting cars) and the frame that
    /// contains what is open, computed from a <see cref="LotConfig"/>. Local lot coordinates. The grid has
    /// rows x columns cells numbered column by column from the curb; the first cells are the stations (charging bays,
    /// then the wash slot) and the rest are the regular spots, numbered 1.. in the same order.
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
        private readonly int _stationCells;

        /// <summary>Regular spots; index + 1 is the spot id, listed column by column from the curb.</summary>
        public Rect[] Spots { get; }
        /// <summary>Charging bays (grid cells); their ids continue after the regular spots.</summary>
        public Rect[] ChargingSpots { get; }
        /// <summary>The wash slot (the grid cell after the charging bays).</summary>
        public Rect Wash { get; }
        public Rect Curb { get; }
        public Vector2[] QueueSlots { get; }
        public Rect[] QueueCars { get; }
        /// <summary>The frame with every grid column open: the largest the lot gets.</summary>
        public Rect Bounds { get; }
        /// <summary>Id of the first charging bay.</summary>
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
            var cells = BuildCells(c, _spotsLeft, spotsHeight * 0.5f);
            _stationCells = Mathf.Clamp(c.ChargingBays + 1, 1, cells.Length - 1);
            ChargingSpots = Slice(cells, 0, _stationCells - 1);
            Wash = cells[_stationCells - 1];
            Spots = Slice(cells, _stationCells, cells.Length - _stationCells);
            var half = spotsHeight * 0.5f + c.Margin;
            _lotWithoutSpots = Rect.MinMaxRect(Curb.xMin - c.Margin, -half, 0f, half);

            QueueSlots = QueueSlotsFor(c);
            QueueCars = Boxes(QueueSlots, c.QueueCarSize, c.QueueRotationDegrees);
            Bounds = FrameFor(_columns);
        }

        /// <summary>
        /// Number of grid columns needed to show regular spot ids 1..<paramref name="highestOpenId"/>, counting the
        /// station cells that come first (at least 1).
        /// </summary>
        public int ColumnsFor(int highestOpenId)
            => Mathf.Clamp((Mathf.Max(0, highestOpenId) + _stationCells + _rows - 1) / _rows, 1, _columns);

        /// <summary>The frame (walls inside edge) around the curb, the queue and the given grid columns.</summary>
        public Rect FrameFor(int columns)
        {
            var cols = Mathf.Clamp(columns, 1, _columns);
            var right = _spotsLeft + cols * _columnPitch - (_columnPitch - _spotWidth) + _margin;
            var frame = Rect.MinMaxRect(_lotWithoutSpots.xMin, _lotWithoutSpots.yMin, right, _lotWithoutSpots.yMax);
            return Grow(frame, QueueCars, _padding);
        }

        public static Rect Box(Vector2 center, Vector2 size) => new Rect(center - size * 0.5f, size);

        private static Rect[] BuildCells(LotConfig c, float left, float top)
        {
            var cells = new Rect[c.Rows * c.SpotsPerRow];
            for (var r = 0; r < c.Rows; r++)
            for (var i = 0; i < c.SpotsPerRow; i++)
            {
                var x = left + c.SpotSize.x * 0.5f + i * (c.SpotSize.x + c.SpotGap);
                var y = top - c.SpotSize.y * 0.5f - r * (c.SpotSize.y + c.AisleWidth);
                cells[i * c.Rows + r] = Box(new Vector2(x, y), c.SpotSize);
            }
            return cells;
        }

        private static Rect[] Slice(Rect[] source, int start, int count)
        {
            var result = new Rect[Mathf.Max(0, count)];
            System.Array.Copy(source, start, result, 0, result.Length);
            return result;
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
