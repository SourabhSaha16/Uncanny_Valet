using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>Tunable numbers for the lot layout and the parked/at-curb rule.</summary>
    [CreateAssetMenu(menuName = "Uncanny Valet/Lot Config", fileName = "LotConfig")]
    public class LotConfig : ScriptableObject
    {
        [Header("Spots")]
        [SerializeField] private int _rows = 2;
        [SerializeField] private int _spotsPerRow = 6;
        [SerializeField] private Vector2 _spotSize = new Vector2(2.6f, 4.4f);
        [SerializeField] private float _spotGap = 0.2f;
        [SerializeField] private float _aisleWidth = 5f;

        [Header("Curb")]
        [SerializeField] private Vector2 _curbPosition = new Vector2(-12f, 0f);
        [SerializeField] private Vector2 _curbSize = new Vector2(3f, 6f);
        [SerializeField] private float _curbToSpotsGap = 3f;

        [Header("Parked rule")]
        [SerializeField] private float _settleSeconds = 0.5f;
        [SerializeField] private float _stoppedSpeed = 0.2f;

        [Header("Curb facing")]
        [SerializeField, Range(5f, 90f), Tooltip("How far a car may be turned from pointing straight down the screen and still be handed back at the curb.")]
        private float _curbFacingToleranceDegrees = 35f;

        [Header("Frame")]
        [SerializeField] private float _margin = 1.5f;
        [SerializeField] private float _wallThickness = 0.6f;

        [Header("Start points")]
        [SerializeField] private float _valetStartOffset = 1.5f;

        [Header("Arrival queue")]
        [Min(1), Tooltip("Number of waiting slots; slot 0 is the arrival point.")]
        [SerializeField] private int _queueSlotCount = 3;
        [Tooltip("Footprint of a waiting car (width, length) at rotation 0.")]
        [SerializeField] private Vector2 _queueCarSize = new Vector2(2.2f, 4.5f);
        [Min(0f), Tooltip("Gap between the sides of neighbouring waiting cars.")]
        [SerializeField] private float _queueSlotGap = 0.6f;
        [Tooltip("Direction the row grows from slot 0. Keep it along the cars' width and away from the spots.")]
        [SerializeField] private Vector2 _queueDirection = Vector2.left;
        [Min(0f), Tooltip("Gap between the curb's lower edge and the nose of the car in slot 0.")]
        [SerializeField] private float _queueStartGap = 0.8f;
        [Min(0f), Tooltip("Space kept between the waiting cars or stations and the lot walls.")]
        [SerializeField] private float _queuePadding = 0.5f;
        [Tooltip("Heading of waiting cars; 0 = facing +Y, towards the curb.")]
        [SerializeField] private float _queueRotationDegrees;

        [Header("Stations")]
        [Min(0), Tooltip("Charging spots (ids continue after the regular spots). They sit in a row left to right, followed by the single wash slot.")]
        [SerializeField] private int _chargingBays = 2;
        [Tooltip("Footprint of each station (width, length).")]
        [SerializeField] private Vector2 _stationSize = new Vector2(2.6f, 4.4f);
        [Min(0f), Tooltip("Gap between neighbouring stations.")]
        [SerializeField] private float _stationGap = 0.4f;
        [Tooltip("Centre of the first charging spot. Default: above the curb, clear of the curb zone and the spots.")]
        [SerializeField] private Vector2 _stationsFirstCenter = new Vector2(-17.5f, 6f);
        [Min(0.1f), Tooltip("Largest size of the CHARGE / WASH text; it shrinks to fit the station width.")]
        [SerializeField] private float _stationLabelSize = 0.45f;
        [Min(0.1f), Tooltip("Estimated width of one label character per unit of label size (generous).")]
        [SerializeField] private float _labelCharWidth = 0.9f;
        [Range(0.3f, 1f), Tooltip("Share of the station width the label may use; the rest is margin.")]
        [SerializeField] private float _stationLabelFill = 0.7f;

        [Header("Labels")]
        [Min(0.1f), Tooltip("Size of the vertical CURB text; 4 lines must fit inside the curb height.")]
        [SerializeField] private float _curbLabelSize = 1f;
        [SerializeField] private float _labelSize = 1.6f;
        [SerializeField] private Color _labelColor = new Color(1f, 1f, 1f, 0.55f);

        public float LabelSize => _labelSize;
        public Color LabelColor => _labelColor;
        public float CurbLabelSize => _curbLabelSize;
        public int QueueSlotCount => Mathf.Max(1, _queueSlotCount);
        public int ChargingBays => Mathf.Max(0, _chargingBays);
        public Vector2 StationSize => _stationSize;
        public float StationGap => _stationGap;
        public Vector2 StationsFirstCenter => _stationsFirstCenter;
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

        public int Rows => Mathf.Max(1, _rows);
        public int SpotsPerRow => Mathf.Max(1, _spotsPerRow);
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
