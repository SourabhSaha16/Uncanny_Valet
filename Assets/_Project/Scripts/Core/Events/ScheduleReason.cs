namespace UncannyValet.Core.Events
{
    /// <summary>Why the director sent the next car now.</summary>
    public enum ScheduleReason
    {
        /// <summary>The player's workload matched the target; the base interval ran out.</summary>
        OnTarget,

        /// <summary>The player had spare time, so the countdown was shortened.</summary>
        UnderTarget,

        /// <summary>The player was busy, so the countdown was lengthened.</summary>
        OverTarget,

        /// <summary>The player had nothing to do for too long: no dead air.</summary>
        Idle,

        /// <summary>One of the plain cars sent after a relief hold.</summary>
        ReliefCar
    }
}
