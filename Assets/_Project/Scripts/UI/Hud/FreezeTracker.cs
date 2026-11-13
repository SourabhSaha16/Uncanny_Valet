using System;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;

namespace UncannyValet.UI.Hud
{
    /// <summary>Follows PowerActivated and PowerEnded of freeze time and drives the <see cref="FreezeFade"/> of the frozen look.</summary>
    public sealed class FreezeTracker : IDisposable
    {
        private readonly IEventBus _bus;
        private float _lastTime = -1f;

        public FreezeTracker(IEventBus bus, FreezeFade fade)
        {
            _bus = bus;
            Fade = fade;
            _bus.Subscribe<PowerActivated>(OnActivated);
            _bus.Subscribe<PowerEnded>(OnEnded);
            _bus.Subscribe<ShiftStarted>(OnShiftStarted);
            _bus.Subscribe<ShiftEnded>(OnShiftEnded);
        }

        public FreezeFade Fade { get; }

        /// <summary>Advances the fade to the given unscaled time (seconds), so it runs in real time while the world is frozen.</summary>
        public void Tick(float time)
        {
            if (_lastTime >= 0f) Fade.Tick(time - _lastTime);
            _lastTime = time;
        }

        public void Dispose()
        {
            _bus.Unsubscribe<PowerActivated>(OnActivated);
            _bus.Unsubscribe<PowerEnded>(OnEnded);
            _bus.Unsubscribe<ShiftStarted>(OnShiftStarted);
            _bus.Unsubscribe<ShiftEnded>(OnShiftEnded);
        }

        private void OnActivated(PowerActivated e)
        {
            if (e.Power == StoryPower.FreezeTime) Fade.Frozen = true;
        }

        private void OnEnded(PowerEnded e)
        {
            if (e.Power == StoryPower.FreezeTime) Fade.Frozen = false;
        }

        private void OnShiftStarted(ShiftStarted e) => Fade.Clear();

        private void OnShiftEnded(ShiftEnded e) => Fade.Frozen = false;
    }
}
