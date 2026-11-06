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
        private NightPlanConfig _plans;
        private int _lastTick;

        public int Night { get; private set; }
        public float MinuteOfNight { get; private set; }
        public bool IsRunning { get; private set; }
        public int NightLengthMinutes => _nightLengthMinutes;

        /// <summary>
        /// Sets the bus. A positive nightLengthMinutes or minutesPerSecond (from RunConfig) overrides the
        /// serialized value; plans supplies each night's NightPlan (the built-in default table when null).
        /// </summary>
        public void Initialize(IEventBus bus, int nightLengthMinutes = 0, float minutesPerSecond = 0f,
            NightPlanConfig plans = null)
        {
            _bus = bus;
            _plans = plans;
            if (nightLengthMinutes > 0) _nightLengthMinutes = nightLengthMinutes;
            if (minutesPerSecond > 0f) _gameMinutesPerSecond = minutesPerSecond;
        }

        public void StartNight(int night)
        {
            _bus ??= Services.Get<IEventBus>();
            Night = night;
            MinuteOfNight = 0f;
            _lastTick = 0;
            IsRunning = true;
            _bus.Publish(new NightPlanChanged(_plans != null ? _plans.For(night) : NightPlanDefaults.For(night)));
            _bus.Publish(new NightStarted(night));
        }

        private void Update() => Advance(Time.deltaTime);

        /// <summary>Moves time forward by real seconds. Public so tests can drive it.</summary>
        public void Advance(float realSeconds)
        {
            if (!IsRunning) return;
            MinuteOfNight = Mathf.Min(MinuteOfNight + realSeconds * _gameMinutesPerSecond, _nightLengthMinutes);
            while (_lastTick < (int)MinuteOfNight)
            {
                _lastTick++;
                _bus.Publish(new NightTick(Night, _lastTick));
            }
            if (MinuteOfNight >= _nightLengthMinutes) EndNight();
        }

        private void EndNight()
        {
            IsRunning = false;
            _bus.Publish(new NightEnded(Night));
        }
    }
}
