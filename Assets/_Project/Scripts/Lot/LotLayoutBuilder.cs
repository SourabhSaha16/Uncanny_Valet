using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>
    /// Builds ground, walls, curb and parking spots from a <see cref="LotConfig"/> at Awake, using
    /// generated coloured rectangles. Centred on its own transform. 0 degrees = car facing +Y (up).
    /// </summary>
    public class LotLayoutBuilder : MonoBehaviour, ILotLayout
    {
        private static readonly Color Ground = new Color(0.22f, 0.23f, 0.26f);
        private static readonly Color Line = new Color(0.92f, 0.92f, 0.85f);
        private static readonly Color SpotFill = new Color(0.30f, 0.32f, 0.37f);
        private static readonly Color CurbEdge = new Color(0.95f, 0.78f, 0.15f);
        private static readonly Color CurbFill = new Color(0.55f, 0.42f, 0.12f);
        private static readonly Color Wall = new Color(0.55f, 0.28f, 0.25f);

        [SerializeField] private LotConfig _config;

        private readonly List<int> _spotIds = new List<int>();
        private readonly List<ParkingSpot> _spots = new List<ParkingSpot>();
        private SpotOccupancy _occupancy;
        private RectFactory _rects;
        private Vector2[] _arrivalSlots = { Vector2.zero };

        public Vector2 ArrivalPoint => _arrivalSlots[0];
        public IReadOnlyList<Vector2> ArrivalSlots => _arrivalSlots;
        public float ArrivalRotationDegrees => _config.QueueRotationDegrees;
        public Vector2 ValetStartPoint { get; private set; }
        public IReadOnlyList<int> SpotIds => _spotIds;

        public bool IsSpotFree(int spotId) => _occupancy != null && _occupancy.IsFree(spotId);

        private void Awake() => Build();

        private void Build()
        {
            _rects = new RectFactory();
            var c = _config;
            var g = new LotGeometry(c);
            var bounds = g.Bounds;
            _arrivalSlots = g.QueueSlots;

            _rects.Make(transform, "Ground", bounds.center, bounds.size, Ground, -10);
            BuildSpots(g.Spots);
            BuildCurb();
            var stations = StationBuilder.Build(_rects, transform, c, g.Stations);
            gameObject.AddComponent<StationSwitch>().Init(stations);
            WallBuilder.Build(_rects, transform, bounds, c.WallThickness, Wall);
            _occupancy = new SpotOccupancy(_spotIds);
            InitSpots();
            ValetStartPoint = c.CurbPosition + new Vector2(c.CurbSize.x * 0.5f + c.ValetStartOffset, 0f);
        }

        private void BuildSpots(Rect[] areas)
        {
            var c = _config;
            foreach (var area in areas)
            {
                var id = _spotIds.Count + 1;
                var pos = area.center;
                _rects.Make(transform, "Spot Line " + id, pos, c.SpotSize, Line, -7);
                _rects.Make(transform, "Spot Fill " + id, pos, c.SpotSize - Vector2.one * 0.16f, SpotFill, -6);
                SpotLabel.Create(transform, id.ToString(), pos, c, -5);
                var zone = new GameObject("Spot " + id);
                zone.transform.SetParent(transform, false);
                zone.transform.localPosition = pos;
                zone.AddComponent<BoxCollider2D>();
                _spots.Add(zone.AddComponent<ParkingSpot>());
                _spotIds.Add(id);
            }
        }

        private void InitSpots()
        {
            for (var i = 0; i < _spots.Count; i++) _spots[i].Init(_spotIds[i], _config, _occupancy);
        }

        private void BuildCurb()
        {
            var c = _config;
            _rects.Make(transform, "Curb Edge", c.CurbPosition, c.CurbSize, CurbEdge, -7);
            _rects.Make(transform, "Curb Fill", c.CurbPosition, c.CurbSize - Vector2.one * 0.3f, CurbFill, -6);
            SpotLabel.Create(transform, string.Join("\n", "CURB".ToCharArray()), c.CurbPosition, c, -5, c.CurbLabelSize);
            var zone = new GameObject("CurbZone");
            zone.transform.SetParent(transform, false);
            zone.transform.localPosition = c.CurbPosition;
            zone.AddComponent<BoxCollider2D>();
            zone.AddComponent<CurbZone>().Init(c);
        }
    }
}
