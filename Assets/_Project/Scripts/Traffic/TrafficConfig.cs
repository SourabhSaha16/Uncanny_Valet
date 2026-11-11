using UnityEngine;

namespace UncannyValet.Traffic
{
    /// <summary>Every tunable of the road traffic: looks, speeds, gaps, merge rules and the ambient rate. Read-only at runtime.</summary>
    [CreateAssetMenu(menuName = "Uncanny Valet/Traffic Config", fileName = "TrafficConfig")]
    public sealed class TrafficConfig : ScriptableObject
    {
        [Header("Look")]
        [Tooltip("Car sprites drawn nose up. Ambient cars pick one at random.")]
        [SerializeField] private Sprite[] _sprites;
        [Tooltip("Tints combined with the sprites. Leave one white entry for no tint.")]
        [SerializeField] private Color[] _tints = { Color.white };
        [Tooltip("Uniform scale of the sprite objects.")]
        [SerializeField, Min(0.01f)] private float _carScale = 1f;
        [Tooltip("Sorting order of traffic cars; below the lot cars and above the road.")]
        [SerializeField] private int _sortingOrder = 1;
        [Tooltip("Bumper to bumper length used for gaps. Match the lot cars so patron cars and traffic agree.")]
        [SerializeField, Min(0.1f)] private float _carLength = 2.2f;

        [Header("Speeds (units per second)")]
        [Tooltip("Cruise speed on the road.")]
        [SerializeField, Min(0.1f)] private float _cruiseSpeed = 5f;
        [Tooltip("Speed a patron car slows to before turning up into the queue, and drives the turn at.")]
        [SerializeField, Min(0.1f)] private float _turnSpeed = 2f;
        [Tooltip("Speed along a curb exit route.")]
        [SerializeField, Min(0.1f)] private float _routeSpeed = 2.5f;
        [Tooltip("Starting multiplier on ambient car speed (the F2 panel can change it live).")]
        [SerializeField, Min(0.05f)] private float _speedScale = 1.25f;
        [Tooltip("Starting speed of patron cars driving in, in units per second (the F2 panel can change it live). The cruise speed above is its scale-1 reference.")]
        [SerializeField, Min(0.1f)] private float _entrySpeed = 7.3f;
        [Tooltip("Starting speed of delivered cars driving off, in units per second (the F2 panel can change it live). The route speed above is its scale-1 reference.")]
        [SerializeField, Min(0.1f)] private float _exitSpeed = 5f;
        [Tooltip("Lowest and highest multiplier on cruise speed for ambient cars.")]
        [SerializeField] private Vector2 _speedScaleRange = new Vector2(0.85f, 1.15f);
        [Tooltip("Speed gained per second.")]
        [SerializeField, Min(0.1f)] private float _acceleration = 3f;
        [Tooltip("Speed lost per second when braking.")]
        [SerializeField, Min(0.1f)] private float _brake = 6f;

        [Header("Gaps (units)")]
        [Tooltip("Bumper gap kept when standing behind another car.")]
        [SerializeField, Min(0.05f)] private float _minGap = 0.6f;
        [Tooltip("Seconds of travel added to the gap per unit of speed.")]
        [SerializeField, Min(0.05f)] private float _headway = 0.8f;
        [Tooltip("Gap needed ahead of a car before it may join a lane.")]
        [SerializeField, Min(0f)] private float _mergeGapAhead = 2.5f;
        [Tooltip("Gap needed behind a car before it may join a lane (grows with the follower's speed).")]
        [SerializeField, Min(0f)] private float _mergeGapBehind = 2.5f;
        [Tooltip("How far before the merge point a leaving car starts yielding to traffic.")]
        [SerializeField, Min(0f)] private float _yieldDistance = 2f;
        [Tooltip("A car counts as arrived this close to the end of its route.")]
        [SerializeField, Min(0.001f)] private float _arriveTolerance = 0.02f;

        [Header("Obstacles")]
        [Tooltip("Seconds a patron car waits behind something solid (the player's car, the valet, a parked car) before it creeps on.")]
        [SerializeField, Range(1f, 30f)] private float _blockedTimeout = 6f;
        [Tooltip("Speed of that creep, in units per second.")]
        [SerializeField, Range(0.1f, 3f)] private float _creepSpeed = 0.8f;

        [Header("Ambient traffic")]
        [Tooltip("Ambient cars per minute over both lanes. 0 switches ambient traffic off.")]
        [SerializeField, Min(0f)] private float _ambientPerMinute = 13f;
        [Tooltip("0 = exact interval, 1 = anywhere from nothing to double the mean.")]
        [SerializeField, Range(0f, 1f)] private float _intervalSpread = 0.6f;
        [Tooltip("Chance that an ambient car takes the east lane (the lane next to the lot).")]
        [SerializeField, Range(0f, 1f)] private float _eastShare = 0.5f;
        [Tooltip("Random seed for ambient traffic.")]
        [SerializeField] private int _seed = 1234;
        [Tooltip("Most traffic cars alive at once (ambient and patron trips). Also the pool size.")]
        [SerializeField, Min(2)] private int _maxCars = 16;

        public int LookCount => (_sprites == null ? 0 : _sprites.Length) * Mathf.Max(1, _tints?.Length ?? 0);
        public float CarScale => _carScale;
        public int SortingOrder => _sortingOrder;
        public float CarLength => _carLength;
        public float CruiseSpeed => _cruiseSpeed;
        public float TurnSpeed => _turnSpeed;
        public float RouteSpeed => _routeSpeed;
        public float SpeedScale => _speedScale;
        public float EntrySpeed => _entrySpeed;
        public float ExitSpeed => _exitSpeed;
        public Vector2 SpeedScaleRange => _speedScaleRange;
        public float Acceleration => _acceleration;
        public float Brake => _brake;
        public float MinGap => _minGap;
        public float Headway => _headway;
        public float MergeGapAhead => _mergeGapAhead;
        public float MergeGapBehind => _mergeGapBehind;
        public float YieldDistance => _yieldDistance;
        public float ArriveTolerance => _arriveTolerance;
        public float BlockedTimeout => _blockedTimeout;
        public float CreepSpeed => _creepSpeed;
        public float AmbientPerMinute => _ambientPerMinute;
        public float IntervalSpread => _intervalSpread;
        public float EastShare => _eastShare;
        public int Seed => _seed;
        public int MaxCars => _maxCars;

        /// <summary>The sprite for a look index.</summary>
        public Sprite SpriteFor(int look) => _sprites[look % _sprites.Length];

        /// <summary>The tint for a look index.</summary>
        public Color TintFor(int look) => _tints.Length == 0 ? Color.white : _tints[look / _sprites.Length % _tints.Length];

        private void OnValidate()
        {
            if (_sprites == null || _sprites.Length == 0) Debug.LogError($"{name}: TrafficConfig has no car sprites.", this);
            if (_speedScaleRange.y < _speedScaleRange.x) _speedScaleRange.y = _speedScaleRange.x;
        }
    }
}
