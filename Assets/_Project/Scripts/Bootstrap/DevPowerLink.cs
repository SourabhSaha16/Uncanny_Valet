using System;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;

namespace UncannyValet.Bootstrap
{
    /// <summary>
    /// The F2 "Developer: owned powers" rows: turns the level of a power (0 none, 1 earned, 2 upgraded) into PowerGranted or
    /// PowerRevoked on the bus, so the run progress (the real owner) gains, upgrades or loses the power for the session.
    /// At start the rows show what is owned.
    /// </summary>
    public sealed class DevPowerLink : IDisposable
    {
        private readonly IEventBus _bus;
        private readonly IRunProgress _progress;

        public DevPowerLink(IEventBus bus, ITuning tuning, IRunProgress progress)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _progress = progress ?? throw new ArgumentNullException(nameof(progress));
            for (var i = 0; i < PowerLevelRows.All.Count; i++)
                if (PowerRows.TryGetDeveloperKey(PowerLevelRows.All[i], out var key)) tuning.Set(key, progress.PowerLevel(PowerLevelRows.All[i]));
            _bus.Subscribe<TuningChanged>(OnChanged);
        }

        /// <summary>Stops listening to the panel.</summary>
        public void Dispose() => _bus.Unsubscribe<TuningChanged>(OnChanged);

        private void OnChanged(TuningChanged e)
        {
            for (var i = 0; i < PowerLevelRows.All.Count; i++)
            {
                var power = PowerLevelRows.All[i];
                if (PowerRows.TryGetDeveloperKey(power, out var key) && key == e.Key) Apply(power, (int)Math.Round(e.Value));
            }
        }

        private void Apply(StoryPower power, int level)
        {
            level = Math.Max(0, Math.Min(level, PowerTuningKeys.Levels));
            var owned = _progress.PowerLevel(power);
            if (owned == level) return;
            if (owned > level) _bus.Publish(new PowerRevoked(power));
            if (level > 0) _bus.Publish(new PowerGranted(power, level));
        }
    }
}
