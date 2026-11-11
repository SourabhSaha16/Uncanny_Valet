using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UnityEngine;

namespace UncannyValet.Traffic
{
    /// <summary>
    /// Runs the road traffic: keeps ambient cars driving both ways on both roads with pooled visual-only cars, and provides the
    /// patron entry/exit trips. Created by Bootstrap; reads <see cref="IRoadLayout"/> from Services in Start and
    /// registers <see cref="IEntryRouter"/>, <see cref="IExitRouter"/> and <see cref="ITripDriver"/> there.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TrafficModule : MonoBehaviour
    {
        private const float NoSpeed = -1f;
        private const int NoCount = -1;

        [SerializeField] private TrafficConfig _config;

        private readonly LaneNetwork _lanes = new LaneNetwork();
        private AmbientTraffic _ambient;
        private AmbientDensity _density;
        private Camera _camera;
        private float _speedScale = 1f;
        private bool _speedScaleSet;
        private TripSpeeds _speeds;
        private float _pendingEntrySpeed = NoSpeed;
        private float _pendingExitSpeed = NoSpeed;
        private int _pendingMin = NoCount;
        private int _pendingTarget = NoCount;
        private readonly WorldTimeReader _world = new WorldTimeReader();
        private bool _ready;

        /// <summary>Fewest ambient cars kept on screen; below it a car is sent in at once. A live tuning value.</summary>
        public void SetMinOnScreen(float count)
        {
            if (_density != null) _density.SetMinOnScreen(Mathf.RoundToInt(count));
            else _pendingMin = Mathf.RoundToInt(count);
        }

        /// <summary>Ambient cars on screen that spawning aims for (0 switches ambient traffic off). A live tuning value.</summary>
        public void SetTargetOnScreen(float count)
        {
            if (_density != null) _density.SetTargetOnScreen(Mathf.RoundToInt(count));
            else _pendingTarget = Mathf.RoundToInt(count);
        }

        /// <summary>Multiplier on the speed of ambient cars (a live tuning value).</summary>
        public void SetSpeedScale(float scale)
        {
            _speedScale = Mathf.Max(0f, scale);
            _speedScaleSet = true;
            _ambient?.SetSpeedScale(_speedScale);
        }

        /// <summary>Speed of patron cars driving in: lane cruise in units per second (the turn into the queue scales with it). A live tuning value.</summary>
        public void SetEntrySpeed(float speed)
        {
            if (_speeds != null) _speeds.SetEntrySpeed(speed);
            else _pendingEntrySpeed = speed;
        }

        /// <summary>Speed of delivered cars driving off: the curb route in units per second (the lane after merging scales with it). A live tuning value.</summary>
        public void SetExitSpeed(float speed)
        {
            if (_speeds != null) _speeds.SetExitSpeed(speed);
            else _pendingExitSpeed = speed;
        }

        private void Start()
        {
            if (_config == null || _config.LookCount == 0)
            {
                Debug.LogError($"{name}: TrafficModule needs a TrafficConfig with at least one sprite.", this);
                enabled = false;
                return;
            }
            if (!Services.TryGet<IRoadLayout>(out var road))
            {
                Debug.LogError($"{name}: TrafficModule found no IRoadLayout in Services.", this);
                enabled = false;
                return;
            }
            _camera = Camera.main;
            if (_camera == null) Debug.LogError($"{name}: TrafficModule found no camera tagged MainCamera; ambient cars need it to keep cars on screen.", this);
            Build(road);
            _ready = true;
        }

        private void Build(IRoadLayout road)
        {
            _lanes.Rebuild(road);
            var follow = new FollowSettings(_config.CruiseSpeed, _config.MinGap, _config.Headway, _config.Acceleration, _config.Brake);
            var merge = new MergeGapRule(_config.MergeGapAhead, _config.MergeGapBehind, _config.Headway);
            var occupancy = new LaneOccupancy(_config.MaxCars * 3);
            var yield = new YieldZone(_config.MergeGapBehind);
            _density = new AmbientDensity(_config.Seed, _config.LookCount, _config.SpeedScaleRange, _config.SpawnGapSeconds,
                _pendingMin != NoCount ? _pendingMin : _config.MinOnScreen, _pendingTarget != NoCount ? _pendingTarget : _config.TargetOnScreen);
            _ambient = new AmbientTraffic(_config, _lanes, occupancy, new TrafficCarPool(transform, _config.MaxCars), follow, merge, yield, _density);
            if (_speedScaleSet) _ambient.SetSpeedScale(_speedScale);
            RegisterTrips(road, occupancy, follow, merge, yield);
        }

        private void RegisterTrips(IRoadLayout road, LaneOccupancy occupancy, FollowSettings follow, MergeGapRule merge, YieldZone yield)
        {
            var tuning = new TripTuning(_config.CarLength, _config.TurnSpeed, _config.RouteSpeed, _config.YieldDistance, _config.ArriveTolerance,
                TripTuning.DefaultRetargetSeconds, _config.BlockedTimeout, _config.CreepSpeed, _config.LaneClearance, _config.YieldLead);
            _speeds = new TripSpeeds(_config.CruiseSpeed, _config.RouteSpeed);
            _speeds.SetEntrySpeed(_config.EntrySpeed);
            _speeds.SetExitSpeed(_config.ExitSpeed);
            if (_pendingEntrySpeed > NoSpeed) _speeds.SetEntrySpeed(_pendingEntrySpeed);
            if (_pendingExitSpeed > NoSpeed) _speeds.SetExitSpeed(_pendingExitSpeed);
            var context = new TripContext(road, _lanes.Path(TrafficLane.East), _lanes.Path(TrafficLane.Top), occupancy, follow, merge, tuning, _speeds, yield);
            var router = new TripRouter(context, _config.MaxCars);
            Services.Register<IEntryRouter>(router);
            Services.Register<IExitRouter>(router);
            Services.Register<ITripDriver>(router);
            Services.Register<ITripObstacles>(router);
        }

        private void Update()
        {
            if (!_ready || _camera == null) return;
            Vector3 at = _camera.transform.position;
            _ambient.Step(_world.Scaled(Time.deltaTime), ViewWindow.Of(new Vector2(at.x, at.y), _camera.orthographicSize, _camera.aspect));
        }
    }
}
