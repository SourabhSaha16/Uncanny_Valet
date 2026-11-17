using System.Collections.Generic;
using UncannyValet.Core.Ids;

namespace UncannyValet.Core.Contracts
{
    /// <summary>
    /// Read-only view of the run state: where the player is, what he has earned and cleared. In memory for the
    /// browser session only. The Run module owns it and updates it from LotStarted, LotCleared and PowerGranted.
    /// </summary>
    public interface IRunProgress
    {
        /// <summary>True once a lot has been started.</summary>
        bool HasCurrentLot { get; }

        /// <summary>The lot being played; meaningful only when <see cref="HasCurrentLot"/>.</summary>
        LotId CurrentLot { get; }

        /// <summary>The powers earned, in the order they were first earned (the key order of the power bar).</summary>
        IReadOnlyList<StoryPower> EarnedPowers { get; }

        /// <summary>0 when the power is not owned, 1 when earned, 2 when upgraded.</summary>
        int PowerLevel(StoryPower power);

        /// <summary>The level of the power whose icon flash the HUD already showed (0 until it flashed); below <see cref="PowerLevel"/> means a flash is due at the next lot start.</summary>
        int FlashedLevel(StoryPower power);

        /// <summary>True once this lot was cleared. Training and crazy lots of one world are separate.</summary>
        bool IsCleared(LotId lot);

        /// <summary>True once the world's crazy lot (or the Asgard finale) was cleared.</summary>
        bool IsWorldCleared(World world);

        /// <summary>The highest blessing total at shift end recorded for the lot; 0 until blessings exist or when never played.</summary>
        int BestBlessings(LotId lot);
    }
}
