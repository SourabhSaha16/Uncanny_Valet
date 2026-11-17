using System;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.UI.Hud
{
    /// <summary>
    /// Starts a "+X" pop on every PowerFilled (the Powers module publishes it only while a power is owned) and clears the pops
    /// when a shift starts or ends.
    /// </summary>
    public sealed class PowerFillPopTracker : IDisposable
    {
        private readonly IEventBus _bus;
        private readonly PowerFillPops _pops;
        private readonly Func<float> _clock;

        public PowerFillPopTracker(IEventBus bus, PowerFillPops pops, Func<float> clock)
        {
            _bus = bus;
            _pops = pops;
            _clock = clock;
            _bus.Subscribe<PowerFilled>(OnFilled);
            _bus.Subscribe<ShiftStarted>(OnShiftStarted);
            _bus.Subscribe<ShiftEnded>(OnShiftEnded);
        }

        public void Dispose()
        {
            _bus.Unsubscribe<PowerFilled>(OnFilled);
            _bus.Unsubscribe<ShiftStarted>(OnShiftStarted);
            _bus.Unsubscribe<ShiftEnded>(OnShiftEnded);
        }

        private void OnFilled(PowerFilled e) => _pops.Start(e.Amount, e.Source, _clock());

        private void OnShiftStarted(ShiftStarted e) => _pops.Clear();

        private void OnShiftEnded(ShiftEnded e) => _pops.Clear();
    }
}
