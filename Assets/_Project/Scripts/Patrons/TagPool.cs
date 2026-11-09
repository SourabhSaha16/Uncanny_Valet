using System;
using UncannyValet.Core.Config;

namespace UncannyValet.Patrons
{
    /// <summary>
    /// Hands out key-tag colour indices: the lowest index not in use by an open ticket. When every colour is
    /// already taken, the least-used colour is shared (not expected with the waiting cap).
    /// </summary>
    public sealed class TagPool
    {
        private readonly int[] _uses = new int[TagPalette.Count];

        /// <summary>Takes the lowest free tag; if none is free, the least-used one.</summary>
        public int Acquire()
        {
            var best = 0;
            for (var i = 1; i < _uses.Length; i++)
                if (_uses[i] < _uses[best]) best = i;
            _uses[best]++;
            return best;
        }

        /// <summary>Frees one use of a tag. Unknown indices are ignored.</summary>
        public void Release(int tag)
        {
            if (tag >= 0 && tag < _uses.Length && _uses[tag] > 0) _uses[tag]--;
        }

        /// <summary>Frees every tag.</summary>
        public void Reset() => Array.Clear(_uses, 0, _uses.Length);
    }
}
