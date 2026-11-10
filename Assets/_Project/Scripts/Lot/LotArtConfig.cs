using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>Sprites, colours and sizes for the diner scenery around the lot. Sprites are set by the editor builder.</summary>
    [CreateAssetMenu(menuName = "Uncanny Valet/Lot Art Config", fileName = "LotArtConfig")]
    public sealed class LotArtConfig : ScriptableObject
    {
        [Header("Sprites")]
        [SerializeField] private Sprite _sidewalkTile;
        [SerializeField, Tooltip("Road tile with the dashed centre line near its top edge.")] private Sprite _roadDashTile;
        [SerializeField] private Sprite[] _trees = new Sprite[0];
        [SerializeField] private Sprite[] _tufts = new Sprite[0];
        [Header("Colours")]
        [SerializeField] private Color _grassColor = new Color(0.42f, 0.66f, 0.28f);
        [SerializeField, Tooltip("Matches the plain asphalt of the road tiles.")] private Color _asphaltColor = new Color(0.53f, 0.53f, 0.53f);
        [SerializeField] private Color _hedgeColor = new Color(0.2f, 0.42f, 0.22f);
        [SerializeField] private Color _crosswalkColor = new Color(0.97f, 0.97f, 0.94f);
        [SerializeField] private Color _curbEdgeColor = new Color(0.95f, 0.78f, 0.15f);
        [SerializeField] private Color _curbPlateColor = new Color(0f, 0f, 0f, 0.45f);
        [Header("Layout")]
        [SerializeField, Min(0.5f)] private float _sidewalkDepth = 1.5f;
        [SerializeField, Min(1f)] private float _grassReach = 40f;
        [SerializeField, Tooltip("Height of the dash tile centre below the road middle (the dash sits near the tile top).")]
        private float _dashTileShift = 0.27f;
        [SerializeField, Min(0)] private int _tuftCount = 40;
        [SerializeField, Min(0.01f)] private float _curbEdgeWidth = 0.15f;
        [SerializeField] private Vector2 _curbPlateShare = new Vector2(0.5f, 0.8f);
        [Header("Diner and bus stop")]
        [SerializeField] private Color _dinerWallColor = new Color(0.78f, 0.62f, 0.42f);
        [SerializeField] private Color _dinerRoofColor = new Color(0.36f, 0.52f, 0.34f);
        [SerializeField] private Color _doorColor = new Color(0.32f, 0.2f, 0.12f);
        [SerializeField, Min(0f), Tooltip("Width of the wall-coloured rim around the green roof.")]
        private float _dinerRoofInset = 0.6f;
        [SerializeField] private Color _busStopColor = new Color(0.25f, 0.45f, 0.7f);
        [SerializeField, Tooltip("Size of the bus stop shelter on the right strip.")]
        private Vector2 _busStopSize = new Vector2(1.2f, 3f);
        [SerializeField, Min(0f), Tooltip("Distance of the bus stop from the bottom of the strip.")]
        private float _busStopFromBottom = 2f;
        [Header("Walkways")]
        [SerializeField] private WalkwayStyle _walkway = new WalkwayStyle();

        public Sprite SidewalkTile => _sidewalkTile;
        public Sprite RoadDashTile => _roadDashTile;
        public Sprite[] Trees => _trees;
        public Sprite[] Tufts => _tufts;
        public Color GrassColor => _grassColor;
        public Color AsphaltColor => _asphaltColor;
        public Color HedgeColor => _hedgeColor;
        public Color CrosswalkColor => _crosswalkColor;
        public Color CurbEdgeColor => _curbEdgeColor;
        public Color CurbPlateColor => _curbPlateColor;
        public Color DinerWallColor => _dinerWallColor;
        public Color DinerRoofColor => _dinerRoofColor;
        public Color DoorColor => _doorColor;
        public float DinerRoofInset => _dinerRoofInset;
        public Color BusStopColor => _busStopColor;
        public Vector2 BusStopSize => _busStopSize;
        public float BusStopFromBottom => _busStopFromBottom;
        public float SidewalkDepth => _sidewalkDepth;
        public float GrassReach => _grassReach;
        public float DashTileShift => _dashTileShift;
        public int TuftCount => _tuftCount;
        public float CurbEdgeWidth => _curbEdgeWidth;
        public Vector2 CurbPlateShare => _curbPlateShare;
        public WalkwayStyle Walkway => _walkway;
    }
}
