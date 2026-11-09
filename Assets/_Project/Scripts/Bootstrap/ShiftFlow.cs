using System;
using UncannyValet.Core.Events;

namespace UncannyValet.Bootstrap
{
    /// <summary>
    /// The shift-to-shift flow. On RetryShiftRequested it starts the shift again from a clean lot. The valet is
    /// only moved when he is on foot; a driven car is ejected by the despawn code.
    /// </summary>
    public sealed class ShiftFlow : IDisposable
    {
        private readonly IEventBus _bus;
        private readonly IShiftStarter _starter;
        private readonly IValetPlacer _valet;
        private bool _driving;

        public ShiftFlow(IEventBus bus, IShiftStarter starter, IValetPlacer valet)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _starter = starter ?? throw new ArgumentNullException(nameof(starter));
            _valet = valet ?? throw new ArgumentNullException(nameof(valet));
            _bus.Subscribe<RetryShiftRequested>(OnRetry);
            _bus.Subscribe<CarEntered>(OnCarEntered);
            _bus.Subscribe<CarExited>(OnCarExited);
        }

        public void Dispose()
        {
            _bus.Unsubscribe<RetryShiftRequested>(OnRetry);
            _bus.Unsubscribe<CarEntered>(OnCarEntered);
            _bus.Unsubscribe<CarExited>(OnCarExited);
        }

        public void StartFirstShift() => _starter.StartShift();

        private void OnCarEntered(CarEntered e) => _driving = true;
        private void OnCarExited(CarExited e) => _driving = false;

        private void OnRetry(RetryShiftRequested e)
        {
            if (_starter.IsRunning) return;
            if (!_driving) _valet.PlaceAtStart();
            _starter.StartShift();
        }
    }
}
