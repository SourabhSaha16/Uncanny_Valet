using System;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;

namespace UncannyValet.Bootstrap
{
    /// <summary>
    /// The F2 "Developer: owned powers" rows: turns the switch of a power into PowerGranted or PowerRevoked on the bus, so
    /// the run progress (the real owner) gains or loses the power for the session. At start the rows show what is owned.
    /// </summary>
    public sealed class DevPowerLink : IDisposable
    {
        private const float OnThreshold = 0.5f;

        private readonly IEventBus _bus;
        private readonly IRunProgress _progress;

        public DevPowerLink(IEventBus bus, ITuning tuning, IRunProgress progress)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _progress = progress ?? throw new ArgumentNullException(nameof(progress));
            tuning.Set(TuningKey.DevOwnFreezeTime, progress.PowerLevel(StoryPower.FreezeTime) > 0 ? 1f : 0f);
            _bus.Subscribe<TuningChanged>(OnChanged);
        }

        /// <summary>Stops listening to the panel.</summary>
        public void Dispose() => _bus.Unsubscribe<TuningChanged>(OnChanged);

        private void OnChanged(TuningChanged e)
        {
            if (e.Key == TuningKey.DevOwnFreezeTime) Apply(StoryPower.FreezeTime, e.Value >= OnThreshold);
        }

        private void Apply(StoryPower power, bool owned)
        {
            if (owned) _bus.Publish(new PowerGranted(power, 1));
            else if (_progress.PowerLevel(power) > 0) _bus.Publish(new PowerRevoked(power));
        }
    }
}
