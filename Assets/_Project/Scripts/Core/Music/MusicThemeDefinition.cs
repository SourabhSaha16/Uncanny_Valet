using System.Collections.Generic;
using UnityEngine;

namespace UncannyValet.Core.Music
{
    /// <summary>
    /// A lot's music as data: tempo, key, the tune (two bars on a 16th grid), the chords and the instruments, plus the last-30-seconds
    /// escalation. Audio renders it with its synth; an assigned <see cref="ClipOverride"/> plays instead. Read-only at runtime.
    /// </summary>
    [CreateAssetMenu(menuName = "Uncanny Valet/Music Theme", fileName = "MusicTheme")]
    public sealed class MusicThemeDefinition : ScriptableObject
    {
        /// <summary>Sixteenth-note steps in the two bars of the tune.</summary>
        public const int Steps = 32;
        /// <summary>Chords per loop (one every half bar).</summary>
        public const int ChordCount = 4;
        /// <summary>The melody value of a silent step.</summary>
        public const int Rest = -100;
        /// <summary>Slowest tempo a theme may have.</summary>
        public const float MinTempo = 50f;
        /// <summary>Fastest tempo a theme may have.</summary>
        public const float MaxTempo = 200f;

        [SerializeField, Range(MinTempo, MaxTempo), Tooltip("Loop tempo in beats per minute.")]
        private float _tempoBpm = 90f;
        [SerializeField, Range(36, 72), Tooltip("MIDI note of the key's root. 57 = A3.")]
        private int _rootMidiNote = 57;
        [SerializeField, Range(0f, 0.5f), Tooltip("Delay of off-beat 16ths as a fraction of a 16th. 0 is straight.")]
        private float _swing;
        [SerializeField, Tooltip("Chords are minor when ticked, major otherwise.")]
        private bool _minor;
        [SerializeField, Tooltip("The tune: 32 sixteenth-note steps, semitones above the root, -100 for a silent step.")]
        private int[] _melody = new int[Steps];
        [SerializeField, Tooltip("The chord under each half bar (4 values): the chord's root in semitones above the key's root.")]
        private int[] _chordRoots = new int[ChordCount];
        [SerializeField, Tooltip("The instruments and their levels.")]
        private List<ThemeVoiceMix> _voices = new List<ThemeVoiceMix>();
        [SerializeField, Range(0f, 1f), Tooltip("Level of this theme's music (on top of the Audio config music volume).")]
        private float _musicVolume = 1f;
        [SerializeField, Range(0f, 1f), Tooltip("Level of the lot's ambience set (on top of the F2 Ambience volume row).")]
        private float _ambienceVolume = 0.8f;
        [SerializeField, Range(0f, 0.5f), Tooltip("Tempo rise for the last 30 seconds: 0.15 = 15% faster.")]
        private float _escalationTempoRise = 0.15f;
        [SerializeField, Tooltip("The layer added for the last 30 seconds.")]
        private EscalationLayer _escalationLayer;
        [SerializeField, Range(0f, 1f), Tooltip("Level of the escalation layer (on top of the F2 Escalation volume row).")]
        private float _escalationVolume = 0.7f;
        [SerializeField, Range(0f, 30f), Tooltip("Seconds the escalation layer takes to fade in to full level.")]
        private float _escalationRampSeconds = 2f;
        [SerializeField, Tooltip("A real recording. When set it plays instead of the generated loop.")]
        private AudioClip _clipOverride;

        public float TempoBpm => _tempoBpm;
        public int RootMidiNote => _rootMidiNote;
        public float Swing => _swing;
        public bool Minor => _minor;
        public IReadOnlyList<int> Melody => _melody;
        public IReadOnlyList<int> ChordRoots => _chordRoots;
        public IReadOnlyList<ThemeVoiceMix> Voices => _voices;
        public float MusicVolume => _musicVolume;
        public float AmbienceVolume => _ambienceVolume;
        public float EscalationTempoRise => _escalationTempoRise;
        public EscalationLayer EscalationLayer => _escalationLayer;
        public float EscalationVolume => _escalationVolume;
        public float EscalationRampSeconds => _escalationRampSeconds;
        public AudioClip ClipOverride => _clipOverride;

        /// <summary>Checks the asset; on failure <paramref name="problem"/> says what is wrong.</summary>
        public bool TryValidate(out string problem)
        {
            problem = MusicThemeRules.Check(this);
            return problem == null;
        }
    }
}
