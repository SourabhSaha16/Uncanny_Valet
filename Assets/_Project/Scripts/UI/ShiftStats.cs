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
            _bus.Subscribe<PerfectPark>(OnPerfectPark);
            _bus.Subscribe<ParkSmashCompleted>(OnSmash);
            _bus.Subscribe<ScoreChanged>(OnScore);
        }

        /// <summary>Different cars that were parked at least once.</summary>
        public int CarsParked => _parked.Count;

        /// <summary>Perfect Parks earned this shift.</summary>
        public int PerfectParks { get; private set; }

        /// <summary>Dollars of tips paid by Park Smashes this shift.</summary>
        public int SmashTips { get; private set; }

        /// <summary>Highest combo multiplier reached.</summary>
        public int BestCombo { get; private set; } = 1;

        public void Dispose()
        {
            _bus.Unsubscribe<ShiftStarted>(OnShiftStarted);
            _bus.Unsubscribe<CarParked>(OnParked);
            _bus.Unsubscribe<PerfectPark>(OnPerfectPark);
            _bus.Unsubscribe<ParkSmashCompleted>(OnSmash);
            _bus.Unsubscribe<ScoreChanged>(OnScore);
        }

        private void OnShiftStarted(ShiftStarted e)
        {
            _parked.Clear();
            PerfectParks = 0;
            SmashTips = 0;
            BestCombo = 1;
        }

        private void OnParked(CarParked e) => _parked.Add(e.Car.Value);

        private void OnPerfectPark(PerfectPark e) => PerfectParks++;

        private void OnSmash(ParkSmashCompleted e) => SmashTips += e.TotalTips;

        private void OnScore(ScoreChanged e)
        {
            if (e.ComboMultiplier > BestCombo) BestCombo = e.ComboMultiplier;
        }
    }
}
