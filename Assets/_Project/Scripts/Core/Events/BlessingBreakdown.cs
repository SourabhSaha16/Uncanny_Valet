namespace UncannyValet.Core.Events
{
    /// <summary>How one blessing was made up. Total is never below 0.</summary>
    public readonly struct BlessingBreakdown
    {
        public readonly int BaseBlessing;
        /// <summary>Speed bonus (positive) or wait penalty (negative).</summary>
        public readonly int SpeedAdjust;
        /// <summary>Blessings for the service modifiers cleared by hand-back (before damage and favour).</summary>
        public readonly int ModifierBonus;
        public readonly int Total;
        public readonly BlessingZeroReason ZeroReason;

        /// <summary>Bonus for patience left on the pickup timer.</summary>
        public readonly int PatienceBonus;

        /// <summary>Blessings taken off for damage (zero or negative).</summary>
        public readonly int DamageAdjust;

        /// <summary>The favour multiplier that was applied to the subtotal (1 or more).</summary>
        public readonly int FavourMultiplier;

        /// <summary>True when a needed service was missed (a modifier still open at hand-back): Total is halved. For tests and tooling; nothing draws it.</summary>
        public readonly bool ServiceMissed;
        public BlessingBreakdown(int baseBlessing, int speedAdjust, int modifierBonus, int total,
            BlessingZeroReason zeroReason, int patienceBonus = 0, int damageAdjust = 0, int favourMultiplier = 1,
            bool serviceMissed = false)
        {
            ServiceMissed = serviceMissed;
            BaseBlessing = baseBlessing; SpeedAdjust = speedAdjust; ModifierBonus = modifierBonus;
            Total = total; ZeroReason = zeroReason;
            PatienceBonus = patienceBonus; DamageAdjust = damageAdjust; FavourMultiplier = favourMultiplier;
        }
    }
}
