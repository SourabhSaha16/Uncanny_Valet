using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Powers
{
    /// <summary>Level 2: world time is scaled down (0 = frozen) for a few seconds. Provides <see cref="IWorldTime"/>.</summary>
    public sealed class FreezeTimeEffect : IPowerEffect, IWorldTime
    {
        private readonly PowerSettings _settings;
        private readonly PowerTimer _timer = new PowerTimer();

        public FreezeTimeEffect(PowerSettings settings) { _settings = settings; }

        public PowerKind Kind => PowerKind.FreezeTime;
        public bool IsActive => _timer.IsRunning;

        /// <summary>Seconds of freeze left.</summary>
        public float Remaining => _timer.Remaining;

        public float Scale => IsActive ? _settings.FreezeTimeScale : 1f;
        public bool IsSlowed => Scale < 1f;

        public void Activate() => _timer.Start(_settings.FreezeSeconds);
        public void Tick(float deltaSeconds) => _timer.Tick(deltaSeconds);
        public void End() => _timer.Stop();
    }
}
