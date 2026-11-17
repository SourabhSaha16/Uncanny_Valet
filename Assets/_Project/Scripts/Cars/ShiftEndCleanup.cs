using System;
using UncannyValet.Core.Events;

namespace UncannyValet.Cars
{
    /// <summary>
    /// Runs the factory's despawn-everything call the moment the shift ends, so no car is left driving away in the background while the
    /// results screen shows. Subscribes on creation, unsubscribes on <see cref="Dispose"/>.
    /// </summary>
    public sealed class ShiftEndCleanup : IDisposable
    {
        private readonly IEventBus _bus;
        private readonly Action _despawnAll;

        public ShiftEndCleanup(IEventBus bus, Action despawnAll)
        {
            _bus = bus;
            _despawnAll = despawnAll;
            _bus.Subscribe<ShiftEnded>(OnShiftEnded);
        }

        public void Dispose() => _bus.Unsubscribe<ShiftEnded>(OnShiftEnded);

        private void OnShiftEnded(ShiftEnded e) => _despawnAll();
    }
}
