namespace UncannyValet.Traffic
{
    /// <summary>
    /// Pure merge/yield decision: a car joining a lane may go only when the gap ahead and behind it on that lane is
    /// large enough. The gap behind grows with the follower's speed so a fast car is not cut off.
    /// </summary>
    public sealed class MergeGapRule
    {
        private readonly float _minAhead;
        private readonly float _minBehind;
        private readonly float _headway;

        public MergeGapRule(float minAhead, float minBehind, float headway)
        {
            _minAhead = minAhead;
            _minBehind = minBehind;
            _headway = headway;
        }

        /// <summary>True when the car may merge. Gaps may be infinity (no car on that side).</summary>
        public bool CanMerge(float gapAhead, float gapBehind, float followerSpeed) =>
            gapAhead >= _minAhead && gapBehind >= _minBehind + followerSpeed * _headway;
    }
}
