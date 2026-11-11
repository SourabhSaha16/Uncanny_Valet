using System;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Director
{
    /// <summary>
    /// Applies <see cref="DifficultyDials"/> whenever a dial changes: it listens for TuningChanged, and the derived
    /// writes it makes are ignored (re-entrancy guard), so there is no feedback loop. Hand-edits to a derived key
    /// stick until the next dial move.
    /// </summary>
    public sealed class DifficultyDialsLink : IDisposable
    {
        private readonly IEventBus _bus;
        private readonly ITuning _tuning;
        private readonly DifficultyDials _dials;
        private bool _applying;

        /// <summary>Captures the starting values (the keys must already be seeded) and applies the dials once.</summary>
        public DifficultyDialsLink(IEventBus bus, ITuning tuning, DifficultyDials dials)
        {
            _bus = bus; _tuning = tuning; _dials = dials;
            _dials.CaptureBaselines(tuning);
            Apply();
            _bus.Subscribe<TuningChanged>(OnChanged);
        }

        /// <summary>Stops listening.</summary>
        public void Dispose() => _bus.Unsubscribe<TuningChanged>(OnChanged);

        private void OnChanged(TuningChanged e)
        {
            if (!_applying && DifficultyDials.IsDial(e.Key)) Apply();
        }

        private void Apply()
        {
            _applying = true;
            try { _dials.Apply(_tuning); }
            finally { _applying = false; }
        }
    }
}
