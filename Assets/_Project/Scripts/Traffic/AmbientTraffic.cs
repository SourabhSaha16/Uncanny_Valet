using System.Collections.Generic;
using UnityEngine;

namespace UncannyValet.Traffic
{
    /// <summary>
    /// The ambient cars of the four lanes. Each frame it moves them (car following, yielding to a merging car), counts how many
    /// are on screen, and lets <see cref="AmbientDensity"/> spawn the next one just beyond the screen edge it drives in from.
    /// Cars are removed once they have left the screen. A null pool runs the same rules without visuals (tests).
    /// </summary>
    public sealed class AmbientTraffic
    {
        private const int WarmUpAttemptsPerCar = 3;

        private readonly TrafficConfig _config;
        private readonly LaneNetwork _lanes;
        private readonly LaneOccupancy _occupancy;
        private readonly TrafficCarPool _pool;
        private readonly FollowSettings _follow;
        private readonly MergeGapRule _merge;
        private readonly YieldZone _yield;
        private readonly AmbientDensity _density;
        private readonly AmbientVehicle[] _vehicles;
        private readonly float[] _laneY = new float[TrafficLanes.Count];
        private float _speedScale;
        private bool _warm;

        public AmbientTraffic(TrafficConfig config, LaneNetwork lanes, LaneOccupancy occupancy, TrafficCarPool pool,
            in FollowSettings follow, MergeGapRule merge, YieldZone yield, AmbientDensity density)
        {
            _config = config;
            _lanes = lanes;
            _occupancy = occupancy;
            _pool = pool;
            _follow = follow;
            _merge = merge;
            _yield = yield;
            _density = density;
            _speedScale = config.SpeedScale;
            _vehicles = new AmbientVehicle[config.MaxCars];
            for (int i = 0; i < _vehicles.Length; i++) _vehicles[i] = new AmbientVehicle();
            for (int i = 0; i < _laneY.Length; i++) _laneY[i] = lanes.Path((TrafficLane)i).PositionAt(0f).y;
        }

        /// <summary>Ambient cars on screen (or within the count margin) after the last <see cref="Step"/>.</summary>
        public int OnScreen { get; private set; }

        /// <summary>Every pooled ambient car; the ones with <see cref="AmbientVehicle.Active"/> are on the road.</summary>
        public IReadOnlyList<AmbientVehicle> Vehicles => _vehicles;

        /// <summary>Ambient cars alive.</summary>
        public int Alive
        {
            get
            {
                int count = 0;
                for (int i = 0; i < _vehicles.Length; i++) count += _vehicles[i].Active ? 1 : 0;
                return count;
            }
        }

        /// <summary>Multiplier on the speed of ambient cars (a live tuning value).</summary>
        public void SetSpeedScale(float scale) => _speedScale = Mathf.Max(0f, scale);

        /// <summary>Moves every car, removes those that left the screen, and spawns at most one car.</summary>
        public void Step(float dt, in ViewWindow view)
        {
            if (!_warm)
            {
                WarmUp(view);
                _warm = true;
            }
            for (int i = 0; i < _vehicles.Length; i++)
            {
                if (_vehicles[i].Active) StepVehicle(_vehicles[i], dt, view);
            }
            int approaching = Count(view, out int leaving);
            int counted = OnScreen + approaching;
            if (_density.Tick(dt, counted, counted - leaving, OpenLanes(view), out SpawnRequest request))
                Spawn(request, EntryDistance(request.Lane, view));
        }

        private void StepVehicle(AmbientVehicle v, float dt, in ViewWindow view)
        {
            LanePath path = _lanes.Path(v.State.Lane);
            LaneMotion.Step(ref v.State, _config.CarLength, v.DesiredSpeed * _speedScale, float.PositiveInfinity, _occupancy, _follow, dt, _yield);
            float distance = v.State.Distance;
            v.Position = path.PositionAt(distance);
            if (distance >= path.Length || view.HasLeft(v.Position.x, TrafficLanes.IsLeftward(v.State.Lane), _config.DespawnMargin))
            {
                Release(v);
                return;
            }
            v.View?.Place(v.Position, path.HeadingAt(distance));
        }

        // Fills OnScreen; returns how many cars are on the way in to a lane that the screen shows, and how many on screen are about to leave.
        private int Count(in ViewWindow view, out int leaving)
        {
            OnScreen = 0;
            leaving = 0;
            int approaching = 0;
            for (int i = 0; i < _vehicles.Length; i++)
            {
                AmbientVehicle v = _vehicles[i];
                if (!v.Active) continue;
                if (view.Contains(v.Position, _config.CountMargin))
                {
                    OnScreen++;
                    if (view.HasLeft(v.Position.x, TrafficLanes.IsLeftward(v.State.Lane), -_config.LeaveLead)) leaving++;
                }
                else if (IsApproaching(v, view)) approaching++;
            }
            return approaching;
        }

        private bool IsApproaching(AmbientVehicle v, in ViewWindow view) =>
            view.CoversHeight(_laneY[(int)v.State.Lane], _config.CountMargin) && view.IsUpstream(v.Position.x, TrafficLanes.IsLeftward(v.State.Lane));

        private LaneSet OpenLanes(in ViewWindow view)
        {
            LaneSet open = LaneSet.None;
            for (int i = 0; i < TrafficLanes.Count; i++)
            {
                var lane = (TrafficLane)i;
                if (!view.CoversHeight(_laneY[i], _config.CountMargin)) continue;
                if (SpawnGate.IsOpen(_occupancy, _merge, lane, EntryDistance(lane, view), _config.CarLength)) open |= TrafficLanes.ToSet(lane);
            }
            return open;
        }

        private float EntryDistance(TrafficLane lane, in ViewWindow view)
        {
            float x = view.EntryX(TrafficLanes.IsLeftward(lane), _config.SpawnMargin);
            return _lanes.Path(lane).Project(new Vector2(x, _laneY[(int)lane]), out _);
        }

        // The first frame: cars already driving on screen, spread along the visible lanes, instead of an empty road.
        private void WarmUp(in ViewWindow view)
        {
            int wanted = _density.Target;
            for (int attempt = 0; attempt < wanted * WarmUpAttemptsPerCar && Alive < wanted; attempt++)
            {
                LaneSet visible = LaneSet.None;
                for (int i = 0; i < TrafficLanes.Count; i++)
                    if (view.CoversHeight(_laneY[i], _config.CountMargin)) visible |= TrafficLanes.ToSet((TrafficLane)i);
                if (!_density.TryRoll(visible, out SpawnRequest request)) return;
                float x = Mathf.Lerp(view.Bounds.xMin, view.Bounds.xMax, _density.NextUnit());
                float distance = _lanes.Path(request.Lane).Project(new Vector2(x, _laneY[(int)request.Lane]), out _);
                if (SpawnGate.IsOpen(_occupancy, _merge, request.Lane, distance, _config.CarLength)) Spawn(request, distance);
            }
        }

        private void Spawn(in SpawnRequest request, float distance)
        {
            AmbientVehicle v = FreeVehicle();
            if (v == null) return;
            TrafficCar view = null;
            if (_pool != null)
            {
                view = _pool.Rent();
                if (view == null) return;
                view.Show(_config.SpriteFor(request.LookIndex), _config.TintFor(request.LookIndex), _config.SortingOrder, _config.CarScale);
            }
            float desired = _config.CruiseSpeed * request.SpeedScale;
            int id = _occupancy.Add(request.Lane, distance, desired * _speedScale, _config.CarLength);
            v.State = new LaneState { Id = id, Lane = request.Lane, Distance = distance, Speed = desired * _speedScale };
            v.DesiredSpeed = desired;
            v.Position = _lanes.Path(request.Lane).PositionAt(distance);
            v.View = view;
            v.Active = true;
            view?.Place(v.Position, _lanes.Path(request.Lane).HeadingAt(distance));
        }

        private AmbientVehicle FreeVehicle()
        {
            for (int i = 0; i < _vehicles.Length; i++)
            {
                if (!_vehicles[i].Active) return _vehicles[i];
            }
            return null;
        }

        private void Release(AmbientVehicle v)
        {
            _occupancy.Remove(v.State.Id);
            if (v.View != null) _pool.Return(v.View);
            v.View = null;
            v.Active = false;
        }
    }
}
