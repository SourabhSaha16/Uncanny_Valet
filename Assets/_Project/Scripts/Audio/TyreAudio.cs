using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UnityEngine;

namespace UncannyValet.Audio
{
    /// <summary>
    /// Plays the squeal loop on one dedicated AudioSource: polls <see cref="ISlipSource"/> (registered by Cars), lets
    /// <see cref="SquealMix"/> map it to volume and pitch, and starts or stops the source. No allocation per frame.
    /// </summary>
    public sealed class TyreAudio
    {
        private readonly AudioSource _source;
        private readonly SquealMix _mix;
        private ISlipSource _slip;
        private float _gain;

        public TyreAudio(AudioSource source, AudioClip loop, SquealMix mix)
        {
            _source = source;
            _mix = mix;
            _source.clip = loop;
            _source.loop = true;
            _source.playOnAwake = false;
            _source.volume = 0f;
        }

        /// <summary>Swaps the loop clip (new sharpness), keeping it playing if it was, and frees the old clip.</summary>
        public void SetLoop(AudioClip clip)
        {
            var old = _source.clip;
            var wasPlaying = _source.isPlaying;
            _source.Stop();
            _source.clip = clip;
            if (wasPlaying) _source.Play();
            if (old != null) Object.Destroy(old);
        }

        /// <summary>The overall effect volume (master x effects, 0 when muted) the loop volume is scaled by.</summary>
        public void SetGain(float gain) => _gain = gain;

        /// <summary>Advances the mix by <paramref name="dt"/> seconds. While <paramref name="paused"/> the loop fades out.</summary>
        public void Tick(float dt, bool paused)
        {
            if (_slip == null) Services.TryGet(out _slip);
            var sliding = !paused && _slip != null && _slip.IsSliding;
            _mix.Step(sliding, _slip != null ? _slip.Intensity01 : 0f, dt);
            _source.volume = _mix.Volume * _gain;
            _source.pitch = _mix.Pitch;
            if (_mix.Volume <= 0f)
            {
                if (_source.isPlaying) _source.Stop();
            }
            else if (!_source.isPlaying) _source.Play();
        }
    }
}
