using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>
    /// Pure placement maths for lot v2: the bay grid (<see cref="BayPlan"/>), the curb with the gap west of it, the queue column under the
    /// curb, the diner with its two doors, the right strip, the walkway lines and the road below, all computed from a
    /// <see cref="LotConfig"/>. Local lot coordinates, y up. The frame is fixed: it holds every bay.
    /// </summary>
    public sealed class LotGeometry
    {
        /// <summary>Regular spots; index + 1 is the spot id (reading order, see <see cref="BayPlan"/>).</summary>
        public Rect[] Spots { get; }
        /// <summary>Charging bays; their ids continue after the regular spots.</summary>
        public Rect[] ChargingSpots { get; }
        /// <summary>The wash slot.</summary>
        public Rect Wash { get; }
        public Rect Curb { get; }
        /// <summary>The open ground between the diner wall and the curb zone, where waiting patrons stand (same height as the curb).</summary>
        public Rect CurbGap { get; }
        /// <summary>Waiting car centres; slot 0 is nearest the curb, later slots lie further down the screen.</summary>
        public Vector2[] QueueSlots { get; }
        public Rect[] QueueCars { get; }
        /// <summary>The frame inside the walls: the whole lot, always.</summary>
        public Rect Bounds { get; }
        public Rect Diner { get; }
        /// <summary>The upper door on the diner's right face: ambient people only.</summary>
        public Rect SideDoor { get; }
        /// <summary>The middle door, beside the curb: patrons only.</summary>
        public Rect MainDoor { get; }
        /// <summary>The right strip: the outside road and bus stop where ambient people appear.</summary>
        public Rect Strip { get; }
        /// <summary>Open ground between the curb and the first bay column.</summary>
        public Rect Corridor { get; }
        /// <summary>X of the north-south crosswalk in the corridor.</summary>
        public float CrossingX { get; }
        /// <summary>X where cars leaving the curb run down the corridor.</summary>
        public float ExitX { get; }
        /// <summary>Y of the middle of the drive aisle (and of the long crossing).</summary>
        public float AisleY { get; }
        /// <summary>X range (min, max) of the opening in the south wall for the queue column and the corridor.</summary>
        public Vector2 EntryGap { get; }
        /// <summary>Centre line height of the eastbound lane (next to the lot).</summary>
        public float EastLaneY { get; }
        /// <summary>Centre line height of the westbound lane.</summary>
        public float WestLaneY { get; }
        /// <summary>X range (min, max) the road is drawn and routed over.</summary>
        public Vector2 RoadSpan { get; }
        public float LaneWidth { get; }
        /// <summary>Id of the first charging bay.</summary>
        public int FirstChargingId => Spots.Length + 1;

        public LotGeometry(LotConfig c)
        {
            var s = c.Site;
            Curb = Box(c.CurbPosition, c.CurbSize);
            var spotsLeft = Curb.xMax + c.CurbToSpotsGap;
            Spots = Cells(c, spotsLeft, BayPlan.Regular);
            ChargingSpots = Cells(c, spotsLeft, BayPlan.Charging);
            Wash = Cells(c, spotsLeft, new[] { BayPlan.Wash })[0];
            AisleY = Curb.yMax - c.SpotSize.y - c.AisleWidth * 0.5f;
            QueueSlots = QueueSlotsFor(c);
            QueueCars = Boxes(QueueSlots, c.QueueCarSize, c.QueueRotationDegrees);

            var baysRight = spotsLeft + (BayPlan.Columns - 1) * (c.SpotSize.x + c.SpotGap) + c.SpotSize.x;
            var bottomRow = Curb.yMax - c.SpotSize.y * BayPlan.Rows - c.AisleWidth * (BayPlan.Rows - 1);
            var sideY = Curb.yMax + s.SideDoorRise;
            var top = Mathf.Max(sideY + s.DoorSize.y * 0.5f, Curb.yMax) + c.Margin;
            var queue = ArrivalQueue.Bounds(QueueSlots, c.QueueCarSize, c.QueueRotationDegrees);
            var bottom = Mathf.Min(queue.yMin - c.QueuePadding, bottomRow - c.Margin);
            Strip = new Rect(baysRight + s.StripGap, bottom, s.StripWidth, top - bottom);
            var dinerRight = Curb.xMin - s.CurbGap;
            Bounds = Rect.MinMaxRect(dinerRight - s.DinerWidth, bottom, Strip.xMax, top);
            Diner = Rect.MinMaxRect(Bounds.xMin, bottom + s.DinerInset, dinerRight, top - s.DinerInset);
            CurbGap = Rect.MinMaxRect(dinerRight, Curb.yMin, Curb.xMin, Curb.yMax);
            SideDoor = Box(new Vector2(dinerRight - s.DoorSize.x * 0.5f, sideY), s.DoorSize);
            MainDoor = Box(new Vector2(dinerRight - s.DoorSize.x * 0.5f, Curb.center.y), s.DoorSize);
            Corridor = Rect.MinMaxRect(Curb.xMax, bottom, spotsLeft, top);
            CrossingX = Curb.xMax + c.CurbToSpotsGap * s.CrossingShare;
            ExitX = Curb.xMax + c.CurbToSpotsGap * s.ExitShare;
            EntryGap = new Vector2(queue.xMin - s.EntryPadding, spotsLeft);

            LaneWidth = s.LaneWidth;
            EastLaneY = bottom - c.WallThickness - s.LaneWidth * 0.5f;
            WestLaneY = EastLaneY - s.LaneWidth;
            RoadSpan = new Vector2(Bounds.xMin - c.WallThickness - s.RoadReach, Bounds.xMax + c.WallThickness + s.RoadReach);
        }

        public static Rect Box(Vector2 center, Vector2 size) => new Rect(center - size * 0.5f, size);

        private Rect[] Cells(LotConfig c, float left, Vector2Int[] plan)
        {
            var cells = new Rect[plan.Length];
            for (var i = 0; i < cells.Length; i++)
            {
                var x = left + c.SpotSize.x * 0.5f + plan[i].x * (c.SpotSize.x + c.SpotGap);
                var y = Curb.yMax - c.SpotSize.y * 0.5f - plan[i].y * (c.SpotSize.y + c.AisleWidth);
                cells[i] = Box(new Vector2(x, y), c.SpotSize);
            }
            return cells;
        }

        private Vector2[] QueueSlotsFor(LotConfig c)
        {
            var footprint = ArrivalQueue.Footprint(Vector2.zero, c.QueueCarSize, c.QueueRotationDegrees);
            var dir = c.QueueDirection.sqrMagnitude < 0.0001f ? Vector2.down : c.QueueDirection.normalized;
            var along = Mathf.Abs(dir.x) * footprint.width + Mathf.Abs(dir.y) * footprint.height;
            var first = new Vector2(c.CurbPosition.x, Curb.yMin - c.QueueStartGap - footprint.height * 0.5f);
            return ArrivalQueue.Slots(first, c.QueueSlotCount, dir, ArrivalQueue.Pitch(along, c.QueueSlotGap));
        }

        private static Rect[] Boxes(Vector2[] centres, Vector2 size, float degrees)
        {
            var rects = new Rect[centres.Length];
            for (var i = 0; i < rects.Length; i++) rects[i] = ArrivalQueue.Footprint(centres[i], size, degrees);
            return rects;
        }
    }
}
