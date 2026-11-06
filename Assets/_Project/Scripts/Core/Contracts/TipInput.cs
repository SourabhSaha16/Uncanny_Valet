namespace UncannyValet.Core.Contracts
{
    /// <summary>Everything the tip maths needs.</summary>
    public readonly struct TipInput
    {
        public readonly int BaseTip;
        public readonly float WaitMinutes;
        public readonly bool Impatient;

        /// <summary>True when the pickup timer ran out before the car came back. The tip is then $0.</summary>
        public readonly bool PickupExpired;

        /// <summary>An EV handed back with a full charge: earns the charge bonus.</summary>
        public readonly bool ChargedEv;

        /// <summary>A dirty car that was washed: earns the clean bonus.</summary>
        public readonly bool CleanedDirty;

        /// <summary>A dirty car handed back unwashed: the tip is $0.</summary>
        public readonly bool UncleanedDirty;

        public TipInput(int baseTip, float waitMinutes, bool impatient, bool pickupExpired = false,
            bool chargedEv = false, bool cleanedDirty = false, bool uncleanedDirty = false)
        {
            BaseTip = baseTip; WaitMinutes = waitMinutes; Impatient = impatient;
            PickupExpired = pickupExpired; ChargedEv = chargedEv;
            CleanedDirty = cleanedDirty; UncleanedDirty = uncleanedDirty;
        }
    }
}
