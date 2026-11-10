using System;
using UncannyValet.Core.Events;
using UnityEngine;

namespace UncannyValet.UI
{
    /// <summary>
    /// Remembers the last time a driven car was refused entry to a taken station (StationEntryRefused) and says whether
    /// the OCCUPIED hint should still show: while refusals keep coming, and for <see cref="HoldSeconds"/> after the last.
    /// </summary>
    public sealed class OccupiedHintTracker : IDisposable
    {
        /// <summary>How long the hint stays after the last refusal.</summary>
        public const float HoldSeconds = 1f;

        private readonly IEventBus _bus;
        private readonly Func<float> _clock;
        private float _lastRefusalAt = float.NegativeInfinity;

        public OccupiedHintTracker(IEventBus bus, Func<float> clock)
        {
            _bus = bus;
            _clock = clock;
            _bus.Subscribe<StationEntryRefused>(OnRefused);
        }

        /// <summary>World position of the refused station (valid once <see cref="IsVisible"/> has been true).</summary>
        public Vector2 Anchor { get; private set; }

        /// <summary>True from a refusal until <see cref="HoldSeconds"/> after the latest one.</summary>
        public bool IsVisible => _clock() - _lastRefusalAt <= HoldSeconds;

        public void Dispose() => _bus.Unsubscribe<StationEntryRefused>(OnRefused);

        private void OnRefused(StationEntryRefused e)
        {
            _lastRefusalAt = _clock();
            Anchor = new Vector2(e.WorldX, e.WorldY);
        }
    }
}
