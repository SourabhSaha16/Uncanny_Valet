using System;
using UnityEngine;

namespace UncannyValet.Core.Music
{
    /// <summary>One instrument of a theme and how loud it is in the mix.</summary>
    [Serializable]
    public struct ThemeVoiceMix
    {
        [SerializeField] private ThemeVoice _voice;
        [SerializeField, Range(0f, 1f), Tooltip("Level of this instrument in the theme.")]
        private float _volume;

        public ThemeVoiceMix(ThemeVoice voice, float volume) { _voice = voice; _volume = volume; }

        public ThemeVoice Voice => _voice;
        public float Volume => _volume;
    }
}
