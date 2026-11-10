using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UnityEngine;

namespace UncannyValet.Traffic
{
    /// <summary>
    /// Runs the road traffic: spawns and steps ambient cars on both lanes with pooled visual-only cars, and provides the
    /// patron entry/exit trips. Created by Bootstrap; reads <see cref="IRoadLayout"/> from Services in Start and
    /// registers <see cref="IEntryRouter"/>, <see cref="IExitRouter"/> and <see cref="ITripDriver"/> there.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TrafficModule : MonoBehaviour
    {
        private const float NoSpeed = -1f;

        [SerializeField] private TrafficConfig _config;

        private readonly LanePath _east = new LanePath();
        private readonly LanePath _west = new LanePath();
        private AmbientVehicle[] _vehicles;
        private LaneOccupancy _occupancy;
        private AmbientSpawner _spawner;
        private TrafficCarPool _pool;
        private FollowSettings _follow;
        private MergeGapRule _merge;
        private float _ambientRate;
        private float _speedScale = 1f;
        private TripSpeeds _speeds;
        private float _pendingEntrySpeed = NoSpeed;
        private float _pendingExitSpeed = NoSpeed;
        private bool _ready;

        /// <summary>Ambient cars per minute; 0 switches ambient traffic off. Starts at the config value.</summary>
        public void SetAmbientRate(float carsPerMinute) => _ambientRate = Mathf.Max(0f, carsPerMinute);

        /// <summary>Multiplier on the speed of ambient cars (a live tuning value).</summary>
        public void SetSpeedScale(float scale) => _speedScale = Mathf.Max(0f, scale);

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
            Build(road);
            _ready = true;
        }

        private void Build(IRoadLayout road)
        {
            _east.Rebuild(road.EastLane);
            _west.Rebuild(road.WestLane);
            _follow = new FollowSettings(_config.CruiseSpeed, _config.MinGap, _config.Headway, _config.Acceleration, _config.Brake);
            _merge = new MergeGapRule(_config.MergeGapAhead, _config.MergeGapBehind, _config.Headway);
            _occupancy = new LaneOccupancy(_config.MaxCars * 3);
            _pool = new TrafficCarPool(transform, _config.MaxCars);
            _vehicles = new AmbientVehicle[_config.MaxCars];
            for (int i = 0; i < _vehicles.Length; i++) _vehicles[i] = new AmbientVehicle();
            Vector2 scale = _config.SpeedScaleRange;
            _spawner = new AmbientSpawner(_config.Seed, _config.LookCount, _config.IntervalSpread, _config.EastShare, scale.x, scale.y);
            _ambientRate = _config.AmbientPerMinute;
            _speedScale = _config.SpeedScale;
            RegisterTrips(road);
        }

        private void RegisterTrips(IRoadLayout road)
        {
            var tuning = new TripTuning(_config.CarLength, _config.TurnSpeed, _config.RouteSpeed, _config.YieldDistance, _config.ArriveTolerance);
            _speeds = new TripSpeeds(_config.CruiseSpeed, _config.RouteSpeed);
            if (_pendingEntrySpeed > NoSpeed) _speeds.SetEntrySpeed(_pendingEntrySpeed);
            if (_pendingExitSpeed > NoSpeed) _speeds.SetExitSpeed(_pendingExitSpeed);
            var router = new TripRouter(new TripContext(road, _east, _occupancy, _follow, _merge, tuning, _speeds), _config.MaxCars);
            Services.Register<IEntryRouter>(router);
            Services.Register<IExitRouter>(router);
            Services.Register<ITripDriver>(router);
        }

        private void Update()
        {
            if (!_ready) return;
            float dt = Time.deltaTime;
            if (_spawner.Tick(dt, _ambientRate, out SpawnRequest request)) TrySpawn(request);
            for (int i = 0; i < _vehicles.Length; i++)
            {
                if (_vehicles[i].Active) StepVehicle(_vehicles[i], dt);
            }
        }

        private void TrySpawn(SpawnRequest request)
        {
            float length = _config.CarLength;
            float ahead = _occupancy.GapAhead(request.Lane, 0f, length, 0, out _);
            if (!_merge.CanMerge(ahead, float.PositiveInfinity, 0f)) return;
            AmbientVehicle vehicle = FreeVehicle();
            TrafficCar view = _pool.Rent();
            if (vehicle == null || view == null)
            {
                if (view != null) _pool.Return(view);
                return;
            }
            float desired = _config.CruiseSpeed * request.SpeedScale;
            int id = _occupancy.Add(request.Lane, 0f, desired, length);
            vehicle.State = new LaneState { Id = id, Lane = request.Lane, Distance = 0f, Speed = desired };
            vehicle.DesiredSpeed = desired;
            vehicle.View = view;
            vehicle.Active = true;
            view.Show(_config.SpriteFor(request.LookIndex), _config.TintFor(request.LookIndex), _config.SortingOrder, _config.CarScale);
        }

        private AmbientVehicle FreeVehicle()
        {
            for (int i = 0; i < _vehicles.Length; i++)
            {
                if (!_vehicles[i].Active) return _vehicles[i];
            }
            return null;
        }

        private void StepVehicle(AmbientVehicle vehicle, float dt)
        {
            LanePath path = vehicle.State.Lane == TrafficLane.East ? _east : _west;
            LaneMotion.Step(ref vehicle.State, _config.CarLength, vehicle.DesiredSpeed * _speedScale,
                float.PositiveInfinity, _occupancy, _follow, dt);
            float distance = vehicle.State.Distance;
            if (distance >= path.Length)
            {
                Release(vehicle);
                return;
            }
            vehicle.View.Place(path.PositionAt(distance), path.HeadingAt(distance));
        }

        private void Release(AmbientVehicle vehicle)
        {
            _occupancy.Remove(vehicle.State.Id);
            _pool.Return(vehicle.View);
            vehicle.View = null;
            vehicle.Active = false;
        }
    }
}
