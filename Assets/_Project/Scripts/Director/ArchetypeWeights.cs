using UncannyValet.Core.Events;

namespace UncannyValet.Director
{
    /// <summary>The base draw weight of each archetype for a lot (the profile's numbers).</summary>
    public readonly struct ArchetypeWeights
    {
        public readonly float Plain;
        public readonly float Dirty;
        public readonly float Ev;
        public readonly float DirtyEv;

        public ArchetypeWeights(float plain, float dirty, float ev, float dirtyEv)
        {
            Plain = plain; Dirty = dirty; Ev = ev; DirtyEv = dirtyEv;
        }

        /// <summary>The Diner's base weights from the spec: 50, 20, 20, 10.</summary>
        public static ArchetypeWeights Diner => new ArchetypeWeights(50f, 20f, 20f, 10f);

        /// <summary>The weight of one archetype.</summary>
        public float Of(CarArchetype archetype)
        {
            switch (archetype)
            {
                case CarArchetype.Dirty: return Dirty;
                case CarArchetype.Ev: return Ev;
                case CarArchetype.DirtyEv: return DirtyEv;
                default: return Plain;
            }
        }
    }
}
