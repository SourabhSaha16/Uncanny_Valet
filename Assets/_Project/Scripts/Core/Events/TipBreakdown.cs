namespace UncannyValet.Core.Events
{
    /// <summary>How one tip was made up. Total is never below 0.</summary>
    public readonly struct TipBreakdown
    {
        public readonly int BaseTip;
        /// <summary>Speed bonus (positive) or wait penalty (negative).</summary>
        public readonly int SpeedAdjust;
        /// <summary>Dollars for the service modifiers cleared by hand-back (before damage and combo).</summary>
        public readonly int ModifierBonus;
        public readonly int Total;
        public readonly TipZeroReason ZeroReason;

        /// <summary>Bonus for patience left on the pickup timer.</summary>
        public readonly int PatienceBonus;

        /// <summary>Dollars taken off for damage (zero or negative).</summary>
        public readonly int DamageAdjust;

        /// <summary>The combo multiplier that was applied to the subtotal (1 or more).</summary>
        public readonly int ComboMultiplier;

        /// <summary>Dollars for Perfect Parks on this ticket (before damage and combo).</summary>
        public readonly int PerfectBonus;

        public TipBreakdown(int baseTip, int speedAdjust, int modifierBonus, int total,
            TipZeroReason zeroReason, int patienceBonus = 0, int damageAdjust = 0, int comboMultiplier = 1,
            int perfectBonus = 0)
        {
            BaseTip = baseTip; SpeedAdjust = speedAdjust; ModifierBonus = modifierBonus;
            Total = total; ZeroReason = zeroReason;
            PatienceBonus = patienceBonus; DamageAdjust = damageAdjust; ComboMultiplier = comboMultiplier;
            PerfectBonus = perfectBonus;
        }
    }
}
