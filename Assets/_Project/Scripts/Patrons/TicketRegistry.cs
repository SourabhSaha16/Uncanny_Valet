using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;

namespace UncannyValet.Patrons
{
    /// <summary>Owns every ticket. Reacts to park, tick, drive and curb events; pays out the tip on close.</summary>
    public sealed class TicketRegistry
    {
        private readonly IEventBus _bus;
        private readonly IGameClock _clock;
        private readonly ITipCalculator _tips;
        private readonly TicketTimerPublisher _timers;
        private readonly TicketBook _book = new TicketBook();
        private readonly List<Ticket> _scratch = new List<Ticket>();

        public int OpenCount => _book.Count;

        /// <summary>Tickets whose car has not been parked yet.</summary>
        public int AwaitingParkCount => _book.AwaitingParkCount;

        public TicketRegistry(IEventBus bus, IGameClock clock, ITipCalculator tips)
        {
            _bus = bus; _clock = clock; _tips = tips;
            _timers = new TicketTimerPublisher(bus);
            _bus.Subscribe<CarParked>(OnCarParked);
            _bus.Subscribe<NightTick>(OnNightTick);
            _bus.Subscribe<CarAtCurb>(OnCarAtCurb);
            _bus.Subscribe<CarEntered>(OnCarEntered);
            _bus.Subscribe<CarExited>(OnCarExited);
        }

        public void Dispose()
        {
            _bus.Unsubscribe<CarParked>(OnCarParked);
            _bus.Unsubscribe<NightTick>(OnNightTick);
            _bus.Unsubscribe<CarAtCurb>(OnCarAtCurb);
            _bus.Unsubscribe<CarEntered>(OnCarEntered);
            _bus.Unsubscribe<CarExited>(OnCarExited);
        }

        public bool TryGet(TicketId id, out Ticket ticket) => _book.TryGet(id, out ticket);

        /// <summary>Opens a ticket for a car, gives it a letter and starts its park timer.</summary>
        public Ticket Open(CarId car, PatronRequest request)
        {
            var ticket = _book.Add(car, request);
            ticket.ParkTimer.Start(_clock.MinuteOfNight);
            _bus.Publish(new TicketOpened(ticket.TicketId, car, request.StayMinutes));
            return ticket;
        }

        private void OnCarParked(CarParked e)
        {
            if (!_book.TryGetByCar(e.Car, out var t)) return;
            t.AtCurb = false;
            if (t.State != TicketState.AwaitingPark) return;
            _timers.Stop(t, TimerKind.Park);
            t.MarkParked(_clock.MinuteOfNight);
        }

        private void OnCarEntered(CarEntered e)
        {
            if (!_book.TryGetByCar(e.Car, out var t)) return;
            t.AtCurb = false;
            t.ParkTimer.Pause(_clock.MinuteOfNight);
        }

        private void OnCarExited(CarExited e)
        {
            if (_book.TryGetByCar(e.Car, out var t)) t.ParkTimer.Resume(_clock.MinuteOfNight);
        }

        private void OnCarAtCurb(CarAtCurb e)
        {
            if (!_book.TryGetByCar(e.Car, out var t)) return;
            t.AtCurb = true;
            if (t.State == TicketState.PickupRequested) Close(t);
        }

        private void OnNightTick(NightTick e)
        {
            _book.CopyTo(_scratch);
            foreach (var t in _scratch) Advance(t, e.MinuteOfNight);
        }

        private void Advance(Ticket t, float minute)
        {
            if (t.State == TicketState.AwaitingPark)
            {
                if (_timers.Tick(t, TimerKind.Park, minute)) Abandon(t);
            }
            else if (t.StayEnded(minute))
            {
                t.MarkPickupRequested(minute);
                t.PickupTimer.Start(minute);
                _bus.Publish(new PickupRequested(t.TicketId, t.CarId));
                if (t.AtCurb) Close(t);
            }
            else if (t.State == TicketState.PickupRequested)
            {
                _timers.Tick(t, TimerKind.Pickup, minute);
            }
        }

        private void Close(Ticket t)
        {
            var wait = _clock.MinuteOfNight - t.PickupRequestMinute;
            var tip = _tips.Calculate(new TipInput(t.Request.BaseTip, wait, t.Request.Impatient, t.PickupExpired));
            t.MarkClosed();
            _timers.Stop(t, TimerKind.Pickup);
            _book.Remove(t);
            _bus.Publish(new TicketClosed(t.TicketId, t.CarId, wait, tip));
            _bus.Publish(new TipEarned(t.TicketId, tip));
        }

        /// <summary>The park window ran out: the patron takes the car. No tip, no penalty.</summary>
        private void Abandon(Ticket t)
        {
            t.MarkAbandoned();
            _book.Remove(t);
            _bus.Publish(new TicketClosed(t.TicketId, t.CarId, 0f, 0));
        }
    }
}
