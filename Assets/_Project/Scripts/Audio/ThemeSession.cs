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
        private readonly LayerSet _layers;
        private readonly List<SourceLayer> _layerSources = new List<SourceLayer>();
        private readonly List<float> _layerLevels = new List<float>();
        private readonly SourceLayer _escalationSource;
        private float _gain;
        private bool _active;

        /// <param name="theme">May be null (ambience only, funk loop in the director).</param>
        /// <param name="ambience">May be null.</param>
        public ThemeSession(IEventBus bus, GameObject host, LayerSet layers, MusicThemeDefinition theme, AmbienceSetDefinition ambience,
            int sampleRate, float tempoSmoothing, WorldSoundTuning tuning)
        {
            _bus = bus;
            _layers = layers;
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
            _bus.Subscribe<NextLotChosen>(OnLotOver);
            _bus.Subscribe<RunCompleted>(OnRunOver);
        }

        public void Unsubscribe()
        {
            if (!_active) return;
            _active = false;
            _bus.Unsubscribe<ShiftStarted>(OnShiftStarted);
            _bus.Unsubscribe<ShiftLast30Seconds>(OnLast30);
            _bus.Unsubscribe<ShiftEnded>(OnShiftEnded);
            _bus.Unsubscribe<NextLotChosen>(OnLotOver);
            _bus.Unsubscribe<RunCompleted>(OnRunOver);
        }

        /// <summary>Sets the base gain (master after mute, times the effect volume) and applies every layer volume.</summary>
        public void SetGain(float gain)
        {
            _gain = gain;
            float ambienceScale = gain * _tuning.Ambience * (_theme != null ? _theme.AmbienceVolume : 1f);
            for (int i = 0; i < _layerSources.Count; i++) _layerSources[i].SetVolume(ambienceScale * _layerLevels[i]);
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
                var source = LoopLayer(host, layer.Kind.ToString(), AmbienceLoops.Build(layer.Kind, rate), rate);
                _layerSources.Add(source);
                _layerLevels.Add(layer.Level);
                source.Play();
            }
        }

        private SourceLayer BuildEscalation(GameObject host, int rate)
        {
            if (_theme == null) return null;
            var loop = EscalationLoops.Build(_theme.EscalationLayer, rate);
            return loop == null ? null : LoopLayer(host, _theme.EscalationLayer.ToString(), loop, rate);
        }

        private SourceLayer LoopLayer(GameObject host, string name, float[] samples, int rate)
        {
            var clip = ClipFactory.Create(name, samples, rate, false);
            var layer = new SourceLayer(host.AddComponent<AudioSource>(), clip, clip);
            _layers.Hold(layer);
            return layer;
        }

        private void ApplyEscalationVolume()
        {
            if (_escalationSource == null) return;
            _escalationSource.SetVolume(_gain * _tuning.Escalation * _theme.EscalationVolume * _rule.LayerLevel);
        }

        // The lot is over (the next one loads from the story card): nothing of it may keep sounding.
        private void OnLotOver(NextLotChosen e) => _layers.SilenceAll();

        private void OnRunOver(RunCompleted e) => _layers.SilenceAll();

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
            _escalationSource?.Stop();
        }
    }
}
