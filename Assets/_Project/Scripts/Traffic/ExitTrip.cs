using UncannyValet.Core.Contracts;
using UnityEngine;

namespace UncannyValet.Traffic
{
    /// <summary>
    /// A delivered car leaving: drives the curb exit route, asks ambient traffic on the exit road above the lot to slow, yields at the merge point until there is a gap,
    /// merges, and drives to the right end of the top lane where the trip finishes.
    /// </summary>
    public sealed class ExitTrip : ITrip
    {
        private readonly TripContext _ctx;
        private readonly RouteRunner _route = new RouteRunner();
        private readonly TripObstacle _obstacle = new TripObstacle();
        private LaneState _lane;
        private TripPhase _phase;

        public ExitTrip(TripContext context)
        {
            _ctx = context;
        }

        /// <inheritdoc />
        public int Id { get; private set; }

        /// <summary>Starts the trip under trip id <paramref name="id"/>.</summary>
        public void Begin(int id)
        {
            Id = id;
            _ctx.Road.CurbExitRoute(_ctx.Scratch);
            _route.Begin(_ctx.Scratch);
            _lane = default;
            _obstacle.Reset();
            _phase = TripPhase.OnRoute;
        }

        /// <inheritdoc />
        public void Step(float dt, out TripPose pose)
        {
            pose = _phase == TripPhase.OnLane ? StepLane(dt) : StepRoute(dt);
            if (pose.Phase == TripPhase.Finished) Cancel();
        }

        /// <inheritdoc />
        public void SetObstacleGap(float gap) => _obstacle.Report(gap);

        private float ObstacleCap(float dt) => _obstacle.SpeedCap(_ctx.Follow, _ctx.Tuning.CreepSpeed, _ctx.Tuning.BlockedTimeout, dt);

        /// <inheritdoc />
        public void Cancel()
        {
            if (_lane.Id != 0) _ctx.Occupancy.Remove(_lane.Id);
            _ctx.Yield.Release(Id);
            _lane = default;
            Id = TripId.None;
        }

        private TripPose StepRoute(float dt)
        {
            float mergeAt = _ctx.Road.CurbMergeDistance;
            bool open = _ctx.CanJoin(TrafficLane.Top, mergeAt);
            if (_route.Remaining <= _ctx.Tuning.YieldLead) _ctx.Yield.Request(Id, mergeAt);
            bool yielding = _route.Remaining <= _ctx.Tuning.YieldDistance && !open;
            _route.Step(Mathf.Min(_ctx.Tuning.RouteSpeed * _ctx.Speeds.ExitScale, ObstacleCap(dt)), yielding, _ctx.Follow.Acceleration, _ctx.Follow.Brake, dt);
            if (!_route.AtEnd(_ctx.Tuning.ArriveTolerance)) return _route.Pose(TripPhase.OnRoute, _route.Speed);
            if (!open) return _route.Pose(TripPhase.Yielding, 0f);
            _lane = _ctx.Join(TrafficLane.Top, mergeAt, _route.Speed);
            _ctx.Yield.Release(Id);
            _phase = TripPhase.OnLane;
            return _route.Pose(TripPhase.OnLane, _route.Speed);
        }

        private TripPose StepLane(float dt)
        {
            LanePath path = _ctx.TopPath;
            LaneMotion.Step(ref _lane, _ctx.Tuning.CarLength, _ctx.Follow.DesiredSpeed * _ctx.Speeds.ExitScale, ObstacleCap(dt),
                _ctx.Occupancy, _ctx.Follow, dt);
            TripPhase phase = _lane.Distance >= path.Length ? TripPhase.Finished : TripPhase.OnLane;
            return new TripPose(path.PositionAt(_lane.Distance), path.HeadingAt(_lane.Distance), _lane.Speed, phase);
        }
    }
}
