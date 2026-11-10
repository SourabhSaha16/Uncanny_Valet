using UncannyValet.Core.Contracts;

namespace UncannyValet.Traffic
{
    /// <summary>
    /// Owns the pooled entry and exit trips and hands out trip ids. Implements the three small trip interfaces so
    /// callers depend on only what they use.
    /// </summary>
    public sealed class TripRouter : IEntryRouter, IExitRouter, ITripDriver
    {
        private readonly EntryTrip[] _entries;
        private readonly ExitTrip[] _exits;
        private int _nextId = 1;

        /// <param name="context">Shared road state.</param>
        /// <param name="capacity">How many entry trips and how many exit trips may run at once.</param>
        public TripRouter(TripContext context, int capacity)
        {
            _entries = new EntryTrip[capacity];
            _exits = new ExitTrip[capacity];
            for (int i = 0; i < capacity; i++)
            {
                _entries[i] = new EntryTrip(context);
                _exits[i] = new ExitTrip(context);
            }
        }

        /// <inheritdoc />
        public int BeginEntry(int slot)
        {
            for (int i = 0; i < _entries.Length; i++)
            {
                if (_entries[i].Id != TripId.None) continue;
                _entries[i].Begin(_nextId, slot);
                return _nextId++;
            }
            return TripId.None;
        }

        /// <inheritdoc />
        public int BeginExit()
        {
            for (int i = 0; i < _exits.Length; i++)
            {
                if (_exits[i].Id != TripId.None) continue;
                _exits[i].Begin(_nextId);
                return _nextId++;
            }
            return TripId.None;
        }

        /// <inheritdoc />
        public bool Step(int tripId, float dt, out TripPose pose)
        {
            ITrip trip = Find(tripId);
            if (trip == null)
            {
                pose = default;
                return false;
            }
            trip.Step(dt, out pose);
            return true;
        }

        /// <inheritdoc />
        public void Cancel(int tripId) => Find(tripId)?.Cancel();

        private ITrip Find(int tripId)
        {
            if (tripId == TripId.None) return null;
            for (int i = 0; i < _entries.Length; i++)
            {
                if (_entries[i].Id == tripId) return _entries[i];
                if (_exits[i].Id == tripId) return _exits[i];
            }
            return null;
        }
    }
}
