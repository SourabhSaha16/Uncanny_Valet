using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>Sprites, colours and sizes for the diner scenery around the lot. Sprites are set by the editor builder.</summary>
    [CreateAssetMenu(menuName = "Uncanny Valet/Lot Art Config", fileName = "LotArtConfig")]
    public sealed class LotArtConfig : ScriptableObject
    {
        [Header("Sprites")]
        [SerializeField] private Sprite _sidewalkTile;
        [Header("Colours")]
        [SerializeField, Tooltip("The ground outside the lot walls (left, right, above and around the road): darker than the lot asphalt.")]
        private Color _outsideColor = new Color(0.36f, 0.36f, 0.38f);
        [SerializeField, Tooltip("Matches the plain asphalt of the road tiles.")] private Color _asphaltColor = new Color(0.53f, 0.53f, 0.53f);
        [SerializeField] private Color _hedgeColor = new Color(0.2f, 0.42f, 0.22f);
        [SerializeField] private Color _crosswalkColor = new Color(0.97f, 0.97f, 0.94f);
        [SerializeField] private Color _curbEdgeColor = new Color(0.95f, 0.78f, 0.15f);
        [SerializeField] private Color _curbPlateColor = new Color(0f, 0f, 0f, 0.45f);
        [Header("Layout")]
        [SerializeField, Min(0.5f)] private float _sidewalkDepth = 1.5f;
        [SerializeField, Min(1f), Tooltip("How far the outside ground reaches past the lot and the road.")] private float _outsideReach = 40f;
        [SerializeField, Min(0.01f)] private float _curbEdgeWidth = 0.15f;
        [SerializeField] private Vector2 _curbPlateShare = new Vector2(0.7f, 0.92f);
        [Header("Road labels (painted on the roads, pixel style)")]
        [SerializeField, Tooltip("Word painted on the entry road below the lot.")] private string _entryWord = "ENTRY";
        [SerializeField, Tooltip("Word painted on the exit road above the lot.")] private string _exitWord = "EXIT";
        [SerializeField, Min(0.02f), Tooltip("Size of one pixel of a road label, world units.")] private float _roadLabelPixel = 0.14f;
        [SerializeField, Range(0f, 1f), Tooltip("Where along the lot width a road label sits (0 = left wall, 1 = right wall).")] private float _roadLabelShare = 0.72f;
        [SerializeField] private Color _roadLabelColor = new Color(0.97f, 0.97f, 0.94f, 0.9f);
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
        public Color OutsideColor => _outsideColor;
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
        public string EntryWord => _entryWord;
        public string ExitWord => _exitWord;
        public float RoadLabelPixel => _roadLabelPixel;
        public float RoadLabelShare => _roadLabelShare;
        public Color RoadLabelColor => _roadLabelColor;
        public float SidewalkDepth => _sidewalkDepth;
        public float OutsideReach => _outsideReach;
        public float CurbEdgeWidth => _curbEdgeWidth;
        public Vector2 CurbPlateShare => _curbPlateShare;
        public WalkwayStyle Walkway => _walkway;
    }
}
