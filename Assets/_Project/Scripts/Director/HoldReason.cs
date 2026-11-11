namespace UncannyValet.Director
{
    /// <summary>Why the arrival pacer is holding back a car.</summary>
    public enum HoldReason
    {
        None,

        /// <summary>The player is drowning; arrivals are held for a while.</summary>
        Relief,

        /// <summary>The waiting queue is full.</summary>
        QueueFull,

        /// <summary>The shift's arrival budget (including slack) is used up.</summary>
        BudgetSpent,

        /// <summary>Too little of the shift is left for a new car to be parked and picked up (see ArrivalIdleCutoff).</summary>
        ShiftEnding
    }
}
