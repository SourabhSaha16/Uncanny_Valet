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

        /// <summary>Share 0..1 of the pickup patience still left when the car came back. Earns the patience bonus.</summary>
        public readonly float PatienceRemaining01;

        /// <summary>Damage 0..1 on the car. Higher damage, lower tip; 1 is a wreck and pays $0.</summary>
        public readonly float Damage01;

        /// <summary>Combo multiplier applied last, 1 or more (a value below 1 counts as 1).</summary>
        public readonly int ComboMultiplier;

        public TipInput(int baseTip, float waitMinutes, bool impatient, bool pickupExpired = false,
            bool chargedEv = false, bool cleanedDirty = false, bool uncleanedDirty = false,
            float patienceRemaining01 = 0f, float damage01 = 0f, int comboMultiplier = 1)
        {
            BaseTip = baseTip; WaitMinutes = waitMinutes; Impatient = impatient;
            PickupExpired = pickupExpired; ChargedEv = chargedEv;
            CleanedDirty = cleanedDirty; UncleanedDirty = uncleanedDirty;
            PatienceRemaining01 = patienceRemaining01; Damage01 = damage01; ComboMultiplier = comboMultiplier;
        }
    }
}
