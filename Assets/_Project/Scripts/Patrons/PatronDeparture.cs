using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;

namespace UncannyValet.Patrons
{
    /// <summary>
    /// On TicketClosed the patron drives off: announce it, then the car drives off and is removed. After a real delivery
    /// (and when a boarding timeout is set) the car first waits for the patron to get in (PatronBoarded), or for the
    /// timeout in game minutes, or for the shift to end. A timeout of 0 sends the car off at once.
    /// </summary>
    public sealed class PatronDeparture
    {
        private readonly IEventBus _bus;
        private readonly ICarFactory _cars;
        private readonly IGameClock _clock;
        private readonly float _boardingMinutes;
        private readonly HashSet<CarId> _delivered = new HashSet<CarId>();
        private readonly Dictionary<CarId, float> _boarding = new Dictionary<CarId, float>();
        private readonly List<CarId> _scratch = new List<CarId>();

        public PatronDeparture(IEventBus bus, ICarFactory cars, IGameClock clock = null, float boardingMinutes = 0f)
        {
            _bus = bus; _cars = cars; _clock = clock;
            _boardingMinutes = clock == null ? 0f : boardingMinutes;
            _bus.Subscribe<TicketClosed>(OnTicketClosed);
            _bus.Subscribe<CarDelivered>(OnDelivered);
            _bus.Subscribe<PatronBoarded>(OnBoarded);
            _bus.Subscribe<ShiftTick>(OnTick);
            _bus.Subscribe<ShiftEnded>(OnShiftEnded);
        }

        public void Dispose()
        {
            _bus.Unsubscribe<TicketClosed>(OnTicketClosed);
            _bus.Unsubscribe<CarDelivered>(OnDelivered);
            _bus.Unsubscribe<PatronBoarded>(OnBoarded);
            _bus.Unsubscribe<ShiftTick>(OnTick);
            _bus.Unsubscribe<ShiftEnded>(OnShiftEnded);
        }

        private void OnDelivered(CarDelivered e)
        {
            if (_boardingMinutes > 0f) _delivered.Add(e.Car);
        }

        private void OnTicketClosed(TicketClosed e)
        {
            if (_delivered.Remove(e.Car)) _boarding[e.Car] = _clock.ElapsedMinutes + _boardingMinutes;
            else Depart(e.Car);
        }

        private void OnBoarded(PatronBoarded e)
        {
            if (_boarding.Remove(e.Car)) Depart(e.Car);
        }

        private void OnTick(ShiftTick e)
        {
            if (_boarding.Count == 0) return;
            _scratch.Clear();
            foreach (var pair in _boarding)
                if (e.ElapsedMinutes >= pair.Value) _scratch.Add(pair.Key);
            ReleaseScratch();
        }

        private void OnShiftEnded(ShiftEnded e)
        {
            _delivered.Clear();
            _scratch.Clear();
            foreach (var pair in _boarding) _scratch.Add(pair.Key);
            ReleaseScratch();
        }

        private void ReleaseScratch()
        {
            foreach (var car in _scratch)
            {
                _boarding.Remove(car);
                Depart(car);
            }
        }

        private void Depart(CarId car)
        {
            _bus.Publish(new CarDeparted(car));
            _cars.DriveAwayAndDespawn(car);
        }
    }
}
