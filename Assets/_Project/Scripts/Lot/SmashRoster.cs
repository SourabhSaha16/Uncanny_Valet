using System.Collections.Generic;
using UncannyValet.Core.Ids;

namespace UncannyValet.Lot
{
    /// <summary>One car Park Smash may park, with the traits that decide which bay it gets.</summary>
    public readonly struct SmashCandidate
    {
        public readonly CarId Car;
        public readonly bool IsEv;
        public readonly bool IsDirty;
        public SmashCandidate(CarId car, bool isEv, bool isDirty) { Car = car; IsEv = isEv; IsDirty = isDirty; }
    }

    /// <summary>
    /// Pure book-keeping of the cars that have arrived and wait for a bay, in the order they arrived, plus the EV and
    /// dirty traits of every open ticket. A car leaves the waiting list when it parks, its patron asks for it, or its ticket closes.
    /// </summary>
    public sealed class SmashRoster
    {
        private readonly List<CarId> _waiting = new List<CarId>();
        private readonly Dictionary<CarId, (bool ev, bool dirty)> _traits = new Dictionary<CarId, (bool ev, bool dirty)>();

        public int Count => _waiting.Count;

        /// <summary>Remembers the traits of a new ticket's car (published before the car arrives).</summary>
        public void Traits(CarId car, bool isEv, bool isDirty) => _traits[car] = (isEv, isDirty);

        /// <summary>The car finished driving in and now waits for a bay.</summary>
        public void Arrived(CarId car)
        {
            if (!_waiting.Contains(car)) _waiting.Add(car);
        }

        /// <summary>The car no longer waits (parked, or its patron asked for it); its traits are kept until the ticket closes.</summary>
        public void Remove(CarId car) => _waiting.Remove(car);

        /// <summary>The ticket closed: forget the car completely.</summary>
        public void Closed(CarId car)
        {
            _waiting.Remove(car);
            _traits.Remove(car);
        }

        /// <summary>Forgets everything (a shift edge).</summary>
        public void Clear()
        {
            _waiting.Clear();
            _traits.Clear();
        }

        /// <summary>Fills <paramref name="into"/> with every waiting car in arrival order, as candidates.</summary>
        public void CopyTo(List<SmashCandidate> into)
        {
            into.Clear();
            foreach (var car in _waiting)
            {
                _traits.TryGetValue(car, out var t);
                into.Add(new SmashCandidate(car, t.ev, t.dirty));
            }
        }
    }
}
