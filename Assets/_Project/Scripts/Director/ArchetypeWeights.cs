using UncannyValet.Core.Events;

namespace UncannyValet.Director
{
    /// <summary>The base draw weight of each archetype for a lot (the profile's numbers).</summary>
    public readonly struct ArchetypeWeights
    {
        public readonly float Plain;
        public readonly float Muddy;
        public readonly float Ev;
        public readonly float MuddyEv;

        public ArchetypeWeights(float plain, float muddy, float ev, float muddyEv)
        {
            Plain = plain; Muddy = muddy; Ev = ev; MuddyEv = muddyEv;
        }

        /// <summary>The Diner's base weights from the spec: 50, 20, 20, 10.</summary>
        public static ArchetypeWeights Diner => new ArchetypeWeights(50f, 20f, 20f, 10f);

        /// <summary>The weight of one archetype.</summary>
        public float Of(CarArchetype archetype)
        {
            switch (archetype)
            {
                case CarArchetype.Muddy: return Muddy;
                case CarArchetype.Ev: return Ev;
                case CarArchetype.MuddyEv: return MuddyEv;
                default: return Plain;
            }
        }
    }
}
