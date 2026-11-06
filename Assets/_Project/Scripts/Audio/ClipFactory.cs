using UnityEngine;

namespace UncannyValet.Audio
{
    /// <summary>Turns synthesised samples into an in-memory mono <see cref="AudioClip"/>.</summary>
    public static class ClipFactory
    {
        public static AudioClip Create(string name, float[] samples, int sampleRate, bool stream)
        {
            var clip = AudioClip.Create(name, samples.Length, 1, sampleRate, stream);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
