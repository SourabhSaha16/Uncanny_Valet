using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Watchers;
using UnityEngine;

namespace UncannyValet.Watchers
{
    /// <summary>
    /// The scene object of the speech system: always runs the speech director (so floating gag lines work in every lot) and, when the lot has
    /// a watcher, its tips and comments. Bootstrap calls <see cref="Configure"/> while the object is still inactive, then activates it.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class WatcherHost : MonoBehaviour
    {
        private WatcherConfig _config;
        private WatcherDefinition _watcher;
        private Transform _anchor;
        private int _queueSlots;
        private SpeechDirector _director;
        private WatcherTips _tips;
        private WatcherComments _comments;

        /// <summary>Sets what to run; <paramref name="watcher"/> and <paramref name="anchor"/> may be null (no watcher in this lot).</summary>
        public void Configure(WatcherConfig config, WatcherDefinition watcher, Transform anchor, int queueSlots)
        {
            _config = config; _watcher = watcher; _anchor = anchor; _queueSlots = queueSlots;
        }

        private void OnEnable()
        {
            if (_config == null)
            {
                Debug.LogError("WatcherHost on " + name + " has no WatcherConfig; rebuild with Setup/Build Everything.", this);
                enabled = false;
                return;
            }
            var bus = Services.Get<IEventBus>();
            _director = new SpeechDirector(bus, new SpeechScheduler(_config.BubbleSeconds, _config.MinGapSeconds));
            if (_watcher == null || _anchor == null || _watcher.Text == null) return;
            var voice = new WatcherVoice(bus, _watcher.SpeakerName, _anchor.position, _anchor);
            _comments = new WatcherComments(bus, voice, _watcher.Text, _config.BigCrashDamage, CurrentGoal);
            _tips = new WatcherTips(bus, voice, _watcher.Text, _config, _queueSlots, WaitingCars, RealTime);
        }

        private void Update() => _director?.Tick(Time.unscaledTime);

        private void OnDisable()
        {
            _tips?.Dispose();
            _comments?.Dispose();
            _director?.Dispose();
            _tips = null;
            _comments = null;
            _director = null;
        }

        private static ILotGoal CurrentGoal() => Services.TryGet<ILotGoal>(out var goal) ? goal : null;

        private static float RealTime() => Time.unscaledTime;

        private static int WaitingCars()
            => Services.TryGet<ILotSnapshotSource>(out var source) ? source.Capture().CountCars(CarLotState.WaitingToPark) : 0;
    }
}
