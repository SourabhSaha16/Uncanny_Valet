using System;

namespace UncannyValet.Economy
{
    /// <summary>
    /// The blessing multiplier. Starts at 1x, rises one step per back-to-back happy patron up to a cap, and falls
    /// back to 1x on a miss. Pure.
    /// </summary>
    public sealed class FavourTracker
    {
        private readonly int _max;

        public FavourTracker(int maxMultiplier) => _max = Math.Max(1, maxMultiplier);

        /// <summary>The multiplier for the next blessing, from 1 to the cap.</summary>
        public int Multiplier { get; private set; } = 1;

        /// <summary>A happy patron: the next blessing is worth one step more. Returns true when the multiplier changed.</summary>
        public bool RecordHappy()
        {
            if (Multiplier >= _max) return false;
            Multiplier++;
            return true;
        }

        /// <summary>A miss, wreck or angry patron: back to 1x. Returns true when the multiplier changed.</summary>
        public bool Reset()
        {
            if (Multiplier == 1) return false;
            Multiplier = 1;
            return true;
        }
    }
}
