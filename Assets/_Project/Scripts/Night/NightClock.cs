using UnityEngine;
using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Night
{
    /// <summary>The night's game time (design.md section 4). Ticks once per game minute on the event bus.</summary>
    public sealed class NightClock : MonoBehaviour, IGameClock
    {
        [SerializeField, Min(0.01f)] private float _gameMinutesPerSecond = 1f;
        [SerializeField, Min(1), Tooltip("Game minutes per night. Bootstrap overrides it from RunConfig.")]
        private int _nightLengthMinutes = 180;

        private IEventBus _bus;
        private int _lastTick;

        public int Night { get; private set; }
        public float MinuteOfNight { get; private set; }
        public bool IsRunning { get; private set; }
        public int NightLengthMinutes => _nightLengthMinutes;

        /// <summary>Sets the bus; a positive nightLengthMinutes (from RunConfig) overrides the serialized length.</summary>
        public void Initialize(IEventBus bus, int nightLengthMinutes = 0)
        {
            _bus = bus;
            if (nightLengthMinutes > 0) _nightLengthMinutes = nightLengthMinutes;
        }

        public void StartNight(int night)
        {
            _bus ??= Services.Get<IEventBus>();
            Night = night;
            MinuteOfNight = 0f;
            _lastTick = 0;
            IsRunning = true;
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
