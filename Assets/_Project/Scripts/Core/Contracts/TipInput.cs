using UncannyValet.Core.Modifiers;

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

        /// <summary>The modifiers cleared by hand-back; each earns its bonus.</summary>
        public readonly ModifierSet ClearedModifiers;

        /// <summary>The modifiers still open at hand-back (reported to the tip rules; the service-missed rule reads it).</summary>
        public readonly ModifierSet OpenModifiers;

        /// <summary>Share 0..1 of the pickup patience still left when the car came back. Earns the patience bonus.</summary>
        public readonly float PatienceRemaining01;

        /// <summary>Damage 0..1 on the car. Higher damage, lower tip; 1 is a wreck and pays $0.</summary>
        public readonly float Damage01;

        /// <summary>Combo multiplier applied last, 1 or more (a value below 1 counts as 1).</summary>
        public readonly int ComboMultiplier;

        /// <summary>Perfect Parks earned and not yet paid on this ticket. Each adds the Perfect Park bonus.</summary>
        public readonly int PerfectParks;

        public TipInput(int baseTip, float waitMinutes, bool impatient, bool pickupExpired = false,
            ModifierSet clearedModifiers = default, ModifierSet openModifiers = default,
            float patienceRemaining01 = 0f, float damage01 = 0f, int comboMultiplier = 1,
            int perfectParks = 0)
        {
            BaseTip = baseTip; WaitMinutes = waitMinutes; Impatient = impatient;
            PickupExpired = pickupExpired;
            ClearedModifiers = clearedModifiers; OpenModifiers = openModifiers;
            PatienceRemaining01 = patienceRemaining01; Damage01 = damage01; ComboMultiplier = comboMultiplier;
            PerfectParks = perfectParks;
        }
    }
}
