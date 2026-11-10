using UncannyValet.Core.Contracts;

namespace UncannyValet.Traffic
{
    /// <summary>
    /// A delivered car leaving: drives the curb exit route, yields at the merge point until the east lane has a gap,
    /// merges, and drives to the right end of the lane where the trip finishes.
    /// </summary>
    public sealed class ExitTrip : ITrip
    {
        private readonly TripContext _ctx;
        private readonly RouteRunner _route = new RouteRunner();
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
            _phase = TripPhase.OnRoute;
        }

        /// <inheritdoc />
        public void Step(float dt, out TripPose pose)
        {
            pose = _phase == TripPhase.OnLane ? StepLane(dt) : StepRoute(dt);
            if (pose.Phase == TripPhase.Finished) Cancel();
        }

        /// <inheritdoc />
        public void Cancel()
        {
            if (_lane.Id != 0) _ctx.Occupancy.Remove(_lane.Id);
            _lane = default;
            Id = TripId.None;
        }

        private TripPose StepRoute(float dt)
        {
            float mergeAt = _ctx.Road.CurbMergeDistance;
            bool open = _ctx.CanJoinEast(mergeAt);
            bool yielding = _route.Remaining <= _ctx.Tuning.YieldDistance && !open;
            _route.Step(_ctx.Tuning.RouteSpeed, yielding, _ctx.Follow.Acceleration, _ctx.Follow.Brake, dt);
            if (!_route.AtEnd(_ctx.Tuning.ArriveTolerance)) return _route.Pose(TripPhase.OnRoute, _route.Speed);
            if (!open) return _route.Pose(TripPhase.Yielding, 0f);
            _lane = _ctx.JoinEast(mergeAt, _route.Speed);
            _phase = TripPhase.OnLane;
            return _route.Pose(TripPhase.OnLane, _route.Speed);
        }

        private TripPose StepLane(float dt)
        {
            LanePath path = _ctx.EastPath;
            LaneMotion.Step(ref _lane, _ctx.Tuning.CarLength, _ctx.Follow.DesiredSpeed, float.PositiveInfinity,
                _ctx.Occupancy, _ctx.Follow, dt);
            TripPhase phase = _lane.Distance >= path.Length ? TripPhase.Finished : TripPhase.OnLane;
            return new TripPose(path.PositionAt(_lane.Distance), path.HeadingAt(_lane.Distance), _lane.Speed, phase);
        }
    }
}
