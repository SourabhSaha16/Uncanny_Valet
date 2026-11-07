using System;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Orders
{
    /// <summary>
    /// The single owner of the run's strike count. Strikes carry across nights and reset only when a new run
    /// starts, which is when night 1 begins again.
    /// </summary>
    public sealed class StrikeTracker : IEventSubscriber
    {
        private const int FirstNight = 1;

        private readonly IEventBus _bus;
        private readonly ITuning _tuning;

        public StrikeTracker(IEventBus bus, ITuning tuning)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _tuning = tuning ?? throw new ArgumentNullException(nameof(tuning));
        }

        public int Total { get; private set; }

        /// <summary>Strikes that lose the run, read live from tuning (at least 1).</summary>
        public int Max => Math.Max(1, (int)Math.Round(_tuning.Get(TuningKey.MaxStrikes)));

        public void Subscribe() => _bus.Subscribe<NightStarted>(OnNightStarted);

        public void Unsubscribe() => _bus.Unsubscribe<NightStarted>(OnNightStarted);

        /// <summary>Adds one strike and publishes StrikeGiven.</summary>
        public void AddStrike()
        {
            Total++;
            _bus.Publish(new StrikeGiven(Total, Max));
        }

        /// <summary>Starts a fresh run. Publishes nothing; the UI resets on NightStarted of night 1.</summary>
        public void Reset() => Total = 0;

        private void OnNightStarted(NightStarted e)
        {
            if (e.Night == FirstNight) Reset();
        }
    }
}
