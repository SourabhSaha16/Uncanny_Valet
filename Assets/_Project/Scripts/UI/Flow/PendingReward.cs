using UncannyValet.Core.Ids;

namespace UncannyValet.UI.Flow
{
    /// <summary>A power or upgrade earned in the lot just played, waiting for its reward card.</summary>
    public readonly struct PendingReward
    {
        public readonly StoryPower Power;
        /// <summary>1 earned, 2 upgraded.</summary>
        public readonly int Level;

        public PendingReward(StoryPower power, int level) { Power = power; Level = level; }
    }
}
