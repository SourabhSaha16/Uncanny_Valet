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

        public Vector2 ArrivalPoint { get; private set; }
        public float ArrivalRotationDegrees => 0f;
        public Vector2 ValetStartPoint { get; private set; }
        public IReadOnlyList<int> SpotIds => _spotIds;

        public bool IsSpotFree(int spotId) => _occupancy != null && _occupancy.IsFree(spotId);

        private void Awake() => Build();

        private void Build()
        {
            _rects = new RectFactory();
            var c = _config;
            var rowWidth = c.SpotsPerRow * (c.SpotSize.x + c.SpotGap) - c.SpotGap;
            var spotsHeight = c.Rows * c.SpotSize.y + (c.Rows - 1) * c.AisleWidth;
            var left = c.CurbPosition.x - c.CurbSize.x * 0.5f - c.Margin;
            var spotsLeft = c.CurbPosition.x + c.CurbSize.x * 0.5f + c.CurbToSpotsGap;
            var right = spotsLeft + rowWidth + c.Margin;
            var half = spotsHeight * 0.5f + c.Margin;
            var bounds = new Rect(left, -half, right - left, half * 2f);

            _rects.Make(transform, "Ground", bounds.center, bounds.size, Ground, -10);
            BuildSpots(spotsLeft, spotsHeight * 0.5f);
            BuildCurb();
            BuildWalls(bounds);
            _occupancy = new SpotOccupancy(_spotIds);
            InitSpots();
            ArrivalPoint = new Vector2(c.CurbPosition.x, bounds.yMin + c.ArrivalInset);
            ValetStartPoint = c.CurbPosition + new Vector2(c.CurbSize.x * 0.5f + c.ValetStartOffset, 0f);
        }

        private void BuildSpots(float spotsLeft, float top)
        {
            var c = _config;
            for (var r = 0; r < c.Rows; r++)
            for (var i = 0; i < c.SpotsPerRow; i++)
            {
                var id = _spotIds.Count + 1;
                var x = spotsLeft + c.SpotSize.x * 0.5f + i * (c.SpotSize.x + c.SpotGap);
                var y = top - c.SpotSize.y * 0.5f - r * (c.SpotSize.y + c.AisleWidth);
                var pos = new Vector2(x, y);
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
            SpotLabel.Create(transform, "CURB", c.CurbPosition, c, -5);
            var zone = new GameObject("CurbZone");
            zone.transform.SetParent(transform, false);
            zone.transform.localPosition = c.CurbPosition;
            zone.AddComponent<BoxCollider2D>();
            zone.AddComponent<CurbZone>().Init(c);
        }

        private void BuildWalls(Rect b)
        {
            var t = _config.WallThickness;
            AddWall("Wall N", new Vector2(b.center.x, b.yMax + t * 0.5f), new Vector2(b.width + t * 2f, t));
            AddWall("Wall S", new Vector2(b.center.x, b.yMin - t * 0.5f), new Vector2(b.width + t * 2f, t));
            AddWall("Wall W", new Vector2(b.xMin - t * 0.5f, b.center.y), new Vector2(t, b.height));
            AddWall("Wall E", new Vector2(b.xMax + t * 0.5f, b.center.y), new Vector2(t, b.height));
        }

        private void AddWall(string wallName, Vector2 center, Vector2 size)
        {
            var go = _rects.Make(transform, wallName, center, size, Wall, 0);
            go.AddComponent<BoxCollider2D>().size = Vector2.one;
        }
    }
}
