using UnityEngine;

namespace UncannyValet.Audio
{
    /// <summary>Volumes and music settings for the audio director. Read-only at runtime.</summary>
    [CreateAssetMenu(menuName = "Uncanny Valet/Audio Config", fileName = "AudioConfig")]
    public sealed class AudioConfig : ScriptableObject
    {
        [Header("Volume")]
        [SerializeField, Range(0f, 1f), Tooltip("Scales everything.")]
        private float _masterVolume = 1f;

        [SerializeField, Range(0f, 1f), Tooltip("Music loop volume. Keep it low so effects stay readable.")]
        private float _musicVolume = 0.2f;

        [SerializeField, Range(0f, 1f), Tooltip("Sound effect volume.")]
        private float _sfxVolume = 0.4f;

        [Header("Music")]
        [SerializeField, Range(70f, 160f), Tooltip("Loop tempo in beats per minute.")]
        private float _tempoBpm = 104f;

        [SerializeField, Range(28, 60), Tooltip("MIDI note of the scale root. 45 = A2.")]
        private int _rootMidiNote = 45;

        [SerializeField, Range(0f, 0.5f), Tooltip("Delay of off-beat 16ths as a fraction of a 16th. 0 is straight.")]
        private float _swing = 0.15f;

        [Header("Urgency")]
        [SerializeField, Range(0.5f, 1.5f), Tooltip("Tempo multiplier with no cars waiting. 1 = normal speed.")]
        private float _minTempoMultiplier = 1f;

        [SerializeField, Range(1f, 1.6f), Tooltip("Tempo multiplier at full urgency. The loop is sped up by pitch, so 1.2 is about three semitones.")]
        private float _maxTempoMultiplier = 1.2f;

        [SerializeField, Range(1, 10), Tooltip("Cars waiting (to be parked or handed back) that reach the maximum tempo.")]
        private int _pendingForMaxTempo = 4;

        [SerializeField, Range(0f, 10f), Tooltip("Seconds for the tempo to close about two thirds of the gap to its target.")]
        private float _tempoSmoothingSeconds = 2.5f;

        [Header("Tyres")]
        [SerializeField, Range(0f, 1f), Tooltip("Squeal loop volume at the gentlest slide, as a share of the effect volume.")]
        private float _squealMinVolume = 0.25f;

        [SerializeField, Range(0f, 1f), Tooltip("Squeal loop volume at a full drift, as a share of the effect volume.")]
        private float _squealMaxVolume = 0.8f;

        [SerializeField, Range(0.5f, 2f), Tooltip("Squeal pitch multiplier at the gentlest slide.")]
        private float _squealMinPitch = 0.9f;

        [SerializeField, Range(0.5f, 2.5f), Tooltip("Squeal pitch multiplier at a full drift.")]
        private float _squealMaxPitch = 1.5f;

        [SerializeField, Range(0.01f, 1f), Tooltip("Seconds the squeal takes to fade out when grip returns.")]
        private float _squealFadeSeconds = 0.15f;

        [SerializeField, Range(0.005f, 0.5f), Tooltip("Seconds the squeal takes to come in when a slide starts.")]
        private float _squealAttackSeconds = 0.03f;

        [Header("Synthesis")]
        [SerializeField, Range(11025, 44100), Tooltip("Sample rate of generated clips. 22050 keeps Web generation cheap.")]
        private int _sampleRate = 22050;

        [SerializeField, Range(2, 16), Tooltip("Number of sound effects that can play at once.")]
        private int _sfxVoices = 6;

        public float MasterVolume => _masterVolume;
        public float MusicVolume => _musicVolume;
        public float SfxVolume => _sfxVolume;
        public float TempoBpm => _tempoBpm;
        public int RootMidiNote => _rootMidiNote;
        public float Swing => _swing;
        public float MinTempoMultiplier => _minTempoMultiplier;
        public float MaxTempoMultiplier => _maxTempoMultiplier;
        public int PendingForMaxTempo => _pendingForMaxTempo;
        public float TempoSmoothingSeconds => _tempoSmoothingSeconds;
        public float SquealMinVolume => _squealMinVolume;
        public float SquealMaxVolume => _squealMaxVolume;
        public float SquealMinPitch => _squealMinPitch;
        public float SquealMaxPitch => _squealMaxPitch;
        public float SquealFadeSeconds => _squealFadeSeconds;
        public float SquealAttackSeconds => _squealAttackSeconds;
        public int SampleRate => _sampleRate;
        public int SfxVoices => _sfxVoices;
    }
}
