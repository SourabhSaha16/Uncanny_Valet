using UncannyValet.Core.Events;
using UncannyValet.Core.Modifiers;

namespace UncannyValet.Director
{
    /// <summary>What the picker has sent this shift: a count per archetype and the current streak of each service modifier.</summary>
    public sealed class ArchetypeHistory
    {
        private readonly int[] _counts = new int[ArchetypeKinds.Count];
        private readonly int[] _streaks = new int[ModifierSet.Capacity];

        /// <summary>Cars picked this shift.</summary>
        public int Picks { get; private set; }

        /// <summary>Consecutive picks that carried this modifier.</summary>
        public int StreakOf(ModifierId id) => _streaks[(int)id];

        public int Count(CarArchetype archetype) => _counts[(int)archetype];

        public void Record(CarArchetype archetype)
        {
            _counts[(int)archetype]++;
            Picks++;
            var carried = ArchetypeKinds.Modifiers(archetype);
            for (var i = 1; i < _streaks.Length; i++)
                _streaks[i] = carried.Contains((ModifierId)i) ? _streaks[i] + 1 : 0;
        }

        public void Reset()
        {
            for (var i = 0; i < _counts.Length; i++) _counts[i] = 0;
            Picks = 0;
            for (var i = 0; i < _streaks.Length; i++) _streaks[i] = 0;
        }
    }
}
