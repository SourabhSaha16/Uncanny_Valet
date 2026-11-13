using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>
    /// Builds ground, walls (with the entry opening in the south wall and the exit opening in the top wall), the diner's solid block, curb, spots and
    /// charging spots from a <see cref="LotConfig"/> at Awake, using generated coloured rectangles. Only open spots
    /// are shown; <see cref="LotExpander"/> opens and closes them with the night plan. Local origin is the lot's own
    /// transform. 0 degrees = car facing +Y (up).
    /// </summary>
    public class LotLayoutBuilder : MonoBehaviour, ILotLayout
    {
        private static readonly Color Ground = new Color(0.22f, 0.23f, 0.26f);
        private static readonly Color CurbEdge = new Color(0.95f, 0.78f, 0.15f);
        private static readonly Color CurbFill = new Color(0.55f, 0.42f, 0.12f);
        private static readonly Color Wall = new Color(0.55f, 0.28f, 0.25f);

        [SerializeField] private LotConfig _config;
        [SerializeField, Tooltip("Optional scenery art: asphalt, hedge walls, sidewalk curb and painted bays.")]
        private LotArtConfig _art;
        [SerializeField, Min(0), Tooltip("Arrival queue slots of this lot (the lot definition's); 0 uses the LotConfig's count. Set by the scene builder.")]
        private int _queueSlots;

        private LotExpander _expander;
        private SpotOccupancy _occupancy;
        private IRoadLayout _road;
        private Vector2[] _arrivalSlots = { Vector2.zero };

        public Vector2 ArrivalPoint => _arrivalSlots[0];
        public IReadOnlyList<Vector2> ArrivalSlots => _arrivalSlots;
        public float ArrivalRotationDegrees => _config.QueueRotationDegrees;
        public Vector2 ValetStartPoint { get; private set; }
        /// <summary>Starting Perfect Park and drop-off thresholds, for Bootstrap to seed into the tuning store.</summary>
        public PerfectSettings Perfect => _config.Perfect;
        /// <summary>Starting queue-clear distance for the stay start rule, for Bootstrap to seed into the tuning store.</summary>
        public QueueClearSettings QueueClear => _config.QueueClear;
        public int RegularSpotCount { get; private set; }
        public int ChargingSpotCount { get; private set; }

        /// <summary>Ids of the open spots: the regular spots the plan opens plus the charging spots while charging is open.</summary>
        public IReadOnlyList<int> SpotIds => _expander != null ? _expander.OpenIds : (IReadOnlyList<int>)System.Array.Empty<int>();

        /// <summary>The road below the lot (lanes and the queue and curb routes); null until the lot has built itself.</summary>
        public IRoadLayout Road => _road;

        public bool IsSpotFree(int spotId) => _occupancy != null && _occupancy.IsFree(spotId);

        public bool IsChargingSpot(int spotId) => _occupancy != null && _occupancy.IsCharging(spotId);

        private void Awake() => Build();

        private void Build()
        {
            var rects = new RectFactory();
            var c = _config;
            var g = new LotGeometry(c, _queueSlots);
            _arrivalSlots = g.QueueSlots;
            RegularSpotCount = g.Spots.Length;
            ChargingSpotCount = g.ChargingSpots.Length;
            _road = new RoadLayout(g, c.Site, transform.position);
            new LotFrame(rects, transform, c.WallThickness,
                _art != null ? _art.AsphaltColor : Ground, _art != null ? _art.HedgeColor : Wall).Apply(g.Bounds, g.EntryGap, g.ExitGap);
            BuildDiner(g);
            var judge = gameObject.AddComponent<PerfectRunner>();
            var curb = BuildCurb(rects);
            judge.Init(c, curb);
            gameObject.AddComponent<QueueClearRunner>().Init(c, c.QueueClear);
            var style = _art != null ? _art.Walkway : new WalkwayStyle();
            LotQueueArt.Build(transform, rects, g.QueueCars, style, _art != null ? _art.CrosswalkColor : Color.white);
            var wash = StationBuilder.BuildWash(rects, transform, c, g.Wash);
            gameObject.AddComponent<StationSwitch>().Init(wash);

            var ids = new List<int>();
            for (var i = 1; i <= g.Spots.Length + g.ChargingSpots.Length; i++) ids.Add(i);
            _occupancy = new SpotOccupancy(ids, g.FirstChargingId);
            var spots = new ParkingSpot[ids.Count];
            for (var i = 0; i < spots.Length; i++)
            {
                var charging = i >= g.Spots.Length;
                var area = charging ? g.ChargingSpots[i - g.Spots.Length] : g.Spots[i];
                spots[i] = SpotBuilder.Build(rects, transform, c, i + 1, area, charging, _occupancy,
                    _art != null ? _art.AsphaltColor : (Color?)null, judge);
            }
            _expander = gameObject.AddComponent<LotExpander>();
            _expander.Init(_occupancy, spots);
            ValetStartPoint = c.CurbPosition + new Vector2(c.CurbSize.x * 0.5f + c.ValetStartOffset, 0f);
        }

        // The diner is a solid block: cars and the valet cannot drive into the building.
        private void BuildDiner(LotGeometry g)
        {
            var block = new GameObject("Diner Block");
            block.transform.SetParent(transform, false);
            block.transform.localPosition = g.Diner.center;
            block.AddComponent<BoxCollider2D>().size = g.Diner.size;
        }

        private CurbZone BuildCurb(RectFactory rects)
        {
            var c = _config;
            if (_art != null && _art.SidewalkTile != null)
            {
                LotCurbArt.Build(rects, transform, c, _art);
            }
            else
            {
                rects.Make(transform, "Curb Edge", c.CurbPosition, c.CurbSize, CurbEdge, -7);
                rects.Make(transform, "Curb Fill", c.CurbPosition, c.CurbSize - Vector2.one * 0.3f, CurbFill, -6);
            }
            LotCurbArt.BuildLabel(transform, c);
            var arrow = LotCurbArrows.Build(rects, transform, c);
            var zone = new GameObject("CurbZone");
            zone.transform.SetParent(transform, false);
            zone.transform.localPosition = c.CurbPosition;
            zone.AddComponent<BoxCollider2D>();
            var curb = zone.AddComponent<CurbZone>();
            curb.Init(c, arrow);
            return curb;
        }
    }
}
