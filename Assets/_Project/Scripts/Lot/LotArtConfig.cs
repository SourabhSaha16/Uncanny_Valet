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
        [SerializeField] private Sprite _building;
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
        [SerializeField, Min(1f)] private float _roadHeight = 3f;
        [SerializeField, Min(0f), Tooltip("How far the road and sidewalks reach past the largest lot frame.")] private float _sideReach = 8f;
        [SerializeField, Min(1f)] private float _grassReach = 40f;
        [SerializeField, Tooltip("Distance of the crosswalk east of the curb centre.")] private float _crosswalkOffset = 5f;
        [SerializeField, Min(1)] private int _crosswalkBars = 5;
        [SerializeField, Min(0.1f)] private float _crosswalkBarWidth = 0.45f;
        [SerializeField, Min(0f)] private float _crosswalkBarGap = 0.45f;
        [SerializeField, Tooltip("Height of the dash tile centre below the road middle (the dash sits near the tile top).")]
        private float _dashTileShift = 0.27f;
        [SerializeField, Min(0)] private int _tuftCount = 40;
        [SerializeField, Min(0.01f)] private float _curbEdgeWidth = 0.15f;
        [SerializeField] private Vector2 _curbPlateShare = new Vector2(0.5f, 0.8f);
        [Header("Camera view")]
        [SerializeField, Range(0f, 1f), Tooltip("Share of the diner's height kept in view above the lot (the rest is cropped).")]
        private float _viewBuildingShare = 0.2f;
        [SerializeField, Min(0f), Tooltip("Road and sidewalk kept in view below the lot, as a share of their depth.")]
        private float _viewRoadShare = 0.6f;
        [Header("Walkways")]
        [SerializeField] private WalkwayStyle _walkway = new WalkwayStyle();

        public Sprite SidewalkTile => _sidewalkTile;
        public Sprite RoadDashTile => _roadDashTile;
        public Sprite Building => _building;
        public Sprite[] Trees => _trees;
        public Sprite[] Tufts => _tufts;
        public Color GrassColor => _grassColor;
        public Color AsphaltColor => _asphaltColor;
        public Color HedgeColor => _hedgeColor;
        public Color CrosswalkColor => _crosswalkColor;
        public Color CurbEdgeColor => _curbEdgeColor;
        public Color CurbPlateColor => _curbPlateColor;
        public float SidewalkDepth => _sidewalkDepth;
        public float RoadHeight => _roadHeight;
        public float SideReach => _sideReach;
        public float GrassReach => _grassReach;
        public float CrosswalkOffset => _crosswalkOffset;
        public int CrosswalkBars => _crosswalkBars;
        public float CrosswalkBarWidth => _crosswalkBarWidth;
        public float CrosswalkBarGap => _crosswalkBarGap;
        public float DashTileShift => _dashTileShift;
        public int TuftCount => _tuftCount;
        public float CurbEdgeWidth => _curbEdgeWidth;
        public Vector2 CurbPlateShare => _curbPlateShare;
        public float ViewBuildingShare => _viewBuildingShare;
        public float ViewRoadShare => _viewRoadShare;
        public WalkwayStyle Walkway => _walkway;
    }
}
