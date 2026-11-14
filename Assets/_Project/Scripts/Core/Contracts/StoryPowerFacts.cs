using UncannyValet.Core.Ids;

namespace UncannyValet.Core.Contracts
{
    /// <summary>What the reward card needs to know about a power that its definition holds: its name and a one-line description.</summary>
    public readonly struct StoryPowerFacts
    {
        public readonly StoryPower Power;
        public readonly string Name;
        public readonly string Description;

        public StoryPowerFacts(StoryPower power, string name, string description)
        {
            Power = power; Name = name; Description = description ?? string.Empty;
        }
    }
}
