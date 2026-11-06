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

        public TipBreakdown(int baseTip, int speedAdjust, int chargeBonus, int cleanBonus, int total, TipZeroReason zeroReason)
        {
            BaseTip = baseTip; SpeedAdjust = speedAdjust; ChargeBonus = chargeBonus;
            CleanBonus = cleanBonus; Total = total; ZeroReason = zeroReason;
        }
    }
}
