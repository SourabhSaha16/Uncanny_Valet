using UncannyValet.Core.Modifiers;

namespace UncannyValet.Patrons
{
    /// <summary>
    /// Keeps dirty (muddy) arrivals apart: after a dirty car at least <c>requiredCleanCars</c> clean ones must arrive
    /// before the next dirty car, so mud never comes back to back. Counted per shift: <see cref="Reset"/> on shift start.
    /// </summary>
    public sealed class DirtySpacing
    {
        private readonly int _requiredCleanCars;
        private int _cleanSinceDirty;
        private bool _anyDirty;

        /// <param name="requiredCleanCars">Clean cars that must separate two dirty ones (0 turns the rule off).</param>
        public DirtySpacing(int requiredCleanCars) { _requiredCleanCars = requiredCleanCars; }

        /// <summary>Forgets the last arrival (a new shift starts with no dirty car behind it).</summary>
        public void Reset() { _anyDirty = false; _cleanSinceDirty = 0; }

        /// <summary>
        /// Takes the next arrival's modifiers and returns what the car really gets: the same set, or without mud when
        /// that would be too soon after the last dirty car. Records the outcome.
        /// </summary>
        public ModifierSet Apply(ModifierSet set)
        {
            var dirty = set.Contains(ModifierId.Muddy);
            if (dirty && _anyDirty && _cleanSinceDirty < _requiredCleanCars)
            {
                set = set.Without(ModifierId.Muddy);
                dirty = false;
            }
            if (dirty) { _anyDirty = true; _cleanSinceDirty = 0; }
            else if (_anyDirty) _cleanSinceDirty++;
            return set;
        }
    }
}
