using UnityEngine;
using UncannyValet.Core;
using UncannyValet.Core.Config;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Night
{
    /// <summary>
    /// The shift's clock. Counts real seconds down from <see cref="ShiftConfig.ShiftLengthSeconds"/> and publishes
    /// ShiftStarted, a ShiftTick per game minute, ShiftLast30Seconds and ShiftEnded on the event bus.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ShiftClock : MonoBehaviour, IGameClock
    {
        private IEventBus _bus;
        private ITuning _tuning;
        private ShiftConfig _config;
        private ShiftPlanSource _source;
        private ShiftTimer _timer;
        private readonly WorldTimeReader _world = new WorldTimeReader();
        private bool _subscribed;
        private int _lastTick;

        public float SecondsRemaining => _timer != null ? _timer.SecondsRemaining : 0f;
        public float ShiftLengthSeconds => _timer != null ? _timer.LengthSeconds : LengthSetting;
        public float ElapsedMinutes { get; private set; }
        public bool IsRunning { get; private set; }

        private float LengthSetting => Read(TuningKey.ShiftLengthSeconds, _config != null ? _config.ShiftLengthSeconds : 0f);
        private float MinutesPerSecond => Read(TuningKey.GameMinutesPerSecond, _config.GameMinutesPerSecond);
        private ShiftPlanSource Source => _source ??= new ShiftPlanSource(_config, _tuning);

        /// <summary>Sets the bus and the config. tuning, else the registered ITuning (resolved in Start), supplies live numbers.</summary>
        public void Initialize(IEventBus bus, ShiftConfig config, ITuning tuning = null)
        {
            _bus = bus;
            _config = config;
            _tuning = tuning;
            _source = null;
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
            if (Source.Record(e.Key, e.Value) && IsRunning) _bus.Publish(new ShiftPlanChanged(Source.Current()));
        }

        /// <summary>Starts (or restarts) the shift: full time, nothing ticked yet. Publishes the plan, then ShiftStarted.</summary>
        public void StartShift()
        {
            if (_config == null)
            {
                Debug.LogError("ShiftClock has no ShiftConfig; call Initialize first.", this);
                return;
            }
            _bus ??= Services.Get<IEventBus>();
            _timer = new ShiftTimer(LengthSetting, _config.FinalStretchSeconds);
            ElapsedMinutes = 0f;
            _lastTick = 0;
            IsRunning = true;
            _bus.Publish(new ShiftPlanChanged(Source.Current()));
            _bus.Publish(new ShiftStarted());
        }

        // World time: frozen (or slowed) by the freeze time power through IWorldTime.
        private void Update() => Advance(_world.Scaled(Time.deltaTime));

        /// <summary>Moves time forward by world seconds (real seconds times the world time scale). Public so tests can drive it.</summary>
        public void Advance(float realSeconds)
        {
            if (!IsRunning) return;
            var counted = Mathf.Min(Mathf.Max(0f, realSeconds), _timer.SecondsRemaining);
            var step = _timer.Advance(realSeconds);
            ElapsedMinutes += counted * MinutesPerSecond;
            while (_lastTick < (int)ElapsedMinutes)
            {
                _lastTick++;
                _bus.Publish(new ShiftTick(_lastTick));
            }
            if ((step & ShiftTimerStep.EnteredFinalStretch) != 0) _bus.Publish(new ShiftLast30Seconds());
            if ((step & ShiftTimerStep.Ended) == 0) return;
            IsRunning = false;
            _bus.Publish(new ShiftEnded());
        }
    }
}
