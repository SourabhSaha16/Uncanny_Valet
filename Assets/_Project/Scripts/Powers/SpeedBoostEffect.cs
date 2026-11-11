using UncannyValet.Core.Events;

namespace UncannyValet.Powers
{
    /// <summary>Level 1: the valet walks faster for a few seconds. Holds the state; the Valet module adopts it later.</summary>
    public sealed class SpeedBoostEffect : IPowerEffect
    {
        private readonly PowerSettings _settings;
        private readonly PowerTimer _timer = new PowerTimer();

        public SpeedBoostEffect(PowerSettings settings) { _settings = settings; }

        public PowerKind Kind => PowerKind.SpeedBoost;
        public bool IsActive => _timer.IsRunning;

        /// <summary>Seconds of boost left.</summary>
        public float Remaining => _timer.Remaining;

        /// <summary>Walk speed multiplier right now: 1 when inactive, the boost value while active (never stacks with itself).</summary>
        public float WalkMultiplier => IsActive ? _settings.WalkMultiplier : 1f;

        public void Activate() => _timer.Start(_settings.SpeedBoostSeconds);
        public void Tick(float deltaSeconds) => _timer.Tick(deltaSeconds);
        public void End() => _timer.Stop();
    }
}
