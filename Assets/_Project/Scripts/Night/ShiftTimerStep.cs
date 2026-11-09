using System;

namespace UncannyValet.Night
{
    /// <summary>What happened during one <see cref="ShiftTimer.Advance"/> call.</summary>
    [Flags]
    public enum ShiftTimerStep
    {
        None = 0,
        /// <summary>The final stretch began (once per shift).</summary>
        EnteredFinalStretch = 1,
        /// <summary>The time ran out (once per shift).</summary>
        Ended = 2,
    }
}
