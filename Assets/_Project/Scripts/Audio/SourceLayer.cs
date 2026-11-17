using UnityEngine;

namespace UncannyValet.Audio
{
    /// <summary>An <see cref="IAudioLayer"/> on one AudioSource: plays a looping clip, and on silence stops and empties the source.</summary>
    public sealed class SourceLayer : IAudioLayer
    {
        private readonly AudioSource _source;
        private AudioClip _ownedClip;

        /// <param name="source">The source the layer plays on.</param>
        /// <param name="clip">The loop to play.</param>
        /// <param name="ownedClip">A clip made for this layer; destroyed on silence. Null when the clip is a shared asset.</param>
        public SourceLayer(AudioSource source, AudioClip clip, AudioClip ownedClip)
        {
            _source = source;
            _ownedClip = ownedClip;
            _source.clip = clip;
            _source.loop = true;
            _source.playOnAwake = false;
        }

        public bool IsSilenced { get; private set; }

        /// <summary>Starts the loop from the beginning; ignored once silenced.</summary>
        public void Play()
        {
            if (!IsSilenced) _source.Play();
        }

        /// <summary>Stops the loop without silencing the layer (the escalation layer between shifts).</summary>
        public void Stop()
        {
            if (!IsSilenced) _source.Stop();
        }

        /// <summary>Sets the volume; ignored once silenced.</summary>
        public void SetVolume(float volume)
        {
            if (!IsSilenced) _source.volume = volume;
        }

        public void Silence()
        {
            IsSilenced = true;
            if (_source == null) return;
            _source.Stop();
            _source.volume = 0f;
            _source.clip = null;
            if (_ownedClip != null) Object.Destroy(_ownedClip);
            _ownedClip = null;
        }
    }
}
