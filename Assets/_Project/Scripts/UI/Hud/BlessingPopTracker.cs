using System;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UnityEngine;

namespace UncannyValet.UI.Hud
{
    /// <summary>
    /// Turns blessing events into pops: CarDelivered then TicketClosed (the final blessing, favour included) are held per car in
    /// <see cref="PendingBlessingPops"/>; when PatronBoarded fires a pop starts over that car, where the patron gets in.
    /// PerfectPark and PerfectDropOff start a PERFECT! pop over their car at once. The car's spot is read when the ticket
    /// closes, because PatronBoarded also sends the car off and the car may be gone by the time this hears of it. A
    /// delivered patron always boards eventually (PatronWalkers publishes it), so a departing car does not drop the blessing;
    /// only the shift ending does.
    /// </summary>
    public sealed class BlessingPopTracker : IDisposable
    {
        private readonly IEventBus _bus;
        private readonly ICarFactory _cars;
        private readonly BlessingPops _pops;
        private readonly BlessingPopSettings _settings;
        private readonly Func<float> _clock;
        private readonly PendingBlessingPops _pending = new PendingBlessingPops();

        public BlessingPopTracker(IEventBus bus, ICarFactory cars, BlessingPops pops, BlessingPopSettings settings, Func<float> clock)
        {
            _bus = bus;
            _cars = cars;
            _pops = pops;
            _settings = settings;
            _clock = clock;
            _bus.Subscribe<CarDelivered>(OnDelivered);
            _bus.Subscribe<TicketClosed>(OnClosed);
            _bus.Subscribe<PatronBoarded>(OnBoarded);
            _bus.Subscribe<ShiftEnded>(OnShiftEnded);
            _bus.Subscribe<PerfectPark>(OnPerfectPark);
            _bus.Subscribe<PerfectDropOff>(OnPerfectDropOff);
        }

        public void Dispose()
        {
            _bus.Unsubscribe<CarDelivered>(OnDelivered);
            _bus.Unsubscribe<TicketClosed>(OnClosed);
            _bus.Unsubscribe<PatronBoarded>(OnBoarded);
            _bus.Unsubscribe<ShiftEnded>(OnShiftEnded);
            _bus.Unsubscribe<PerfectPark>(OnPerfectPark);
            _bus.Unsubscribe<PerfectDropOff>(OnPerfectDropOff);
        }

        private void OnDelivered(CarDelivered e) => _pending.Delivered(e.Car);

        private void OnClosed(TicketClosed e)
        {
            if (!_cars.TryGet(e.Car, out var car) || car.Transform == null) return;
            _pending.Closed(e.Car, e.Blessing, car.Transform.position);
        }

        private void OnShiftEnded(ShiftEnded e)
        {
            _pending.Clear();
            _pops.Clear();
        }

        private void OnPerfectPark(PerfectPark e) => PopPerfect(e.Car);

        private void OnPerfectDropOff(PerfectDropOff e) => PopPerfect(e.Car);

        private void PopPerfect(CarId id)
        {
            if (!_cars.TryGet(id, out var car) || car.Transform == null) return;
            var at = car.Transform.position;
            var lift = _settings.LiftWorldUnits + _settings.PerfectExtraLiftWorldUnits;
            _pops.StartLabel(new Vector2(at.x, at.y + lift), BlessingPopText.PerfectLabel, _clock());
        }

        private void OnBoarded(PatronBoarded e)
        {
            if (!_pending.TryBoard(e.Car, out var blessing, out var at)) return;
            _pops.Start(new Vector2(at.x, at.y + _settings.LiftWorldUnits), blessing, _clock());
        }
    }
}
