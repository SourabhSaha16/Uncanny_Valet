using System.Collections.Generic;
using UncannyValet.Core.Ids;

namespace UncannyValet.Patrons
{
    /// <summary>
    /// Remembers which arrival slot each car that is still driving in is heading for. Such a car has its collider off,
    /// so the physics scan cannot see it; the reservation keeps the slot taken until the car arrives (then the scan
    /// sees it) or its ticket ends. Cars are ordered by id, which is the order they were sent in.
    /// </summary>
    public sealed class ArrivalReservations
    {
        private readonly Dictionary<CarId, int> _slots = new Dictionary<CarId, int>();
        private readonly List<CarId> _scratch = new List<CarId>();

        public int Count => _slots.Count;

        /// <summary>Marks a slot as taken by a car until it is released.</summary>
        public void Reserve(CarId car, int slot) => _slots[car] = slot;

        /// <summary>Frees the car's slot (it arrived, was parked, or its ticket ended). Returns false if it held none.</summary>
        public bool Release(CarId car) => _slots.Remove(car);

        public void Clear() => _slots.Clear();

        /// <summary>The car that is heading for <paramref name="slot"/>, if any.</summary>
        public bool TryHolder(int slot, out CarId car)
        {
            foreach (var pair in _slots)
            {
                if (pair.Value != slot) continue;
                car = pair.Key;
                return true;
            }
            car = CarId.None;
            return false;
        }

        /// <summary>Sets <paramref name="flags"/>[slot] for every reserved slot of a car sent before <paramref name="before"/>.</summary>
        public void MarkAhead(List<bool> flags, CarId before)
        {
            foreach (var pair in _slots)
                if (pair.Key.Value < before.Value && pair.Value < flags.Count) flags[pair.Value] = true;
        }

        /// <summary>True when any car is heading for <paramref name="slot"/>.</summary>
        public bool IsReserved(int slot) => TryHolder(slot, out _);

        /// <summary>Points the car's reservation at another slot.</summary>
        public void Move(CarId car, int slot)
        {
            if (_slots.ContainsKey(car)) _slots[car] = slot;
        }

        /// <summary>Drops reservations whose ticket is gone or no longer waiting to be parked (nothing leaks).</summary>
        public void Prune(TicketRegistry registry)
        {
            _scratch.Clear();
            foreach (var car in _slots.Keys)
                if (!registry.TryGetByCar(car, out var t) || t.State != TicketState.AwaitingPark) _scratch.Add(car);
            foreach (var car in _scratch) _slots.Remove(car);
        }
    }
}
