using System;
using UncannyValet.Core.Events;

namespace UncannyValet.UI.Flow
{
    /// <summary>Feeds the run's events into the flow machine: shift started and over, the next lot, the end of the run and powers earned. One per lot scene.</summary>
    public sealed class FlowEventLink : IDisposable
    {
        private readonly IEventBus _bus;
        private readonly FlowMachine _machine;

        public FlowEventLink(IEventBus bus, FlowMachine machine)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _machine = machine ?? throw new ArgumentNullException(nameof(machine));
            _bus.Subscribe<ShiftStarted>(OnShiftStarted);
            _bus.Subscribe<ShiftResultReady>(OnShiftResult);
            _bus.Subscribe<NextLotChosen>(OnNextLot);
            _bus.Subscribe<RunCompleted>(OnRunCompleted);
            _bus.Subscribe<PowerGranted>(OnPowerGranted);
        }

        public void Dispose()
        {
            _bus.Unsubscribe<ShiftStarted>(OnShiftStarted);
            _bus.Unsubscribe<ShiftResultReady>(OnShiftResult);
            _bus.Unsubscribe<NextLotChosen>(OnNextLot);
            _bus.Unsubscribe<RunCompleted>(OnRunCompleted);
            _bus.Unsubscribe<PowerGranted>(OnPowerGranted);
        }

        private void OnShiftStarted(ShiftStarted e) => _machine.ShiftStarted();
        private void OnShiftResult(ShiftResultReady e) => _machine.ShiftEnded();
        private void OnNextLot(NextLotChosen e) => _machine.NextLotChosen(e.Next, e.Reason);
        private void OnRunCompleted(RunCompleted e) => _machine.RunCompleted(e.Reason);
        private void OnPowerGranted(PowerGranted e) => _machine.PowerGranted(e.Power, e.Level);
    }
}
