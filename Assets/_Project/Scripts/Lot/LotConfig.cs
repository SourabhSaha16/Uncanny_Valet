using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>Tunable numbers for the lot layout and the parked/at-curb rule.</summary>
    [CreateAssetMenu(menuName = "Uncanny Valet/Lot Config", fileName = "LotConfig")]
    public class LotConfig : ScriptableObject
    {
        [Header("Spots")]
        [SerializeField, Tooltip("Size of one tall bay (width, length). The bay grid itself is fixed by BayPlan: two rows of four.")]
        private Vector2 _spotSize = new Vector2(2.6f, 4.4f);
        [SerializeField, Tooltip("Gap between neighbouring bays in a row.")]
        private float _spotGap = 0.6f;
        [SerializeField, Tooltip("Height of the drive aisle between the two bay rows.")]
        private float _aisleWidth = 4.6f;

        [Header("Curb")]
        [SerializeField, Tooltip("Centre of the curb zone; it sits right of the diner's main door with an open gap between (SiteLayout.CurbGap). The top bay row is aligned with the curb's top edge.")]
        private Vector2 _curbPosition = new Vector2(-8.25f, 0f);
        [SerializeField] private Vector2 _curbSize = new Vector2(3f, 5f);
        [SerializeField, Tooltip("Width of the open corridor between the curb and the first bay column (queue exit, crosswalk, car exit).")]
        private float _curbToSpotsGap = 5.25f;

        [Header("Parked rule")]
        [SerializeField] private float _settleSeconds = 0.5f;
        [SerializeField] private float _stoppedSpeed = 0.2f;

        [Header("Curb facing")]
        [SerializeField, Range(5f, 90f), Tooltip("Largest angle from pointing straight down the screen at which the patron still takes the car (80 = any car pointing towards the road).")]
        private float _curbFacingToleranceDegrees = 80f;

        [Header("Frame")]
        [SerializeField] private float _margin = 1.5f;
        [SerializeField] private float _wallThickness = 0.6f;

        [Header("Site")]
        [SerializeField] private SiteLayout _site = new SiteLayout();

        [Header("Start points")]
        [SerializeField] private float _valetStartOffset = 1.5f;

        [Header("Arrival queue")]
        [Range(1, 8), Tooltip("Number of waiting slots stacked in a column under the curb; slot 0 is the arrival point, nearest the curb. The queue cap is 3.")]
        [SerializeField] private int _queueSlotCount = 3;
        [Tooltip("Footprint of a waiting car (width, length) at rotation 0.")]
        [SerializeField] private Vector2 _queueCarSize = new Vector2(2.2f, 4.5f);
        [Min(0f), Tooltip("Gap between the sides of neighbouring waiting cars.")]
        [SerializeField] private float _queueSlotGap = 0.15f;
        [Tooltip("Direction the column grows from slot 0, away from the curb (down the screen).")]
        [SerializeField] private Vector2 _queueDirection = Vector2.down;
        [Min(0f), Tooltip("Gap between the curb's lower edge and the nose of the car in slot 0.")]
        [SerializeField] private float _queueStartGap = 0.8f;
        [Min(0f), Tooltip("Space kept between the waiting cars and the lot walls.")]
        [SerializeField] private float _queuePadding = 0.5f;
        [Tooltip("Heading of waiting cars; 0 = facing +Y, towards the curb.")]
        [SerializeField] private float _queueRotationDegrees;

        [Header("Stations")]
        [Min(0.1f), Tooltip("Largest size of the CHARGE / WASH text; it shrinks to fit the station width.")]
        [SerializeField] private float _stationLabelSize = 0.45f;
        [Min(0.1f), Tooltip("Estimated width of one label character per unit of label size (generous).")]
        [SerializeField] private float _labelCharWidth = 0.9f;
        [Range(0.3f, 1f), Tooltip("Share of the station width the label may use; the rest is margin.")]
        [SerializeField] private float _stationLabelFill = 0.7f;

        [Header("Single-occupancy stations")]
        [SerializeField, Min(0.05f), Tooltip("Least time between two OCCUPIED refusals while a driven car keeps pushing into the taken wash slot.")]
        private float _refusalRepeatSeconds = 0.25f;

        [Header("Labels")]
        [Min(0.1f), Tooltip("Size of the vertical curb text; the longest word's letters must fit inside the curb height (about 1.15 lines per size).")]
        [SerializeField] private float _curbLabelSize = 0.65f;
        [Tooltip("Words written vertically (one column each, left to right) on the curb zone.")]
        [SerializeField] private string[] _curbLabelWords = { "VALET", "PICKUP" };
        [Min(0.1f), Tooltip("Distance between the columns of the curb text.")]
        [SerializeField] private float _curbLabelColumnSpacing = 0.95f;
        [SerializeField] private float _labelSize = 1.6f;
        [SerializeField] private Color _labelColor = new Color(1f, 1f, 1f, 0.55f);

        public float RefusalRepeatSeconds => _refusalRepeatSeconds;
        public float LabelSize => _labelSize;
        public Color LabelColor => _labelColor;
        public float CurbLabelSize => _curbLabelSize;
        public string[] CurbLabelWords => _curbLabelWords;
        public float CurbLabelColumnSpacing => _curbLabelColumnSpacing;
        public int QueueSlotCount => Mathf.Max(1, _queueSlotCount);
        public SiteLayout Site => _site;
        public float StationLabelSize => _stationLabelSize;
        public float LabelCharWidth => _labelCharWidth;
        public float StationLabelFill => _stationLabelFill;
        public Vector2 QueueCarSize => _queueCarSize;
        public float QueueSlotGap => _queueSlotGap;
        public Vector2 QueueDirection => _queueDirection;
        public float QueueStartGap => _queueStartGap;
        public float QueuePadding => _queuePadding;
        public float QueueRotationDegrees => _queueRotationDegrees;
        public float ValetStartOffset => _valetStartOffset;

        public Vector2 SpotSize => _spotSize;
        public float SpotGap => _spotGap;
        public float AisleWidth => _aisleWidth;
        public Vector2 CurbPosition => _curbPosition;
        public Vector2 CurbSize => _curbSize;
        public float CurbToSpotsGap => _curbToSpotsGap;
        public float CurbFacingToleranceDegrees => _curbFacingToleranceDegrees;
        public float SettleSeconds => _settleSeconds;
        public float StoppedSpeed => _stoppedSpeed;
        public float Margin => _margin;
        public float WallThickness => _wallThickness;
    }
}
