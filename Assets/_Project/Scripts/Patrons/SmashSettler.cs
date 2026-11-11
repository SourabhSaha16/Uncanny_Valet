using System;
using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Patrons
{
    /// <summary>
    /// The Patrons side of Park Smash. On PowerActivated(ParkSmash) every ticket whose patron is waiting for the car is
    /// paid a full tip (speed bonus maxed, the combo multiplier read once, the combo not reset) and closed, so the car leaves.
    /// A ticket whose car the valet is driving is left alone. It also tops up the charge and wash of the cars Lot parked,
    /// and joins its half with Lot's into ParkSmashCompleted.
    /// </summary>
    public sealed class SmashSettler : IDisposable
    {
        private const int NoCombo = 1;

        private readonly IEventBus _bus;
        private readonly TicketBook _book;
        private readonly TicketSettler _settler;
        private readonly IShiftScoreView _score;
        private readonly SmashTally _tally = new SmashTally();
        private readonly List<Ticket> _scratch = new List<Ticket>();

        public SmashSettler(IEventBus bus, TicketBook book, TicketSettler settler, IShiftScoreView score = null)
        {
            _bus = bus; _book = book; _settler = settler; _score = score;
            _bus.Subscribe<PowerActivated>(OnActivated);
            _bus.Subscribe<ParkSmashCarParked>(OnCarParked);
            _bus.Subscribe<ParkSmashParked>(OnParked);
            _bus.Subscribe<ShiftStarted>(OnShiftStarted);
        }

        public void Dispose()
        {
            _bus.Unsubscribe<PowerActivated>(OnActivated);
            _bus.Unsubscribe<ParkSmashCarParked>(OnCarParked);
            _bus.Unsubscribe<ParkSmashParked>(OnParked);
            _bus.Unsubscribe<ShiftStarted>(OnShiftStarted);
        }

        private void OnActivated(PowerActivated e)
        {
            if (e.Power != PowerKind.ParkSmash) return;
            var combo = _score != null ? _score.ComboMultiplier : NoCombo;
            var delivered = 0;
            var total = 0;
            _book.CopyTo(_scratch);
            foreach (var t in _scratch)
            {
                if (t.State != TicketState.PickupRequested || t.Driven) continue;
                var tip = _settler.PaySmash(t, combo);
                _bus.Publish(new ParkSmashCarDelivered(t.TicketId, t.CarId, tip));
                delivered++;
                total += tip;
            }
            _tally.RecordDeliveries(delivered, total);
            Finish();
        }

        private void OnCarParked(ParkSmashCarParked e)
        {
            if (!_book.TryGetByCar(e.Car, out var t)) return;
            if (e.Charged && t.IsEv && t.Charge.Complete())
            {
                _bus.Publish(new CarChargeChanged(t.CarId, t.Charge.Charge01));
                _bus.Publish(new CarFullyCharged(t.CarId));
            }
            if (e.Washed && t.IsDirty && t.Wash.Complete())
            {
                _bus.Publish(new CarCleanChanged(t.CarId, t.Wash.Clean01));
                _bus.Publish(new CarCleaned(t.CarId));
            }
        }

        private void OnParked(ParkSmashParked e)
        {
            _tally.RecordParking(e.Parked, e.Charged, e.Washed);
            Finish();
        }

        private void OnShiftStarted(ShiftStarted e) => _tally.Reset();

        private void Finish()
        {
            if (_tally.TryComplete(out var done)) _bus.Publish(done);
        }
    }
}
