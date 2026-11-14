using System.Collections.Generic;
using UncannyValet.Core.Events;
using UncannyValet.Core.Music;
using UnityEngine;

namespace UncannyValet.Audio
{
    /// <summary>
    /// The ambience loops and the last-30-seconds layer of a lot, plus the escalation tempo. The director owns the music source and asks
    /// <see cref="Tick"/> for its pitch each frame.
    /// </summary>
    public sealed class ThemeSession
    {
        private readonly IEventBus _bus;
        private readonly MusicThemeDefinition _theme;
        private readonly AmbienceSetDefinition _ambience;
        private readonly WorldSoundTuning _tuning;
        private readonly EscalationRule _rule;
        private readonly List<AudioSource> _layerSources = new List<AudioSource>();
        private readonly List<float> _layerLevels = new List<float>();
        private readonly AudioSource _escalationSource;
        private float _gain;
        private bool _active;

        /// <param name="theme">May be null (ambience only, funk loop in the director).</param>
        /// <param name="ambience">May be null.</param>
        public ThemeSession(IEventBus bus, GameObject host, MusicThemeDefinition theme, AmbienceSetDefinition ambience,
            int sampleRate, float tempoSmoothing, WorldSoundTuning tuning)
        {
            _bus = bus;
            _theme = theme;
            _ambience = ambience;
            _tuning = tuning;
            _rule = new EscalationRule(theme != null ? theme.EscalationTempoRise : 0f, tempoSmoothing,
                theme != null ? theme.EscalationRampSeconds : 0f);
            BuildAmbience(host, sampleRate);
            _escalationSource = BuildEscalation(host, sampleRate);
        }

        /// <summary>The escalation state (the tempo rise and the layer fade-in).</summary>
        public EscalationRule Rule => _rule;

        public void Subscribe()
        {
            if (_active) return;
            _active = true;
            _bus.Subscribe<ShiftStarted>(OnShiftStarted);
            _bus.Subscribe<ShiftLast30Seconds>(OnLast30);
            _bus.Subscribe<ShiftEnded>(OnShiftEnded);
        }

        public void Unsubscribe()
        {
            if (!_active) return;
            _active = false;
            _bus.Unsubscribe<ShiftStarted>(OnShiftStarted);
            _bus.Unsubscribe<ShiftLast30Seconds>(OnLast30);
            _bus.Unsubscribe<ShiftEnded>(OnShiftEnded);
        }

        /// <summary>Sets the base gain (master after mute, times the effect volume) and applies every layer volume.</summary>
        public void SetGain(float gain)
        {
            _gain = gain;
            float ambienceScale = gain * _tuning.Ambience * (_theme != null ? _theme.AmbienceVolume : 1f);
            for (int i = 0; i < _layerSources.Count; i++) _layerSources[i].volume = ambienceScale * _layerLevels[i];
            ApplyEscalationVolume();
        }

        /// <summary>Advances the escalation and returns the music pitch (1 = normal tempo).</summary>
        public float Tick(float deltaSeconds)
        {
            float pitch = _rule.Step(deltaSeconds);
            if (_rule.Active) ApplyEscalationVolume();
            return pitch;
        }

        private void BuildAmbience(GameObject host, int rate)
        {
            if (_ambience == null) return;
            for (int i = 0; i < _ambience.Layers.Count; i++)
            {
                var layer = _ambience.Layers[i];
                if (!AmbienceLoops.IsLoop(layer.Kind)) continue;
                var source = LoopSource(host, layer.Kind.ToString(), AmbienceLoops.Build(layer.Kind, rate), rate);
                _layerSources.Add(source);
                _layerLevels.Add(layer.Level);
                source.Play();
            }
        }

        private AudioSource BuildEscalation(GameObject host, int rate)
        {
            if (_theme == null) return null;
            var loop = EscalationLoops.Build(_theme.EscalationLayer, rate);
            return loop == null ? null : LoopSource(host, _theme.EscalationLayer.ToString(), loop, rate);
        }

        private static AudioSource LoopSource(GameObject host, string name, float[] samples, int rate)
        {
            var source = host.AddComponent<AudioSource>();
            source.clip = ClipFactory.Create(name, samples, rate, false);
            source.loop = true;
            source.playOnAwake = false;
            return source;
        }

        private void ApplyEscalationVolume()
        {
            if (_escalationSource == null) return;
            _escalationSource.volume = _gain * _tuning.Escalation * _theme.EscalationVolume * _rule.LayerLevel;
        }

        private void OnShiftStarted(ShiftStarted e) => StopEscalation();

        private void OnShiftEnded(ShiftEnded e) => StopEscalation();

        private void OnLast30(ShiftLast30Seconds e)
        {
            _rule.Trigger();
            if (_escalationSource == null) return;
            ApplyEscalationVolume();
            _escalationSource.Play();
        }

        private void StopEscalation()
        {
            _rule.Reset();
            if (_escalationSource != null) _escalationSource.Stop();
        }
    }
}
