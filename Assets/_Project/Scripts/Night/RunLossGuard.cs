using System;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Night
{
    /// <summary>
    /// Ends the night when the boss gives the final strike. The end happens on the clock's next advance, after every
    /// StrikeGiven subscriber has run, so the tally sees the strike before NightEnded whatever the subscribe order.
    /// Strikes outside a running night (summary card showing) are ignored. Create from Bootstrap; Dispose on teardown.
    /// </summary>
    public sealed class RunLossGuard : IDisposable
    {
        private readonly IEventBus _bus;
        private readonly INightEnder _night;

        public RunLossGuard(IEventBus bus, INightEnder night)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _night = night ?? throw new ArgumentNullException(nameof(night));
            _bus.Subscribe<StrikeGiven>(OnStrikeGiven);
        }

        public void Dispose() => _bus.Unsubscribe<StrikeGiven>(OnStrikeGiven);

        private void OnStrikeGiven(StrikeGiven e)
        {
            if (e.IsFinal && _night.IsRunning) _night.EndAtNextTick();
        }
    }
}
