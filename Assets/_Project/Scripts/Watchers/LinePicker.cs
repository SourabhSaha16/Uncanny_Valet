using System;
using System.Collections.Generic;
using UncannyValet.Core.Watchers;

namespace UncannyValet.Watchers
{
    /// <summary>Picks the lines of each event in turn (round robin), so the same line never repeats before the others were said.</summary>
    public sealed class LinePicker
    {
        private readonly int[] _next = new int[Enum.GetValues(typeof(WatcherEvent)).Length];

        /// <summary>The next line for the event; false when the list is empty.</summary>
        public bool TryNext(WatcherEvent watcherEvent, IReadOnlyList<string> lines, out string line)
        {
            line = null;
            if (lines.Count == 0) return false;
            var slot = (int)watcherEvent;
            line = lines[_next[slot] % lines.Count];
            _next[slot] = (_next[slot] + 1) % lines.Count;
            return true;
        }
    }
}
