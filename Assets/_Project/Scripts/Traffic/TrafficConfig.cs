using UnityEngine;

namespace UncannyValet.Traffic
{
    /// <summary>Every tunable of the road traffic: looks, speeds, gaps, merge rules and how many ambient cars are kept on screen. Read-only at runtime.</summary>
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

        [Tooltip("A patron car turning off the lane stays visible to traffic on it until its centre is this far from the lane line.")]
        [SerializeField, Range(0.5f, 4f)] private float _laneClearance = 1.6f;

        [Header("Ambient traffic (all four lanes: both roads, both directions)")]
        [Tooltip("Fewest ambient cars kept on screen. Below this a car is sent in at once from the screen edge (the F2 panel can change it live).")]
        [SerializeField, Range(0, 8)] private int _minOnScreen = 3;
        [Tooltip("Ambient cars on screen that spawning aims for: between the minimum and this, a car follows after a random wait (the F2 panel can change it live). 0 switches ambient traffic off.")]
        [SerializeField, Range(0, 10)] private int _targetOnScreen = 4;
        [Tooltip("Shortest and longest wait in seconds before the next car while fewer than the target are on screen.")]
        [SerializeField] private Vector2 _spawnGapSeconds = new Vector2(1.5f, 4f);
        [Tooltip("A car counts as on screen while its centre is within this many units of the visible area (a little under half a car length, so a car with its nose still showing counts).")]
        [SerializeField, Min(0f)] private float _countMargin = 1f;
        [Tooltip("A car this close to the edge it drives out at no longer counts toward the minimum, so its replacement is sent in a moment before it goes (the minimum then never dips).")]
        [SerializeField, Min(0f)] private float _leaveLead = 6f;
        [Tooltip("New cars appear this many units beyond the screen edge they drive in from: just over half a car length, so they never pop in, and close enough that a replacement shows up at once.")]
        [SerializeField, Min(0.5f)] private float _spawnMargin = 1.4f;
        [Tooltip("A car is removed once it is this many units beyond the screen edge it drives out at (just over half a car length).")]
        [SerializeField, Min(0.5f)] private float _despawnMargin = 1.4f;
        [Tooltip("How far before the merge point a delivered car asks traffic on the exit road to start slowing for it (units of its route).")]
        [SerializeField, Min(0f)] private float _yieldLead = 10f;
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
        public float LaneClearance => _laneClearance;
        public int MinOnScreen => _minOnScreen;
        public int TargetOnScreen => _targetOnScreen;
        public Vector2 SpawnGapSeconds => _spawnGapSeconds;
        public float CountMargin => _countMargin;
        public float LeaveLead => _leaveLead;
        public float SpawnMargin => _spawnMargin;
        public float DespawnMargin => _despawnMargin;
        public float YieldLead => _yieldLead;
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
            if (_spawnGapSeconds.y < _spawnGapSeconds.x) _spawnGapSeconds.y = _spawnGapSeconds.x;
            if (_targetOnScreen < _minOnScreen) _targetOnScreen = _minOnScreen;
        }
    }
}
