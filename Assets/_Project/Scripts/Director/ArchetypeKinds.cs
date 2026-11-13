using UncannyValet.Core.Events;
using UncannyValet.Core.Modifiers;

namespace UncannyValet.Director
{
    /// <summary>Pure facts about the car archetypes: how many there are and which service modifiers each one carries.</summary>
    public static class ArchetypeKinds
    {
        /// <summary>The number of archetypes; archetype values index arrays of this size.</summary>
        public const int Count = 4;

        /// <summary>The service modifiers a car of this archetype carries.</summary>
        public static ModifierSet Modifiers(CarArchetype a) => ArchetypeModifiers.Of(a);
    }
}
