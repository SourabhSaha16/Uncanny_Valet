using UncannyValet.Core.Events;

namespace UncannyValet.Director
{
    /// <summary>Pure facts about the car archetypes: how many there are and what each one needs.</summary>
    public static class ArchetypeKinds
    {
        /// <summary>The number of archetypes; archetype values index arrays of this size.</summary>
        public const int Count = 4;

        /// <summary>The archetype needs the wash.</summary>
        public static bool NeedsWash(CarArchetype a) => a == CarArchetype.Dirty || a == CarArchetype.DirtyEv;

        /// <summary>The archetype needs a charging bay.</summary>
        public static bool NeedsCharge(CarArchetype a) => a == CarArchetype.Ev || a == CarArchetype.DirtyEv;
    }
}
