namespace UncannyValet.Core.Watchers
{
    /// <summary>The things a watcher comments on while the shift runs. Each has its own list of lines in the text asset.</summary>
    public enum WatcherEvent
    {
        /// <summary>A patron turned angry or a curse hit.</summary>
        Curse = 0,
        PerfectPark = 1,
        PerfectDropOff = 2,
        /// <summary>A hit that damaged a car by more than the configured amount.</summary>
        BigCrash = 3,
        /// <summary>The shift's blessings passed half of the threshold.</summary>
        HalfwayToGoal = 4,
        /// <summary>The shift's blessings reached the threshold.</summary>
        GoalReached = 5,
        Last30Seconds = 6
    }
}
