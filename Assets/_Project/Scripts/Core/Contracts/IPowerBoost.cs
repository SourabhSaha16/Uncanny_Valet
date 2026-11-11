namespace UncannyValet.Core.Contracts
{
    /// <summary>The live state of the level 1 speed boost, registered by the Powers module and read by whoever walks the valet.</summary>
    public interface IPowerBoost
    {
        /// <summary>True while the speed boost runs.</summary>
        bool IsBoosted { get; }

        /// <summary>Walk speed multiplier right now: 1 when not boosted.</summary>
        float WalkMultiplier { get; }

        /// <summary>Seconds of boost left (0 when not boosted).</summary>
        float Remaining { get; }
    }
}
