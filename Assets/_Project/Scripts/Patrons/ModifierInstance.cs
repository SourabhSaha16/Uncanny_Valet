using UncannyValet.Core.Events;
using UncannyValet.Core.Modifiers;

namespace UncannyValet.Patrons
{
    /// <summary>
    /// One service modifier on one car: its own progress toward cleared. Fills only on game minutes the car sits in the
    /// station kind that clears it, and keeps its progress when the car leaves.
    /// </summary>
    public sealed class ModifierInstance
    {
        private readonly ServiceProgress _progress;

        public ModifierInstance(ModifierId id, StationKind clearedAt, float startProgress01, float clearMinutes)
        {
            Id = id; ClearedAt = clearedAt;
            _progress = new ServiceProgress(startProgress01, clearMinutes);
        }

        public ModifierId Id { get; }

        /// <summary>The kind of station that clears it.</summary>
        public StationKind ClearedAt { get; }

        /// <summary>0 untouched, 1 cleared.</summary>
        public float Progress01 => _progress.Value;

        public bool IsCleared => _progress.IsFull;

        /// <summary>One game minute. Returns true if the progress changed.</summary>
        public bool Advance(bool inStation) => inStation && _progress.Step();
    }
}
