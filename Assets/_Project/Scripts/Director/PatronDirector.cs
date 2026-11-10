using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UnityEngine;

namespace UncannyValet.Director
{
    /// <summary>
    /// Thin shell around <see cref="DirectorBrain"/>: ticks it every few game seconds while the shift runs. All the
    /// decisions live in plain classes. Bootstrap calls <see cref="Initialize"/> before activating this object.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PatronDirector : MonoBehaviour
    {
        private DirectorProfile _profile;
        private DirectorState _state;
        private IDirectorRandom _random;
        private int _maxCombo;
        private IGameClock _clock;
        private DirectorSettings _settings;
        private FormTracker _form;
        private DirectorBrain _brain;
        private DilemmaDirector _dilemmas;
        private float _sinceTick;

        /// <summary>Hands over what only Bootstrap can supply. Must be called before this object is activated.</summary>
        /// <param name="maxCombo">The highest combo multiplier, for the form calculation.</param>
        public void Initialize(DirectorProfile profile, DirectorState state, IDirectorRandom random, int maxCombo)
        {
            _profile = profile; _state = state; _random = random; _maxCombo = maxCombo;
        }

        // Bootstrap registers services, then activates this object, so OnEnable is the first safe moment.
        private void OnEnable()
        {
            if (_profile == null || _state == null || _random == null)
            {
                Debug.LogError("PatronDirector was not initialised: call Initialize before activating it.", this);
                enabled = false;
                return;
            }
            var bus = Services.Get<IEventBus>();
            var tuning = Services.Get<ITuning>();
            _clock = Services.Get<IGameClock>();
            Services.TryGet<IShiftScoreView>(out var score);
            _settings = new DirectorSettings();
            _settings.Refresh(tuning);
            _form = new FormTracker(bus, _settings.Form, _clock, score, _maxCombo);
            var pacer = new ArrivalPacer(_random, _settings.Pacing, new ReliefGate(_settings.Relief));
            var rules = DilemmaRules.CreateAll(_settings.Rules);
            var picker = new ArchetypePicker(_random, rules, _settings.Picker);
            _dilemmas = new DilemmaDirector(bus, _clock, picker, new DilemmaGate(_settings.Gate), rules, _state);
            _brain = new DirectorBrain(bus, tuning, _clock, Services.Get<ILotSnapshotSource>(), _profile, _state,
                _settings, _form, pacer, _dilemmas);
            _sinceTick = 0f;
        }

        private void OnDisable()
        {
            _brain?.Dispose();
            _dilemmas?.Dispose();
            _form?.Dispose();
            _brain = null;
            _dilemmas = null;
            _form = null;
        }

        private void Update()
        {
            if (_brain == null) return;
            if (!_clock.IsRunning) { _sinceTick = 0f; return; }
            _sinceTick += Time.deltaTime;
            if (_sinceTick < _settings.TickSeconds) return;
            _brain.Tick(_sinceTick);
            _sinceTick = 0f;
        }
    }
}
