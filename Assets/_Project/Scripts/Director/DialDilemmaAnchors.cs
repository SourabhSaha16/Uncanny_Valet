using UncannyValet.Core.Events;

namespace UncannyValet.Director
{
    /// <summary>Anchors of the Dilemma Frequency dial: higher = boosts stronger, cooldowns shorter, more situations count.</summary>
    public static class DialDilemmaAnchors
    {
        private const TuningKey Dial = TuningKey.DialDilemmas;
        private const float BoostPivot = 1f; // a weight multiplier of 1 means "no boost"
        private const float BoostMax = 1.7f;

        /// <summary>Every link of the dial.</summary>
        public static DialLink[] All() => new[]
        {
            Boost(TuningKey.DirDilemmaWashBlocked),
            Boost(TuningKey.DirDilemmaWashBusy),
            Boost(TuningKey.DirDilemmaBaysBlocked),
            Boost(TuningKey.DirDilemmaBaySquatted),
            Boost(TuningKey.DirDilemmaLastBay),
            Boost(TuningKey.DirDilemmaLotTight),
            Boost(TuningKey.DirDilemmaChain),
            new DialLink(Dial, TuningKey.DirLotTightSpots, 0f, 2f),
            new DialLink(Dial, TuningKey.DirDilemmaCooldown, 3f, 0.4f),
            new DialLink(Dial, TuningKey.DirDilemmaBlackout, 2.5f, 0.4f),
            new DialLink(Dial, TuningKey.DirHeadroomGap, 0.3f, 2f),
        };

        // At dial 0 every boost is 1 (the picker is not nudged); at 10 each boost is 1.7 times as strong.
        private static DialLink Boost(TuningKey key) => new DialLink(Dial, key, 0f, BoostMax, DialEffect.Scale, BoostPivot);
    }
}
