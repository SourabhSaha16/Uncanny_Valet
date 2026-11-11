using System;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UnityEngine;

namespace UncannyValet.UI.Hud
{
    /// <summary>
    /// Turns tip events into pops: CarDelivered then TicketClosed (the final tip, combo included) are held per car in
    /// <see cref="PendingTipPops"/>; when PatronBoarded fires a pop starts over that car, where the patron gets in.
    /// A car that departs without its patron boarding, or the shift ending, drops the held tip.
    /// </summary>
    public sealed class TipPopTracker : IDisposable
    {
        private readonly IEventBus _bus;
        private readonly ICarFactory _cars;
        private readonly TipPops _pops;
        private readonly TipPopSettings _settings;
        private readonly Func<float> _clock;
        private readonly PendingTipPops _pending = new PendingTipPops();

        public TipPopTracker(IEventBus bus, ICarFactory cars, TipPops pops, TipPopSettings settings, Func<float> clock)
        {
            _bus = bus;
            _cars = cars;
            _pops = pops;
            _settings = settings;
            _clock = clock;
            _bus.Subscribe<CarDelivered>(OnDelivered);
            _bus.Subscribe<TicketClosed>(OnClosed);
            _bus.Subscribe<PatronBoarded>(OnBoarded);
            _bus.Subscribe<CarDeparted>(OnDeparted);
            _bus.Subscribe<ShiftEnded>(OnShiftEnded);
        }

        public void Dispose()
        {
            _bus.Unsubscribe<CarDelivered>(OnDelivered);
            _bus.Unsubscribe<TicketClosed>(OnClosed);
            _bus.Unsubscribe<PatronBoarded>(OnBoarded);
            _bus.Unsubscribe<CarDeparted>(OnDeparted);
            _bus.Unsubscribe<ShiftEnded>(OnShiftEnded);
        }

        private void OnDelivered(CarDelivered e) => _pending.Delivered(e.Car);

        private void OnClosed(TicketClosed e) => _pending.Closed(e.Car, e.Tip);

        private void OnDeparted(CarDeparted e) => _pending.Drop(e.Car);

        private void OnShiftEnded(ShiftEnded e)
        {
            _pending.Clear();
            _pops.Clear();
        }

        private void OnBoarded(PatronBoarded e)
        {
            if (!_pending.TryBoard(e.Car, out var tip)) return;
            if (!_cars.TryGet(e.Car, out var car) || car.Transform == null) return;
            var at = car.Transform.position;
            _pops.Start(new Vector2(at.x, at.y + _settings.LiftWorldUnits), tip, _clock());
        }
    }
}
