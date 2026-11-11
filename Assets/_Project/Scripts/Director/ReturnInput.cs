namespace UncannyValet.Director
{
    /// <summary>What the return scheduler reads in one tick besides the lot snapshot.</summary>
    public readonly struct ReturnInput
    {
        /// <summary>Shift seconds elapsed.</summary>
        public readonly float NowSeconds;
        /// <summary>Shift seconds left.</summary>
        public readonly float SecondsLeft;
        /// <summary>The pacing gap: workload minus target; negative means the player is under target.</summary>
        public readonly float Gap;
        /// <summary>Half-width of the on-target band.</summary>
        public readonly float Band;
        public readonly bool InRelief;
        /// <summary>Free regular spots are at or below the lot-tight threshold.</summary>
        public readonly bool LotTight;
        /// <summary>The intensity curve is in a wave peak, where the pickup spacing is waived.</summary>
        public readonly bool WavePeak;
        /// <summary>Shift seconds at which the last patron was sent to the curb; negative infinity when none yet.</summary>
        public readonly float LastSummonedSeconds;

        public ReturnInput(float nowSeconds, float secondsLeft, float gap, float band, bool inRelief, bool lotTight,
            bool wavePeak, float lastSummonedSeconds)
        {
            NowSeconds = nowSeconds; SecondsLeft = secondsLeft; Gap = gap; Band = band; InRelief = inRelief;
            LotTight = lotTight; WavePeak = wavePeak; LastSummonedSeconds = lastSummonedSeconds;
        }
    }
}
