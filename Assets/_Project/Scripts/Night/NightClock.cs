using UnityEngine;
using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Night
{
    /// <summary>The night's game time (design.md section 4). Ticks once per game minute on the event bus.</summary>
    public sealed class NightClock : MonoBehaviour, IGameClock
    {
        [SerializeField, Min(0.01f), Tooltip("Game minutes per real second. Bootstrap overrides it from RunConfig.")]
        private float _gameMinutesPerSecond = 1.5f;
        [SerializeField, Min(1), Tooltip("Game minutes per night. Bootstrap overrides it from RunConfig.")]
        private int _nightLengthMinutes = 180;

        private IEventBus _bus;
        private ITuning _tuning;
        private NightPlanConfig _plans;
        private NightPlanSource _source;
        private bool _subscribed;
        private int _lastTick;

        public int Night { get; private set; }
        public float MinuteOfNight { get; private set; }
        public bool IsRunning { get; private set; }
        public int NightLengthMinutes => Mathf.RoundToInt(Read(TuningKey.NightLengthMinutes, _nightLengthMinutes));
        private float MinutesPerSecond => Read(TuningKey.GameMinutesPerSecond, _gameMinutesPerSecond);
        private NightPlanSource Source => _source ??= new NightPlanSource(_plans, _tuning);

        /// <summary>
        /// Sets the bus. A positive nightLengthMinutes or minutesPerSecond (from RunConfig) is the fallback when
        /// ITuning has no value; plans supplies each night's NightPlan (the default table when null). tuning, else the
        /// registered ITuning (resolved in Start), supplies speed, length and plan numbers live.
        /// </summary>
        public void Initialize(IEventBus bus, int nightLengthMinutes = 0, float minutesPerSecond = 0f,
            NightPlanConfig plans = null, ITuning tuning = null)
        {
            _bus = bus;
            _plans = plans;
            _tuning = tuning;
            _source = null;
            if (nightLengthMinutes > 0) _nightLengthMinutes = nightLengthMinutes;
            if (minutesPerSecond > 0f) _gameMinutesPerSecond = minutesPerSecond;
            Subscribe();
        }

        private void Start()
        {
            if (_tuning == null && Services.TryGet(out _tuning)) _source = null;
        }

        private void OnEnable() => Subscribe();

        private void OnDisable()
        {
            if (_subscribed) _bus.Unsubscribe<TuningChanged>(OnTuningChanged);
            _subscribed = false;
        }

        private void Subscribe()
        {
            if (_subscribed || _bus == null || !isActiveAndEnabled) return;
            _bus.Subscribe<TuningChanged>(OnTuningChanged);
            _subscribed = true;
        }

        private float Read(TuningKey key, float fallback)
            => _tuning != null && _tuning.TryGet(key, out var v) ? v : fallback;

        private void OnTuningChanged(TuningChanged e)
        {
            if (Source.Record(e.Key, e.Value) && IsRunning) _bus.Publish(new NightPlanChanged(Source.For(Night)));
        }

        public void StartNight(int night)
        {
            _bus ??= Services.Get<IEventBus>();
            Night = night;
            MinuteOfNight = 0f;
            _lastTick = 0;
            IsRunning = true;
            _bus.Publish(new NightPlanChanged(Source.For(night)));
            _bus.Publish(new NightStarted(night));
        }

        private void Update() => Advance(Time.deltaTime);

        /// <summary>Moves time forward by real seconds. Public so tests can drive it.</summary>
        public void Advance(float realSeconds)
        {
            if (!IsRunning) return;
            var length = NightLengthMinutes;
            MinuteOfNight = Mathf.Min(MinuteOfNight + realSeconds * MinutesPerSecond, length);
            while (_lastTick < (int)MinuteOfNight)
            {
                _lastTick++;
                _bus.Publish(new NightTick(Night, _lastTick));
            }
            if (MinuteOfNight >= length) EndNight();
        }

        private void EndNight()
        {
            IsRunning = false;
            _bus.Publish(new NightEnded(Night));
        }
    }
}
