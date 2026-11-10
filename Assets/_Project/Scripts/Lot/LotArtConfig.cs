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
        [SerializeField] private Vector2 _curbPlateShare = new Vector2(0.7f, 0.92f);
        [Header("Diner and bus stop")]
        [SerializeField, Tooltip("The tall MetroCity building the diner's roof and facade are cut from (set by the editor builder).")]
        private Sprite _dinerBuilding;
        [SerializeField, Tooltip("Plain roof area of the building sprite, in pixels from its top-left; tiled over the roof.")]
        private RectInt _dinerRoofPixels = new RectInt(52, 8, 72, 48);
        [SerializeField, Tooltip("One band of windows of the building sprite, in pixels from its top-left; stacked to make the visible front face.")]
        private RectInt _dinerFacadePixels = new RectInt(11, 132, 149, 96);
        [SerializeField, Range(1, 4), Tooltip("Number of facade bands shown along the bottom of the diner.")]
        private int _dinerFacadeBands = 1;
        [SerializeField] private Color _doorColor = new Color(0.32f, 0.2f, 0.12f);
        [SerializeField, Tooltip("Size of the striped awning over each door (depth into the diner, extra height beyond the door).")]
        private Vector2 _awningSize = new Vector2(1.1f, 0.8f);
        [SerializeField] private Color _awningColorA = new Color(0.78f, 0.06f, 0.06f);
        [SerializeField] private Color _awningColorB = new Color(0.96f, 0.95f, 0.9f);
        [SerializeField, Min(0.1f), Tooltip("Height of one awning stripe.")]
        private float _awningStripe = 0.4f;
        [SerializeField, Tooltip("Size of the small DINER sign on the roof.")]
        private Vector2 _signSize = new Vector2(3f, 1f);
        [SerializeField] private Color _signColor = new Color(0.12f, 0.1f, 0.14f);
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
        public Sprite DinerBuilding => _dinerBuilding;
        public RectInt DinerRoofPixels => _dinerRoofPixels;
        public RectInt DinerFacadePixels => _dinerFacadePixels;
        public int DinerFacadeBands => _dinerFacadeBands;
        public Color DoorColor => _doorColor;
        public Vector2 AwningSize => _awningSize;
        public Color AwningColorA => _awningColorA;
        public Color AwningColorB => _awningColorB;
        public float AwningStripe => _awningStripe;
        public Vector2 SignSize => _signSize;
        public Color SignColor => _signColor;
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
