namespace UncannyValet.Core.Contracts
{
    /// <summary>Everything the tip maths needs. Damage and requests join this struct later.</summary>
    public readonly struct TipInput
    {
        public readonly int BaseTip;
        public readonly float WaitMinutes;
        public readonly bool Impatient;

        /// <summary>True when the pickup timer ran out before the car came back. The tip is then $0.</summary>
        public readonly bool PickupExpired;

        public TipInput(int baseTip, float waitMinutes, bool impatient, bool pickupExpired = false)
        {
            BaseTip = baseTip; WaitMinutes = waitMinutes; Impatient = impatient;
            PickupExpired = pickupExpired;
        }
    }
}
