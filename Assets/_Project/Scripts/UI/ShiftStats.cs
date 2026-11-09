using System;
using System.Collections.Generic;
using UncannyValet.Core.Events;

namespace UncannyValet.UI
{
    /// <summary>Counts what the results screen reports for the running shift, from events only. Resets on ShiftStarted.</summary>
    public sealed class ShiftStats : IDisposable
    {
        private readonly IEventBus _bus;
        private readonly HashSet<int> _parked = new HashSet<int>();

        public ShiftStats(IEventBus bus)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _bus.Subscribe<ShiftStarted>(OnShiftStarted);
            _bus.Subscribe<CarParked>(OnParked);
            _bus.Subscribe<CarDelivered>(OnDelivered);
            _bus.Subscribe<ScoreChanged>(OnScore);
        }

        /// <summary>Different cars that were parked at least once.</summary>
        public int CarsParked => _parked.Count;

        /// <summary>Cars handed back to their patron at the curb.</summary>
        public int CarsReturned { get; private set; }

        /// <summary>Highest combo multiplier reached.</summary>
        public int BestCombo { get; private set; } = 1;

        public void Dispose()
        {
            _bus.Unsubscribe<ShiftStarted>(OnShiftStarted);
            _bus.Unsubscribe<CarParked>(OnParked);
            _bus.Unsubscribe<CarDelivered>(OnDelivered);
            _bus.Unsubscribe<ScoreChanged>(OnScore);
        }

        private void OnShiftStarted(ShiftStarted e)
        {
            _parked.Clear();
            CarsReturned = 0;
            BestCombo = 1;
        }

        private void OnParked(CarParked e) => _parked.Add(e.Car.Value);

        private void OnDelivered(CarDelivered e) => CarsReturned++;

        private void OnScore(ScoreChanged e)
        {
            if (e.ComboMultiplier > BestCombo) BestCombo = e.ComboMultiplier;
        }
    }
}
