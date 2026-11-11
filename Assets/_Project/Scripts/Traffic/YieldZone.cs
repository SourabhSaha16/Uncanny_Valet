using UnityEngine;

namespace UncannyValet.Traffic
{
    /// <summary>
    /// A virtual stopped car on the exit lane, a set distance before the point where a delivered car wants to merge.
    /// Ambient cars that can still stop in time treat it as a leader and brake smoothly, so the gap behind the merge point
    /// opens; cars too close or too fast to stop carry on (the merging car then waits for them to pass). One owner at a time.
    /// </summary>
    public sealed class YieldZone
    {
        private const float ToleranceSteps = 2f;

        private readonly float _lineBehind;
        private int _owner;
        private float _lineDistance;

        /// <param name="lineBehind">How far before the merge point the virtual car stands (match the merge gap behind).</param>
        public YieldZone(float lineBehind)
        {
            _lineBehind = lineBehind;
        }

        /// <summary>True while a merging car holds the zone.</summary>
        public bool Active => _owner != 0;

        /// <summary>
        /// Asks traffic to yield for a car (trip <paramref name="owner"/>) merging at <paramref name="mergeDistance"/> on the exit lane.
        /// Returns false when another car holds the zone.
        /// </summary>
        public bool Request(int owner, float mergeDistance)
        {
            if (_owner != 0 && _owner != owner) return false;
            _owner = owner;
            _lineDistance = mergeDistance - _lineBehind;
            return true;
        }

        /// <summary>Lets go of the zone if <paramref name="owner"/> holds it.</summary>
        public void Release(int owner)
        {
            if (_owner == owner) _owner = 0;
        }

        /// <summary>
        /// The bumper gap from a car to the virtual leader, and true when the car must obey it: the zone is active, the car is
        /// on the exit lane, short of the line and still able to stop (with a small tolerance for the step it is on).
        /// </summary>
        public bool TryGap(TrafficLane lane, float distance, float carLength, float speed, in FollowSettings follow, float dt, out float gap)
        {
            gap = float.PositiveInfinity;
            if (_owner == 0 || lane != TrafficLane.Top || distance >= _lineDistance) return false;
            float bumper = _lineDistance - distance - carLength;
            float free = Mathf.Max(0f, bumper - follow.MinGap);
            float stoppable = Mathf.Sqrt(2f * follow.Brake * free) + ToleranceSteps * follow.Brake * dt;
            if (speed > stoppable) return false;
            gap = bumper;
            return true;
        }
    }
}
