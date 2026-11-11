using System;

namespace UncannyValet.UI.Hud
{
    /// <summary>
    /// Pure model of the power meter HUD: the level and target fill it was told, the smoothed fill it shows, and the
    /// pulse timers. A rise in fill pulses the bar; reaching a higher level pulses the ready glow.
    /// </summary>
    public sealed class PowerMeterView
    {
        private readonly float _smoothing;
        private readonly float _pulseSeconds;
        private float _target;
        private float _pulse;
        private float _ready;

        public PowerMeterView(float smoothing, float pulseSeconds)
        {
            _smoothing = smoothing;
            _pulseSeconds = pulseSeconds;
        }

        /// <summary>Meter level, 0 to 3.</summary>
        public int Level { get; private set; }

        /// <summary>The fill shown, 0 to 1, easing toward the target.</summary>
        public float Fill { get; private set; }

        /// <summary>1 right after the bar filled, fading to 0.</summary>
        public float Pulse01 => _pulseSeconds <= 0f ? 0f : _pulse / _pulseSeconds;

        /// <summary>1 right after a level was reached, fading to 0.</summary>
        public float Ready01 => _pulseSeconds <= 0f ? 0f : _ready / _pulseSeconds;

        /// <summary>Takes a PowerMeterChanged reading.</summary>
        public void Set(int level, float fill01)
        {
            var clamped = Math.Min(Math.Max(fill01, 0f), 1f);
            var newLevel = Math.Min(Math.Max(level, 0), 3);
            if (clamped > _target) _pulse = _pulseSeconds;
            if (newLevel > Level) _ready = _pulseSeconds;
            _target = clamped;
            Level = newLevel;
        }

        /// <summary>Advances the animation by the given seconds.</summary>
        public void Tick(float dt)
        {
            if (dt <= 0f) return;
            Fill += (_target - Fill) * Math.Min(1f, dt * _smoothing);
            if (Math.Abs(_target - Fill) < 0.002f) Fill = _target;
            _pulse = Math.Max(0f, _pulse - dt);
            _ready = Math.Max(0f, _ready - dt);
        }
    }
}
