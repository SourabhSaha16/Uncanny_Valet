using System.Collections.Generic;

namespace UncannyValet.Traffic
{
    /// <summary>
    /// Registry of every car on the road: lane, distance along it (centre), speed and length. Answers the gap to the
    /// nearest car ahead or behind. Plain data; allocates only if its starting capacity is exceeded.
    /// </summary>
    public sealed class LaneOccupancy
    {
        private struct Entry
        {
            public int Id;
            public TrafficLane Lane;
            public float Distance;
            public float Speed;
            public float Length;
        }

        private readonly List<Entry> _entries;
        private int _nextId = 1;

        public LaneOccupancy(int capacity)
        {
            _entries = new List<Entry>(capacity);
        }

        /// <summary>Number of registered cars.</summary>
        public int Count => _entries.Count;

        /// <summary>Registers a car and returns its id (never 0).</summary>
        public int Add(TrafficLane lane, float distance, float speed, float length)
        {
            int id = _nextId++;
            _entries.Add(new Entry { Id = id, Lane = lane, Distance = distance, Speed = speed, Length = length });
            return id;
        }

        /// <summary>Updates a car's distance and speed. Unknown ids are ignored.</summary>
        public void Update(int id, float distance, float speed)
        {
            int index = IndexOf(id);
            if (index < 0) return;
            Entry entry = _entries[index];
            entry.Distance = distance;
            entry.Speed = speed;
            _entries[index] = entry;
        }

        /// <summary>Removes a car. Unknown ids are ignored.</summary>
        public void Remove(int id)
        {
            int index = IndexOf(id);
            if (index >= 0) _entries.RemoveAt(index);
        }

        /// <summary>Forgets every car.</summary>
        public void Clear() => _entries.Clear();

        /// <summary>Bumper gap to the nearest car ahead of a car at <paramref name="distance"/> (infinity if none). May be negative on overlap.</summary>
        public float GapAhead(TrafficLane lane, float distance, float length, int excludeId, out float leaderSpeed)
        {
            float best = float.PositiveInfinity;
            leaderSpeed = 0f;
            for (int i = 0; i < _entries.Count; i++)
            {
                Entry e = _entries[i];
                if (e.Lane != lane || e.Id == excludeId || e.Distance <= distance) continue;
                float gap = (e.Distance - e.Length * 0.5f) - (distance + length * 0.5f);
                if (gap >= best) continue;
                best = gap;
                leaderSpeed = e.Speed;
            }
            return best;
        }

        /// <summary>Bumper gap to the nearest car behind (infinity if none).</summary>
        public float GapBehind(TrafficLane lane, float distance, float length, int excludeId, out float followerSpeed)
        {
            float best = float.PositiveInfinity;
            followerSpeed = 0f;
            for (int i = 0; i < _entries.Count; i++)
            {
                Entry e = _entries[i];
                if (e.Lane != lane || e.Id == excludeId || e.Distance >= distance) continue;
                float gap = (distance - length * 0.5f) - (e.Distance + e.Length * 0.5f);
                if (gap >= best) continue;
                best = gap;
                followerSpeed = e.Speed;
            }
            return best;
        }

        private int IndexOf(int id)
        {
            for (int i = 0; i < _entries.Count; i++)
            {
                if (_entries[i].Id == id) return i;
            }
            return -1;
        }
    }
}
