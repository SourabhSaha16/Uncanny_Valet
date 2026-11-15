using System.Collections.Generic;
using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Ids;
using UnityEngine;

namespace UncannyValet.Patrons
{
    /// <summary>
    /// Checks which arrival slots have a car in them. Trigger colliders (stop zones, sensors) and pedestrians never count, and the
    /// scan reuses its buffers, so it never allocates.
    /// </summary>
    public sealed class ArrivalSlotScanner : IArrivalScan
    {
        private readonly Vector2 _clearSize;
        private readonly ILotLayout _lot;
        private readonly List<bool> _blocked = new List<bool>();
        private readonly Collider2D[] _hits = new Collider2D[8];
        private ContactFilter2D _filter;

        public ArrivalSlotScanner(Vector2 clearSize, ILotLayout lot)
        {
            _clearSize = clearSize;
            _lot = lot;
            _filter = new ContactFilter2D();
            _filter.useTriggers = false;
            _filter.SetLayerMask(~PhysicsLayers.PedestrianMask);
        }

        /// <inheritdoc />
        public bool IsTaken(int slot, CarId except)
        {
            var slots = _lot.ArrivalSlots;
            if (slot < 0 || slot >= slots.Count) return false;
            var count = Physics2D.OverlapBox(slots[slot], _clearSize, _lot.ArrivalRotationDegrees, _filter, _hits);
            for (var i = 0; i < count; i++)
            {
                var car = _hits[i].GetComponentInParent<IDrivable>();
                if (car == null || car.Id != except) return true;
            }
            return false;
        }

        /// <summary>Tests the first <paramref name="maxSlots"/> slots. The result is valid until the next scan.</summary>
        public IReadOnlyList<bool> Scan(int maxSlots)
        {
            _blocked.Clear();
            var slots = _lot.ArrivalSlots;
            var count = Mathf.Min(maxSlots, slots.Count);
            for (var i = 0; i < count; i++)
                _blocked.Add(Physics2D.OverlapBox(slots[i], _clearSize, _lot.ArrivalRotationDegrees, _filter, _hits) > 0);
            return _blocked;
        }
    }
}
