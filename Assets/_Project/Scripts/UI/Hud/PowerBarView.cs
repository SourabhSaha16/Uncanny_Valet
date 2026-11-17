using System;

namespace UncannyValet.UI.Hud
{
    /// <summary>
    /// Pure model of the power bar HUD: the value and open buckets it was told, the smoothed fill it shows, the pulse
    /// when the bar gains a drop and the flash of the bolt that was just spent.
    /// </summary>
    public sealed class PowerBarView
    {
        private const float FullValue = 100f;

        private readonly float _smoothing;
        private readonly float _pulseSeconds;
        private readonly float _flashSeconds;
        private float _target;
        private float _pulse;
        private float _flash;

        public PowerBarView(float smoothing, float pulseSeconds, float flashSeconds)
        {
            _smoothing = smoothing;
            _pulseSeconds = pulseSeconds;
            _flashSeconds = flashSeconds;
        }

        /// <summary>Buckets open, 0 to 3.</summary>
        public int OpenBuckets { get; private set; }

        /// <summary>The fill shown, 0 to 1, easing toward the target.</summary>
        public float Fill { get; private set; }

        /// <summary>The bucket (1 to 3) that was spent last; meaningful while <see cref="Flash01"/> is above 0.</summary>
        public int SpentBucket { get; private set; }

        /// <summary>1 right after the bar gained, fading to 0.</summary>
        public float Pulse01 => _pulseSeconds <= 0f ? 0f : _pulse / _pulseSeconds;

        /// <summary>1 right after a bucket was spent, fading to 0.</summary>
        public float Flash01 => _flashSeconds <= 0f ? 0f : _flash / _flashSeconds;

        /// <summary>Takes a PowerBarChanged reading.</summary>
        public void Set(float value, int openBuckets, int spentBucket)
        {
            var target = Math.Min(Math.Max(value / FullValue, 0f), 1f);
            if (target > _target) _pulse = _pulseSeconds;
            if (spentBucket > 0)
            {
                SpentBucket = spentBucket;
                _flash = _flashSeconds;
            }
            _target = target;
            OpenBuckets = Math.Min(Math.Max(openBuckets, 0), 3);
        }

        /// <summary>Advances the animation by the given seconds.</summary>
        public void Tick(float dt)
        {
            if (dt <= 0f) return;
            Fill += (_target - Fill) * Math.Min(1f, dt * _smoothing);
            if (Math.Abs(_target - Fill) < 0.002f) Fill = _target;
            _pulse = Math.Max(0f, _pulse - dt);
            _flash = Math.Max(0f, _flash - dt);
        }
    }
}
