using UncannyValet.Core.Modifiers;

namespace UncannyValet.Core.Contracts
{
    /// <summary>Everything the blessing maths needs.</summary>
    public readonly struct BlessingInput
    {
        public readonly int BaseBlessing;
        public readonly float WaitMinutes;
        public readonly bool Impatient;

        /// <summary>True when the pickup timer ran out before the car came back. The blessing is then 0.</summary>
        public readonly bool PickupExpired;

        /// <summary>The modifiers cleared by hand-back; each earns its bonus.</summary>
        public readonly ModifierSet ClearedModifiers;

        /// <summary>The modifiers still open at hand-back (reported to the blessing rules; the service-missed rule reads it).</summary>
        public readonly ModifierSet OpenModifiers;

        /// <summary>Share 0..1 of the pickup patience still left when the car came back. Earns the patience bonus.</summary>
        public readonly float PatienceRemaining01;

        /// <summary>Damage 0..1 on the car. Higher damage, lower blessing; 1 is a wreck and pays 0.</summary>
        public readonly float Damage01;

        /// <summary>Favour multiplier applied last, 1 or more (a value below 1 counts as 1).</summary>
        public readonly int FavourMultiplier;
        public BlessingInput(int baseBlessing, float waitMinutes, bool impatient, bool pickupExpired = false,
            ModifierSet clearedModifiers = default, ModifierSet openModifiers = default,
            float patienceRemaining01 = 0f, float damage01 = 0f, int favourMultiplier = 1)
        {
            BaseBlessing = baseBlessing; WaitMinutes = waitMinutes; Impatient = impatient;
            PickupExpired = pickupExpired;
            ClearedModifiers = clearedModifiers; OpenModifiers = openModifiers;
            PatienceRemaining01 = patienceRemaining01; Damage01 = damage01; FavourMultiplier = favourMultiplier;
        }
    }
}
