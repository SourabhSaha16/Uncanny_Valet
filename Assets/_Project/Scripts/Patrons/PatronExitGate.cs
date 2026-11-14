using System.Collections.Generic;
using UncannyValet.Core.Ids;

namespace UncannyValet.Patrons
{
    /// <summary>
    /// Pure rule: a patron stays in the car until the valet comes up to the car in the pickup spot (issue 127). Each car is
    /// released once. Cars further back in the queue are never released by approach.
    /// </summary>
    public sealed class PatronExitGate
    {
        private readonly HashSet<CarId> _released = new HashSet<CarId>();

        /// <summary>True once for a car that stands in the pickup spot with the valet within <paramref name="range"/>.</summary>
        public bool TryReleaseByApproach(CarId car, int slot, float valetDistance, float range)
        {
            if (slot != QueueAdvanceRule.PickupSlot || valetDistance > range) return false;
            return _released.Add(car);
        }

        /// <summary>The valet took the car some other way, so the patron must not stay inside. True once per car.</summary>
        public bool TryReleaseByTakeover(CarId car) => _released.Add(car);

        /// <summary>True when the car's patron was already told to step out.</summary>
        public bool IsReleased(CarId car) => _released.Contains(car);

        public void Forget(CarId car) => _released.Remove(car);

        public void Clear() => _released.Clear();
    }
}
