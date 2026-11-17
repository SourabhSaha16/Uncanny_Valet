using UncannyValet.Core.Ids;

namespace UncannyValet.Core.Contracts
{
    /// <summary>What the HUD and other modules may know about one owned power right now.</summary>
    public readonly struct PowerState
    {
        public readonly StoryPower Power;
        /// <summary>1 earned, 2 upgraded.</summary>
        public readonly int Level;
        public readonly PowerPhase Phase;
        /// <summary>Active: share of the use left. Cooling: share of the cooldown left. Ready: 0. Always 0 to 1.</summary>
        public readonly float Remaining01;
        /// <summary>The live numbers of the current level.</summary>
        public readonly PowerLevelValues Values;

        public PowerState(StoryPower power, int level, PowerPhase phase, float remaining01, PowerLevelValues values)
        {
            Power = power; Level = level; Phase = phase; Remaining01 = remaining01; Values = values;
        }
    }
}
