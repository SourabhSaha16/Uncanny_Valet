using UncannyValet.Core.Events;

namespace UncannyValet.Patrons
{
    /// <summary>Pure mapping from a director archetype to the car traits it stands for.</summary>
    public static class ArchetypeTraits
    {
        public static void Of(CarArchetype archetype, out bool isEv, out bool isDirty)
        {
            isEv = archetype == CarArchetype.Ev || archetype == CarArchetype.DirtyEv;
            isDirty = archetype == CarArchetype.Dirty || archetype == CarArchetype.DirtyEv;
        }
    }
}
