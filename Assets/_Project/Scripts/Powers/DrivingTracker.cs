using System;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Powers
{
    /// <summary>Tells whether the valet sits in a car, from CarEntered and CarExited (a power that acts differently on foot and in a car asks it).</summary>
    public sealed class DrivingTracker : IDisposable
    {
        private readonly IEventBus _bus;

        public DrivingTracker(IEventBus bus)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _bus.Subscribe<CarEntered>(OnEntered);
            _bus.Subscribe<CarExited>(OnExited);
        }

        /// <summary>True while a car has the valet as its driver.</summary>
        public bool IsDriving { get; private set; }

        /// <summary>Stops listening.</summary>
        public void Dispose()
        {
            _bus.Unsubscribe<CarEntered>(OnEntered);
            _bus.Unsubscribe<CarExited>(OnExited);
        }

        private void OnEntered(CarEntered e) => IsDriving = true;

        private void OnExited(CarExited e) => IsDriving = false;
    }
}
