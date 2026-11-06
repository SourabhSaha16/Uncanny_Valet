using System;
using UnityEngine;

namespace UncannyValet.Audio
{
    /// <summary>Synthesises every sound effect once and keeps the clips. Playing never allocates.</summary>
    public sealed class SfxBank
    {
        private readonly AudioClip[] _clips;

        public SfxBank(int sampleRate)
        {
            var ids = (SfxId[])Enum.GetValues(typeof(SfxId));
            _clips = new AudioClip[ids.Length];
            foreach (SfxId id in ids)
            {
                float[] samples = SfxRecipeBook.Render(id, sampleRate);
                _clips[(int)id] = ClipFactory.Create(id.ToString(), samples, sampleRate, false);
            }
        }

        public AudioClip Get(SfxId id) => _clips[(int)id];
    }
}
