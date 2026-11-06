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
        [Min(0f), Tooltip("Space kept between the waiting cars and the lot walls.")]
        [SerializeField] private float _queuePadding = 0.5f;
        [Tooltip("Heading of waiting cars; 0 = facing +Y, towards the curb.")]
        [SerializeField] private float _queueRotationDegrees;

        [Header("Labels")]
        [Min(0.1f), Tooltip("Size of the vertical CURB text; 4 lines must fit inside the curb height.")]
        [SerializeField] private float _curbLabelSize = 1f;
        [SerializeField] private float _labelSize = 1.6f;
        [SerializeField] private Color _labelColor = new Color(1f, 1f, 1f, 0.55f);

        public float LabelSize => _labelSize;
        public Color LabelColor => _labelColor;
        public float CurbLabelSize => _curbLabelSize;
        public int QueueSlotCount => Mathf.Max(1, _queueSlotCount);
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
        public float SettleSeconds => _settleSeconds;
        public float StoppedSpeed => _stoppedSpeed;
        public float Margin => _margin;
        public float WallThickness => _wallThickness;
    }
}
