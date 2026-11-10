using UncannyValet.Core.Events;

namespace UncannyValet.Director
{
    /// <summary>What the picker has sent this shift: a count per archetype and the current Dirty and EV streaks.</summary>
    public sealed class ArchetypeHistory
    {
        private readonly int[] _counts = new int[ArchetypeKinds.Count];

        /// <summary>Cars picked this shift.</summary>
        public int Picks { get; private set; }

        /// <summary>Consecutive picks that needed the wash.</summary>
        public int DirtyStreak { get; private set; }

        /// <summary>Consecutive picks that needed a charging bay.</summary>
        public int EvStreak { get; private set; }

        public int Count(CarArchetype archetype) => _counts[(int)archetype];

        public void Record(CarArchetype archetype)
        {
            _counts[(int)archetype]++;
            Picks++;
            DirtyStreak = ArchetypeKinds.NeedsWash(archetype) ? DirtyStreak + 1 : 0;
            EvStreak = ArchetypeKinds.NeedsCharge(archetype) ? EvStreak + 1 : 0;
        }

        public void Reset()
        {
            for (var i = 0; i < _counts.Length; i++) _counts[i] = 0;
            Picks = 0;
            DirtyStreak = 0;
            EvStreak = 0;
        }
    }
}
