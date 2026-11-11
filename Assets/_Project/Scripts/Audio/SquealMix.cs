using System;

namespace UncannyValet.Audio
{
    /// <summary>
    /// Pure mapping of the slide to the squeal loop's volume and pitch. Volume and pitch rise with the slip intensity;
    /// volume ramps up quickly and fades to silence within <c>fadeSeconds</c> once the slide ends.
    /// </summary>
    public sealed class SquealMix
    {
        private readonly float _minVolume, _maxVolume, _minPitch, _maxPitch, _fadeSeconds, _attackSeconds;

        public SquealMix(float minVolume, float maxVolume, float minPitch, float maxPitch, float fadeSeconds, float attackSeconds)
        {
            _minVolume = minVolume;
            _maxVolume = maxVolume;
            _minPitch = minPitch;
            _maxPitch = maxPitch;
            _fadeSeconds = Math.Max(fadeSeconds, 1e-3f);
            _attackSeconds = Math.Max(attackSeconds, 1e-3f);
            Pitch = minPitch;
        }

        /// <summary>Volume of the loop, 0..max, before the master gain.</summary>
        public float Volume { get; private set; }

        /// <summary>Pitch multiplier of the loop; holds its last value while the volume fades out.</summary>
        public float Pitch { get; private set; }

        /// <summary>Moves volume and pitch toward the targets for the current slide.</summary>
        public void Step(bool sliding, float intensity01, float dt)
        {
            var t = Math.Min(Math.Max(intensity01, 0f), 1f);
            if (sliding)
            {
                Pitch = _minPitch + (_maxPitch - _minPitch) * t;
                var target = _minVolume + (_maxVolume - _minVolume) * t;
                Volume = Toward(Volume, target, _maxVolume / _attackSeconds * dt);
            }
            else Volume = Toward(Volume, 0f, _maxVolume / _fadeSeconds * dt);
        }

        private static float Toward(float value, float target, float maxDelta)
            => Math.Abs(target - value) <= maxDelta ? target : value + Math.Sign(target - value) * maxDelta;
    }
}
