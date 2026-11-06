using System.Collections.Generic;
using UnityEngine;

namespace UncannyValet.Patrons
{
    /// <summary>Checks which arrival slots have a car in them. Reuses its buffer, so it never allocates per scan.</summary>
    public sealed class ArrivalSlotScanner
    {
        private readonly Vector2 _clearSize;
        private readonly List<bool> _blocked = new List<bool>();

        public ArrivalSlotScanner(Vector2 clearSize) { _clearSize = clearSize; }

        /// <summary>Tests the first <paramref name="maxSlots"/> slots. The result is valid until the next scan.</summary>
        public IReadOnlyList<bool> Scan(IReadOnlyList<Vector2> slots, float rotationDegrees, int maxSlots)
        {
            _blocked.Clear();
            var count = Mathf.Min(maxSlots, slots.Count);
            for (var i = 0; i < count; i++)
                _blocked.Add(Physics2D.OverlapBox(slots[i], _clearSize, rotationDegrees) != null);
            return _blocked;
        }
    }
}
