using System;

namespace UncannyValet.UI.Hud
{
    /// <summary>Pure fade of the freeze time look: 0 = off, 1 = fully on. It eases toward the target in real time, linearly.</summary>
    public sealed class FreezeFade
    {
        private readonly float _fadeInSeconds;
        private readonly float _fadeOutSeconds;

        public FreezeFade(float fadeInSeconds, float fadeOutSeconds)
        {
            _fadeInSeconds = Math.Max(0f, fadeInSeconds);
            _fadeOutSeconds = Math.Max(0f, fadeOutSeconds);
        }

        /// <summary>True while the freeze is on, so the look fades in.</summary>
        public bool Frozen { get; set; }

        /// <summary>How strongly the look shows, 0 to 1.</summary>
        public float Alpha01 { get; private set; }

        /// <summary>Moves the fade by <paramref name="dt"/> real seconds.</summary>
        public void Tick(float dt)
        {
            if (dt <= 0f) return;
            var seconds = Frozen ? _fadeInSeconds : _fadeOutSeconds;
            var step = seconds <= 0f ? 1f : dt / seconds;
            Alpha01 = Frozen ? Math.Min(1f, Alpha01 + step) : Math.Max(0f, Alpha01 - step);
        }

        /// <summary>Drops the look at once (a new shift).</summary>
        public void Clear()
        {
            Frozen = false;
            Alpha01 = 0f;
        }
    }
}
