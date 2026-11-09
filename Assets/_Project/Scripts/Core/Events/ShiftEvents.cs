namespace UncannyValet.Core.Events
{
    // Shift events, grouped in one file by domain.

    /// <summary>The rules for the shift about to start (or changed live). Published just before ShiftStarted.</summary>
    public readonly struct ShiftPlanChanged : IGameEvent
    {
        public readonly ShiftPlan Plan;
        public ShiftPlanChanged(ShiftPlan plan) { Plan = plan; }
    }

    /// <summary>A shift began. Score, combo and tickets start clean.</summary>
    public readonly struct ShiftStarted : IGameEvent { }

    /// <summary>Published once per game minute. ElapsedMinutes counts from the start of the shift.</summary>
    public readonly struct ShiftTick : IGameEvent
    {
        public readonly float ElapsedMinutes;
        public ShiftTick(float elapsedMinutes) { ElapsedMinutes = elapsedMinutes; }
    }

    /// <summary>The shift entered its final stretch (30 seconds by default). Published once per shift.</summary>
    public readonly struct ShiftLast30Seconds : IGameEvent { }

    /// <summary>The time ran out. Published once per shift.</summary>
    public readonly struct ShiftEnded : IGameEvent { }

    /// <summary>Published by the Economy module after ShiftEnded, once the shift's numbers are final.</summary>
    public readonly struct ShiftResultReady : IGameEvent
    {
        public readonly ShiftResult Result;
        public ShiftResultReady(ShiftResult result) { Result = result; }
    }

    /// <summary>The player pressed Retry on the results screen: start the shift again.</summary>
    public readonly struct RetryShiftRequested : IGameEvent { }
}
