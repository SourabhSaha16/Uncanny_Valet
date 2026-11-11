using UnityEngine;

namespace UncannyValet.Traffic
{
    /// <summary>
    /// A trip car's reaction to something solid ahead (reported by the car): a speed cap that brakes it to a stop
    /// behind the obstacle with the car-following maths, and after a timeout lets it creep on so it never waits forever.
    /// </summary>
    public sealed class TripObstacle
    {
        /// <summary>A cap at or below this counts as standing still behind the obstacle.</summary>
        public const float StoppedSpeed = 0.05f;

        private float _gap = float.PositiveInfinity;
        private float _blockedFor;

        /// <summary>Seconds the car has been held up behind the current obstacle.</summary>
        public float BlockedFor => _blockedFor;

        /// <summary>Records the gap ahead (infinity when clear).</summary>
        public void Report(float gap) => _gap = gap;

        /// <summary>Forgets the obstacle and the waiting time (the trip starts or ends).</summary>
        public void Reset()
        {
            _gap = float.PositiveInfinity;
            _blockedFor = 0f;
        }

        /// <summary>
        /// The fastest speed the car may drive this step: infinity when clear, the stopping speed behind the obstacle,
        /// or the creep speed once blocked longer than the timeout (it then pushes on until the way is clear).
        /// </summary>
        public float SpeedCap(in FollowSettings follow, float creepSpeed, float timeout, float dt)
        {
            if (float.IsPositiveInfinity(_gap))
            {
                _blockedFor = 0f;
                return float.PositiveInfinity;
            }
            float cap = CarFollowing.TargetSpeed(_gap, 0f, follow.WithDesiredSpeed(float.MaxValue));
            if (cap <= StoppedSpeed) _blockedFor += dt;
            return _blockedFor >= timeout ? Mathf.Max(cap, creepSpeed) : cap;
        }
    }
}
