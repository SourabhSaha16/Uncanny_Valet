using System;
using UncannyValet.Night;

namespace UncannyValet.Bootstrap
{
    /// <summary>Adapts the scene's <see cref="NightClock"/> to <see cref="INightStarter"/>.</summary>
    public sealed class ClockNightStarter : INightStarter
    {
        private readonly NightClock _clock;

        public ClockNightStarter(NightClock clock)
        {
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        public bool IsRunning => _clock.IsRunning;

        public void StartNight(int night) => _clock.StartNight(night);
    }
}
