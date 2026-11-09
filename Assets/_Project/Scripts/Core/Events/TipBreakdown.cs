namespace UncannyValet.Core.Events
{
    /// <summary>How one tip was made up. Total is never below 0.</summary>
    public readonly struct TipBreakdown
    {
        public readonly int BaseTip;
        /// <summary>Speed bonus (positive) or wait penalty (negative).</summary>
        public readonly int SpeedAdjust;
        public readonly int ChargeBonus;
        public readonly int CleanBonus;
        public readonly int Total;
        public readonly TipZeroReason ZeroReason;

        /// <summary>Bonus for patience left on the pickup timer.</summary>
        public readonly int PatienceBonus;

        /// <summary>Dollars taken off for damage (zero or negative).</summary>
        public readonly int DamageAdjust;

        /// <summary>The combo multiplier that was applied to the subtotal (1 or more).</summary>
        public readonly int ComboMultiplier;

        public TipBreakdown(int baseTip, int speedAdjust, int chargeBonus, int cleanBonus, int total,
            TipZeroReason zeroReason, int patienceBonus = 0, int damageAdjust = 0, int comboMultiplier = 1)
        {
            BaseTip = baseTip; SpeedAdjust = speedAdjust; ChargeBonus = chargeBonus;
            CleanBonus = cleanBonus; Total = total; ZeroReason = zeroReason;
            PatienceBonus = patienceBonus; DamageAdjust = damageAdjust; ComboMultiplier = comboMultiplier;
        }
    }
}
