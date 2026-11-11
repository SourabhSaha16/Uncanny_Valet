using UncannyValet.Core.Contracts;
using UnityEngine;

namespace UncannyValet.Traffic
{
    /// <summary>
    /// A patron car arriving: waits off-screen at the east lane start until it has a gap, drives the lane with car
    /// following, slows toward the queue turn, then drives the queue entry route and stops at its end.
    /// </summary>
    public sealed class EntryTrip : ITrip
    {
        private readonly TripContext _ctx;
        private readonly RouteRunner _route = new RouteRunner();
        private LaneState _lane;
        private TripPhase _phase;
        private int _slot;
        private float _sinceAsk;
        private readonly TripObstacle _obstacle = new TripObstacle();

        public EntryTrip(TripContext context)
        {
            _ctx = context;
        }

        /// <inheritdoc />
        public int Id { get; private set; }

        // The turn speed, scaled live by the entry speed tuning.
        private float TurnSpeed => _ctx.Tuning.TurnSpeed * _ctx.Speeds.EntryScale;

        /// <summary>Starts the trip toward arrival slot <paramref name="slot"/> under trip id <paramref name="id"/>.</summary>
        public void Begin(int id, int slot)
        {
            Id = id;
            _slot = slot;
            _sinceAsk = 0f;
            _ctx.Road.QueueEntryRoute(slot, _ctx.Scratch);
            _route.Begin(_ctx.Scratch);
            _lane = default;
            _obstacle.Reset();
            _phase = TripPhase.Waiting;
        }

        /// <inheritdoc />
        public void Step(float dt, out TripPose pose)
        {
            Reevaluate(dt);
            if (_phase == TripPhase.Waiting) pose = StepWaiting();
            else if (_phase == TripPhase.OnLane) pose = StepLane(dt);
            else pose = StepRoute(dt);
            if (pose.Phase == TripPhase.Finished) Cancel();
        }

        /// <inheritdoc />
        public void SetObstacleGap(float gap) => _obstacle.Report(gap);

        private float ObstacleCap(float dt) => _obstacle.SpeedCap(_ctx.Follow, _ctx.Tuning.CreepSpeed, _ctx.Tuning.BlockedTimeout, dt);

        /// <inheritdoc />
        public void Cancel()
        {
            if (_lane.Id != 0) _ctx.Occupancy.Remove(_lane.Id);
            _lane = default;
            Id = TripId.None;
        }

        // While driving in, ask now and then whether a slot nearer the curb has freed up, and head for it instead.
        private void Reevaluate(float dt)
        {
            _sinceAsk += dt;
            if (_sinceAsk < _ctx.Tuning.RetargetSeconds || !_ctx.TryGetSlots(out var source)) return;
            _sinceAsk = 0f;
            int slot = source.Reevaluate(_slot);
            if (slot >= _slot) return;
            _slot = slot;
            _ctx.Road.QueueEntryRoute(slot, _ctx.Scratch);
            _route.Retarget(_ctx.Scratch);
        }

        private TripPose StepWaiting()
        {
            if (_ctx.CanJoinEast(0f))
            {
                _lane = _ctx.JoinEast(0f, TurnSpeed);
                _phase = TripPhase.OnLane;
            }
            return LanePose(_phase, 0f);
        }

        private TripPose StepLane(float dt)
        {
            float turnAt = _ctx.Road.QueueTurnDistance;
            float cap = Mathf.Min(ApproachSpeed.Cap(turnAt - _lane.Distance, TurnSpeed, _ctx.Follow.Brake), ObstacleCap(dt));
            float cruise = _ctx.Follow.DesiredSpeed * _ctx.Speeds.EntryScale;
            LaneMotion.Step(ref _lane, _ctx.Tuning.CarLength, cruise, cap, _ctx.Occupancy, _ctx.Follow, dt);
            if (_lane.Distance < turnAt) return LanePose(TripPhase.OnLane, _lane.Speed);
            _lane.Distance = turnAt;
            _route.Speed = _lane.Speed;
            _ctx.Occupancy.Remove(_lane.Id);
            _lane = default;
            _phase = TripPhase.OnRoute;
            return _route.Pose(TripPhase.OnRoute, _route.Speed);
        }

        private TripPose StepRoute(float dt)
        {
            _route.Step(Mathf.Min(TurnSpeed, ObstacleCap(dt)), true, _ctx.Follow.Acceleration, _ctx.Follow.Brake, dt);
            if (!_route.AtEnd(_ctx.Tuning.ArriveTolerance)) return _route.Pose(TripPhase.OnRoute, _route.Speed);
            return _route.Pose(TripPhase.Finished, 0f);
        }

        private TripPose LanePose(TripPhase phase, float speed)
        {
            LanePath path = _ctx.EastPath;
            float distance = Mathf.Max(0f, _lane.Distance);
            return new TripPose(path.PositionAt(distance), path.HeadingAt(distance), speed, phase);
        }
    }
}
