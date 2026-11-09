using System;
using UncannyValet.Night;

namespace UncannyValet.Bootstrap
{
    /// <summary>Adapts the scene's <see cref="ShiftClock"/> to <see cref="IShiftStarter"/>.</summary>
    public sealed class ClockShiftStarter : IShiftStarter
    {
        private readonly ShiftClock _clock;

        public ClockShiftStarter(ShiftClock clock)
        {
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        public bool IsRunning => _clock.IsRunning;

        public void StartShift() => _clock.StartShift();
    }
}
