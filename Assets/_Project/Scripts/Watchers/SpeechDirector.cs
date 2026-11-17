using System;
using UncannyValet.Core.Events;

namespace UncannyValet.Watchers
{
    /// <summary>
    /// Turns <see cref="SpeechRequested"/> events into at most one <see cref="SpeechShown"/> at a time through a <see cref="SpeechScheduler"/>.
    /// Anyone (watchers, gag-line pedestrians) publishes a request; the owner calls <see cref="Tick"/> once per frame.
    /// </summary>
    public sealed class SpeechDirector : IDisposable
    {
        private readonly IEventBus _bus;
        private readonly SpeechScheduler _scheduler;

        public SpeechDirector(IEventBus bus, SpeechScheduler scheduler)
        {
            _bus = bus;
            _scheduler = scheduler;
            _bus.Subscribe<SpeechRequested>(OnRequested);
            _bus.Subscribe<ShiftStarted>(OnShiftStarted);
        }

        /// <summary>Publishes <see cref="SpeechShown"/> when a request wins at time <paramref name="now"/> (seconds, real time).</summary>
        public void Tick(float now)
        {
            if (_scheduler.Tick(now, out var shown)) _bus.Publish(new SpeechShown(shown, _scheduler.BubbleSeconds));
        }

        public void Dispose()
        {
            _bus.Unsubscribe<SpeechRequested>(OnRequested);
            _bus.Unsubscribe<ShiftStarted>(OnShiftStarted);
        }

        private void OnRequested(SpeechRequested e) => _scheduler.Submit(e.Request);

        private void OnShiftStarted(ShiftStarted e) => _scheduler.Reset();
    }
}
