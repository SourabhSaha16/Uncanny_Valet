using System;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Cars
{
    /// <summary>Forwards the handling keys of the F2 panel to the live handling numbers, and applies the stored values at start.</summary>
    public sealed class CarHandlingTuningLink : IDisposable
    {
        private readonly IEventBus _bus;
        private readonly CarHandlingLive _live;

        /// <param name="tuning">When given, its current handling values are applied first (the panel may have changed them before the cars exist).</param>
        public CarHandlingTuningLink(IEventBus bus, ITuning tuning, CarHandlingLive live)
        {
            _bus = bus;
            _live = live;
            if (tuning != null) Apply(tuning);
            _bus.Subscribe<TuningChanged>(OnChanged);
        }

        /// <summary>Stops forwarding.</summary>
        public void Dispose() => _bus.Unsubscribe<TuningChanged>(OnChanged);

        private void OnChanged(TuningChanged e) => _live.TrySet(e.Key, e.Value);

        private void Apply(ITuning tuning)
        {
            foreach (var row in HandlingRows.Rows)
                if (tuning.TryGet(row.Key, out var value)) _live.TrySet(row.Key, value);
        }
    }
}
