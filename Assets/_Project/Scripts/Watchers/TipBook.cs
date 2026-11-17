using System.Collections.Generic;
using UncannyValet.Core.Watchers;

namespace UncannyValet.Watchers
{
    /// <summary>
    /// Pure bookkeeping of a watcher's tips: each tip is said at most once, a tip is only picked when its situation matches, and at least
    /// a gap of seconds separates two tips. A tip counts as used when it was really shown (<see cref="MarkShown"/>), so one that lost to a
    /// higher priority bubble stays available.
    /// </summary>
    public sealed class TipBook
    {
        private readonly IReadOnlyList<WatcherTip> _tips;
        private readonly bool[] _used;
        private readonly float _gapSeconds;
        private float _lastTipAt = float.NegativeInfinity;

        public TipBook(IReadOnlyList<WatcherTip> tips, float gapSeconds)
        {
            _tips = tips;
            _used = new bool[tips.Count];
            _gapSeconds = gapSeconds;
        }

        /// <summary>The first unused tip of the situation, when the gap since the last shown tip is over.</summary>
        public bool TryPick(TipSituation situation, float now, out int index)
        {
            index = -1;
            if (now - _lastTipAt < _gapSeconds) return false;
            for (var i = 0; i < _tips.Count; i++)
            {
                if (_used[i] || _tips[i].Situation != situation) continue;
                index = i;
                return true;
            }
            return false;
        }

        /// <summary>The tip at <paramref name="index"/> was shown at <paramref name="now"/>.</summary>
        public void MarkShown(int index, float now)
        {
            _used[index] = true;
            _lastTipAt = now;
        }

        /// <summary>The text of a tip.</summary>
        public string TextOf(int index) => _tips[index].Text;

        /// <summary>True once the tip was shown.</summary>
        public bool IsUsed(int index) => _used[index];
    }
}
