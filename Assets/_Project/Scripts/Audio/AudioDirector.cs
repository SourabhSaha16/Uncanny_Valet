using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Modifiers;
using UncannyValet.Core.Music;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UncannyValet.Audio
{
    /// <summary>
    /// Plays synthesised music and effects. Add one to the Bootstrap scene; assign an <see cref="AudioConfig"/>
    /// (a default is used, with one warning, if none is set). M toggles mute. Needs IEventBus in Services.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AudioDirector : MonoBehaviour
    {
        [SerializeField] private AudioConfig _config;

        /// <summary>The assigned config (seeds the tyre sound F2 keys); null if none is assigned.</summary>
        public AudioConfig Config => _config;

        private readonly MuteState _mute = new MuteState();
        private SfxEventRouter _router;
        private PendingCarTracker _pending;
        private UrgencyRule _urgency;
        private SfxBank _bank;
        private TyreAudio _tyres;
        private TyreSoundTuning _tyreTuning;
        private WorldSoundTuning _worldTuning;
        private ThemeSession _session;
        private MusicThemeDefinition _theme;
        private float _sfxGain;
        private AudioSource _music;
        private SourceLayer _musicLayer;
        private readonly LayerSet _layers = new LayerSet();
        private AudioSource[] _voices;
        private int _nextVoice;

        private void Start()
        {
            if (!Services.TryGet<IEventBus>(out var bus))
            {
                Debug.LogError("AudioDirector: IEventBus is not registered in Services; audio events are off.", this);
                return;
            }
            if (_config == null)
            {
                Debug.LogWarning("AudioDirector: no AudioConfig assigned; using defaults.", this);
                _config = ScriptableObject.CreateInstance<AudioConfig>();
            }
            _tyreTuning = new TyreSoundTuning(bus, _config.TyreVolume, _config.TyreSharpness, OnTyreTuned);
            _worldTuning = new WorldSoundTuning(bus, _config.AmbienceVolume, _config.EscalationVolume, _config.WorldSoundVolume, ApplyVolumes);
            if (Services.TryGet<ITuning>(out var tuning))
            {
                _tyreTuning.Load(tuning);
                _worldTuning.Load(tuning);
            }
            Services.TryGet<ILotAudio>(out var lotAudio);
            _theme = lotAudio != null ? lotAudio.Theme : null;
            BuildAudio();
            _session = new ThemeSession(bus, gameObject, _layers, _theme, lotAudio != null ? lotAudio.Ambience : null,
                _config.SampleRate, _config.TempoSmoothingSeconds, _worldTuning);
            ApplyVolumes();
            Services.TryGet<IModifierCatalog>(out var modifiers);
            _router = new SfxEventRouter(bus, Play, () => Time.unscaledTime, modifiers, lotAudio);
            _pending = new PendingCarTracker(bus);
            _urgency = new UrgencyRule(_config.MinTempoMultiplier, _config.MaxTempoMultiplier,
                _config.PendingForMaxTempo, _config.TempoSmoothingSeconds);
            if (!isActiveAndEnabled) return;
            _router.Subscribe();
            _pending.Subscribe();
            _tyreTuning.Subscribe();
            _worldTuning.Subscribe();
            _session.Subscribe();
        }

        private void OnEnable()
        {
            _router?.Subscribe();
            _pending?.Subscribe();
            _tyreTuning?.Subscribe();
            _worldTuning?.Subscribe();
            _session?.Subscribe();
        }

        private void OnDisable()
        {
            _router?.Unsubscribe();
            _pending?.Unsubscribe();
            _tyreTuning?.Unsubscribe();
            _worldTuning?.Unsubscribe();
            _session?.Unsubscribe();
        }

        // Level audio never outlives its scene: every layer is stopped, muted and released when the director goes.
        private void OnDestroy() => _layers.SilenceAll();

        private void Update()
        {
            if (_music == null) return;
            _tyres.Tick(Time.unscaledDeltaTime, Time.timeScale <= 0f);
            var escalated = _session.Tick(Time.unscaledDeltaTime);
            _music.pitch = _theme != null ? escalated : _urgency.Step(_pending.Count, Time.unscaledDeltaTime);
            var keyboard = Keyboard.current;
            if (keyboard == null || !keyboard.mKey.wasPressedThisFrame) return;
            _mute.Toggle();
            ApplyVolumes();
        }

        private void BuildAudio()
        {
            var settings = new LoopSettings(_config.SampleRate, _config.TempoBpm, _config.RootMidiNote, _config.Swing);
            _layers.SilenceAll();
            _music = gameObject.AddComponent<AudioSource>();
            var clip = MusicClip(settings);
            _musicLayer = new SourceLayer(_music, clip, _theme != null && _theme.ClipOverride == clip ? null : clip);
            _layers.Hold(_musicLayer);
            _bank = new SfxBank(settings.SampleRate, _tyreTuning.Sharpness);
            _voices = new AudioSource[_config.SfxVoices];
            for (int i = 0; i < _voices.Length; i++)
            {
                _voices[i] = gameObject.AddComponent<AudioSource>();
                _voices[i].playOnAwake = false;
            }
            var squeal = new SquealMix(_config.SquealMinVolume, _config.SquealMaxVolume, _config.SquealMinPitch,
                _config.SquealMaxPitch, _config.SquealFadeSeconds, _config.SquealAttackSeconds);
            _tyres = new TyreAudio(gameObject.AddComponent<AudioSource>(), SquealClip(), squeal);
            ApplyVolumes();
            _musicLayer.Play();
        }

        // The theme's clip override, else the theme rendered by the synth; no theme keeps the funk loop.
        private AudioClip MusicClip(in LoopSettings funk)
        {
            if (_theme == null) return ClipFactory.Create("FunkLoop", FunkLoopBuilder.Build(funk), funk.SampleRate, false);
            if (_theme.ClipOverride != null) return _theme.ClipOverride;
            return ClipFactory.Create(_theme.name, ThemeLoopBuilder.Build(_theme, funk.SampleRate), funk.SampleRate, false);
        }

        private AudioClip SquealClip()
            => ClipFactory.Create("SquealLoop", SquealLoop.Build(_config.SampleRate, _tyreTuning.Sharpness), _config.SampleRate, false);

        // The panel moved a tyre sound key: re-render the tyre clips with the new sharpness and apply the volume.
        private void OnTyreTuned()
        {
            if (_bank == null) return;
            _bank.SetTyreSharpness(_tyreTuning.Sharpness);
            _tyres.SetLoop(SquealClip());
            ApplyVolumes();
        }

        private void ApplyVolumes()
        {
            float master = _mute.Apply(_config.MasterVolume);
            _musicLayer.SetVolume(master * _config.MusicVolume * (_theme != null ? _theme.MusicVolume : 1f));
            _session?.SetGain(master * _config.SfxVolume);
            _sfxGain = master * _config.SfxVolume;
            for (int i = 0; i < _voices.Length; i++) _voices[i].volume = _sfxGain;
            _tyres.SetGain(_sfxGain * _tyreTuning.Volume);
        }

        private void Play(SfxId id)
        {
            var source = _voices[_nextVoice];
            _nextVoice = (_nextVoice + 1) % _voices.Length;
            var tyre = id == SfxId.TyreChirp || id == SfxId.TyreScreech;
            var world = id == SfxId.BlessingChime || id == SfxId.WashSplash || id == SfxId.MudSquelch;
            source.volume = tyre ? _sfxGain * _tyreTuning.Volume * _config.TyreOneShotVolume
                : world ? _sfxGain * _worldTuning.WorldSound : _sfxGain;
            source.clip = _bank.Get(id);
            source.Play();
        }
    }
}
