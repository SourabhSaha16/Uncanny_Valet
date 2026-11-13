using UncannyValet.Core.Modifiers;

namespace UncannyValet.Patrons
{
    /// <summary>
    /// Everything that defines one patron visit. The spawner and story cards both build one of these
    /// and hand it to <see cref="PatronSpawner.TrySpawn"/>.
    /// </summary>
    public readonly struct PatronRequest
    {
        /// <summary>Default pickup window in game minutes (also the PatronConfig default). Impatient patrons get it scaled down.</summary>
        public const float DefaultPickupWindowMinutes = 25f;

        public readonly float StayMinutes;
        public readonly int BaseBlessing;
        public readonly bool Impatient;
        public readonly float PickupWindowMinutes;
        /// <summary>The service modifiers the car arrives with.</summary>
        public readonly ModifierSet Modifiers;

        public PatronRequest(float stayMinutes, int baseBlessing, bool impatient,
            float pickupWindowMinutes = DefaultPickupWindowMinutes,
            ModifierSet modifiers = default)
        {
            StayMinutes = stayMinutes; BaseBlessing = baseBlessing; Impatient = impatient;
            PickupWindowMinutes = pickupWindowMinutes;
            Modifiers = modifiers;
        }

        /// <summary>The same visit with another stay.</summary>
        public PatronRequest WithStay(float stayMinutes)
            => new PatronRequest(stayMinutes, BaseBlessing, Impatient, PickupWindowMinutes, Modifiers);
    }
}
