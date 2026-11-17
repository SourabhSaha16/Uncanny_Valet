using UncannyValet.Core.Ids;

namespace UncannyValet.Core.Events
{
    // Run events, grouped in one file by domain.

    /// <summary>A lot scene finished loading and built itself. Published once per lot load by Bootstrap.</summary>
    public readonly struct LotStarted : IGameEvent
    {
        public readonly LotId Lot;
        public LotStarted(LotId lot) { Lot = lot; }
    }

    /// <summary>The player met the lot's blessing threshold. Published by the clear rules (not yet built).</summary>
    public readonly struct LotCleared : IGameEvent
    {
        public readonly LotId Lot;
        public LotCleared(LotId lot) { Lot = lot; }
    }

    /// <summary>The shift ended below the lot's blessing threshold. Published by the clear rules (not yet built).</summary>
    public readonly struct LotFailed : IGameEvent
    {
        public readonly LotId Lot;
        public LotFailed(LotId lot) { Lot = lot; }
    }

    /// <summary>
    /// The run decided which lot comes next after a lot ended. HasNext is false (and Next is meaningless) when Reason is
    /// RunComplete. Nothing loads a scene from it; the between-lot flow reacts.
    /// </summary>
    public readonly struct NextLotChosen : IGameEvent
    {
        public readonly LotId Next;
        public readonly RunOutcome Reason;
        public bool HasNext => Reason != RunOutcome.RunComplete;
        public NextLotChosen(LotId next, RunOutcome reason) { Next = next; Reason = reason; }
    }

    /// <summary>The run is over: the finale was cleared, or the last lot of the run definition was (end of content).</summary>
    public readonly struct RunCompleted : IGameEvent
    {
        public readonly RunEnd Reason;
        public RunCompleted(RunEnd reason) { Reason = reason; }
    }

    /// <summary>A story power was earned (level 1) or upgraded (level 2).</summary>
    public readonly struct PowerGranted : IGameEvent
    {
        public readonly StoryPower Power;
        public readonly int Level;
        public PowerGranted(StoryPower power, int level) { Power = power; Level = level; }
    }

    /// <summary>A story power was taken away again (the developer panel only; the story never removes a power).</summary>
    public readonly struct PowerRevoked : IGameEvent
    {
        public readonly StoryPower Power;
        public PowerRevoked(StoryPower power) { Power = power; }
    }
}
