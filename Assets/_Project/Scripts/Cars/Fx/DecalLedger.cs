using UnityEngine;

namespace UncannyValet.Cars.Fx
{
    /// <summary>
    /// Pure bookkeeping of a fixed number of fading decals: hands out a free slot, or recycles the oldest when full, and
    /// ages and expires them. <see cref="DecalPool"/> draws what it says. No allocation after construction.
    /// </summary>
    public sealed class DecalLedger
    {
        private readonly float[] _age;
        private readonly float[] _life;
        private readonly float[] _peak;
        private readonly bool[] _active;

        public DecalLedger(int capacity)
        {
            _age = new float[capacity];
            _life = new float[capacity];
            _peak = new float[capacity];
            _active = new bool[capacity];
        }

        /// <summary>Most decals alive at once.</summary>
        public int Capacity => _active.Length;

        /// <summary>Decals alive now.</summary>
        public int ActiveCount { get; private set; }

        /// <summary>Takes a free slot, or the oldest one when all are in use. The decal starts at <paramref name="peakAlpha"/> and fades to 0 over <paramref name="lifeSeconds"/>.</summary>
        public int Acquire(float lifeSeconds, float peakAlpha)
        {
            var slot = -1;
            var oldest = -1f;
            for (var i = 0; i < _active.Length; i++)
            {
                if (!_active[i]) { slot = i; break; }
                if (_age[i] > oldest) { oldest = _age[i]; slot = i; }
            }
            if (!_active[slot]) { _active[slot] = true; ActiveCount++; }
            _age[slot] = 0f;
            _life[slot] = Mathf.Max(lifeSeconds, 0.001f);
            _peak[slot] = peakAlpha;
            return slot;
        }

        /// <summary>Ages every live decal and frees the ones whose time is up.</summary>
        public void Tick(float dt)
        {
            for (var i = 0; i < _active.Length; i++)
            {
                if (!_active[i]) continue;
                _age[i] += dt;
                if (_age[i] < _life[i]) continue;
                _active[i] = false;
                ActiveCount--;
            }
        }

        /// <summary>True while the slot holds a live decal.</summary>
        public bool IsActive(int slot) => _active[slot];

        /// <summary>Current opacity of a live decal: the peak, falling linearly to 0.</summary>
        public float Alpha(int slot) => _peak[slot] * (1f - _age[slot] / _life[slot]);
    }
}
