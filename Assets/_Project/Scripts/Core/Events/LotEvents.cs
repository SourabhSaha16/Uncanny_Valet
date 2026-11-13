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

    /// <summary>A story power was earned (level 1) or upgraded (level 2).</summary>
    public readonly struct PowerGranted : IGameEvent
    {
        public readonly StoryPower Power;
        public readonly int Level;
        public PowerGranted(StoryPower power, int level) { Power = power; Level = level; }
    }
}
