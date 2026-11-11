using System;
using UnityEngine;

namespace UncannyValet.Audio
{
    /// <summary>Synthesises every sound effect once and keeps the clips. Playing never allocates.</summary>
    public sealed class SfxBank
    {
        private readonly AudioClip[] _clips;
        private readonly int _sampleRate;

        public SfxBank(int sampleRate, float tyreSharpness = SfxTyres.DefaultSharpness)
        {
            _sampleRate = sampleRate;
            var ids = (SfxId[])Enum.GetValues(typeof(SfxId));
            _clips = new AudioClip[ids.Length];
            foreach (SfxId id in ids) _clips[(int)id] = Render(id, tyreSharpness);
        }

        public AudioClip Get(SfxId id) => _clips[(int)id];

        /// <summary>Re-renders the tyre one-shots with a new sharpness (0..1), replacing the old clips.</summary>
        public void SetTyreSharpness(float sharpness)
        {
            Replace(SfxId.TyreChirp, sharpness);
            Replace(SfxId.TyreScreech, sharpness);
        }

        private void Replace(SfxId id, float sharpness)
        {
            if (_clips[(int)id] != null) UnityEngine.Object.Destroy(_clips[(int)id]);
            _clips[(int)id] = Render(id, sharpness);
        }

        private AudioClip Render(SfxId id, float sharpness)
            => ClipFactory.Create(id.ToString(), SfxRecipeBook.Render(id, _sampleRate, sharpness), _sampleRate, false);
    }
}
