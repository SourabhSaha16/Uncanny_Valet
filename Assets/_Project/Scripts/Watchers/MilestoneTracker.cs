namespace UncannyValet.Watchers
{
    /// <summary>
    /// Pure: reports each blessing milestone once per shift, the first time the blessings reach half of the threshold and the first time
    /// they reach the threshold. Reaching both at once reports only <see cref="GoalMilestone.Reached"/>.
    /// </summary>
    public sealed class MilestoneTracker
    {
        private bool _halfway;
        private bool _reached;

        /// <summary>The milestone newly passed by <paramref name="blessings"/>, or None.</summary>
        public GoalMilestone Update(int blessings, int threshold)
        {
            if (threshold < 1) return GoalMilestone.None;
            if (!_reached && blessings >= threshold)
            {
                _reached = true;
                _halfway = true;
                return GoalMilestone.Reached;
            }
            if (!_halfway && blessings * 2 >= threshold)
            {
                _halfway = true;
                return GoalMilestone.Halfway;
            }
            return GoalMilestone.None;
        }

        /// <summary>A new shift starts: every milestone can be reported again.</summary>
        public void Reset()
        {
            _halfway = false;
            _reached = false;
        }
    }
}
