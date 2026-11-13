using System;
using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;

namespace UncannyValet.UI.Hud
{
    /// <summary>
    /// Feeds the bar view and the power icons: the bar from PowerBarChanged, the icon states from IPowerView, and the
    /// one-time icon flash at the start of the first lot after a power (or its upgrade) was earned (ShiftStarted, compared
    /// with the level the run state says was already flashed). Advances with real time. Visible only while Thor owns a power.
    /// </summary>
    public sealed class PowerBarTracker : IDisposable
    {
        private readonly IEventBus _bus;
        private IRunProgress _progress;
        private IPowerView _powers;
        private float _lastTime = -1f;

        public PowerBarTracker(IEventBus bus, PowerBarSettings settings, IRunProgress progress = null, IPowerView powers = null)
        {
            _bus = bus;
            _progress = progress;
            _powers = powers;
            View = new PowerBarView(settings.FillSmoothing, settings.PulseSeconds, settings.SpentFlashSeconds);
            Flashes = new PowerIconFlashes(settings.IconFlashSeconds);
            _bus.Subscribe<PowerBarChanged>(OnBar);
            _bus.Subscribe<ShiftStarted>(OnShiftStarted);
        }

        public PowerBarView View { get; }
        public PowerIconFlashes Flashes { get; }

        /// <summary>True while at least one power is owned; nothing of the bar is drawn before that.</summary>
        public bool IsVisible => Progress != null && Progress.EarnedPowers.Count > 0;

        /// <summary>Powers owned; one icon each, in key order.</summary>
        public int IconCount => Progress != null ? Progress.EarnedPowers.Count : 0;

        /// <summary>The power on icon slot <paramref name="slot"/> (key slot + 1 on the keyboard).</summary>
        public StoryPower PowerAt(int slot) => Progress.EarnedPowers[slot];

        /// <summary>How the icon of an owned power looks right now; false when the power view has no state for it.</summary>
        public bool TryGetLook(StoryPower power, out PowerIconLook look)
        {
            look = default;
            if (!Powers.TryGetState(power, out var state)) return false;
            look = PowerIconLook.Of(state, View.OpenBuckets);
            return true;
        }

        private IRunProgress Progress
        {
            get
            {
                if (_progress == null) Services.TryGet(out _progress);
                return _progress;
            }
        }

        private IPowerView Powers
        {
            get
            {
                if (_powers == null) Services.TryGet(out _powers);
                return _powers ?? NoPowers.Instance;
            }
        }

        /// <summary>Advances the animations to the given unscaled time (seconds).</summary>
        public void Tick(float time)
        {
            if (_lastTime >= 0f)
            {
                var dt = time - _lastTime;
                View.Tick(dt);
                Flashes.Tick(dt);
            }
            _lastTime = time;
        }

        public void Dispose()
        {
            _bus.Unsubscribe<PowerBarChanged>(OnBar);
            _bus.Unsubscribe<ShiftStarted>(OnShiftStarted);
        }

        private void OnBar(PowerBarChanged e) => View.Set(e.Value, e.OpenBuckets, e.SpentBucket);

        private void OnShiftStarted(ShiftStarted e)
        {
            var progress = Progress;
            if (progress == null) return;
            for (var i = 0; i < progress.EarnedPowers.Count; i++)
            {
                var power = progress.EarnedPowers[i];
                var level = progress.PowerLevel(power);
                if (level <= progress.FlashedLevel(power)) continue;
                Flashes.Begin(power);
                _bus.Publish(new PowerIconFlashed(power, level));
            }
        }

        private sealed class NoPowers : IPowerView
        {
            public static readonly NoPowers Instance = new NoPowers();
            public bool TryGetState(StoryPower power, out PowerState state) { state = default; return false; }
            public bool TryGetValues(StoryPower power, int level, out PowerLevelValues values) { values = default; return false; }
        }
    }
}
