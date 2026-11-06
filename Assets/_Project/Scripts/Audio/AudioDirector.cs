using UncannyValet.Core;
using UncannyValet.Core.Events;
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

        private readonly MuteState _mute = new MuteState();
        private SfxEventRouter _router;
        private SfxBank _bank;
        private AudioSource _music;
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
            BuildAudio();
            _router = new SfxEventRouter(bus, Play);
            if (isActiveAndEnabled) _router.Subscribe();
        }

        private void OnEnable() => _router?.Subscribe();

        private void OnDisable() => _router?.Unsubscribe();

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (_music == null || keyboard == null || !keyboard.mKey.wasPressedThisFrame) return;
            _mute.Toggle();
            ApplyVolumes();
        }

        private void BuildAudio()
        {
            var settings = new LoopSettings(_config.SampleRate, _config.TempoBpm, _config.RootMidiNote, _config.Swing);
            float[] loop = FunkLoopBuilder.Build(settings);
            _music = gameObject.AddComponent<AudioSource>();
            _music.clip = ClipFactory.Create("FunkLoop", loop, settings.SampleRate, false);
            _music.loop = true;
            _music.playOnAwake = false;
            _bank = new SfxBank(settings.SampleRate);
            _voices = new AudioSource[_config.SfxVoices];
            for (int i = 0; i < _voices.Length; i++)
            {
                _voices[i] = gameObject.AddComponent<AudioSource>();
                _voices[i].playOnAwake = false;
            }
            ApplyVolumes();
            _music.Play();
        }

        private void ApplyVolumes()
        {
            float master = _mute.Apply(_config.MasterVolume);
            _music.volume = master * _config.MusicVolume;
            for (int i = 0; i < _voices.Length; i++) _voices[i].volume = master * _config.SfxVolume;
        }

        private void Play(SfxId id)
        {
            var source = _voices[_nextVoice];
            _nextVoice = (_nextVoice + 1) % _voices.Length;
            source.clip = _bank.Get(id);
            source.Play();
        }
    }
}
