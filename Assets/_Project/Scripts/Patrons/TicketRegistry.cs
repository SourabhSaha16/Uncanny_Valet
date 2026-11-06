using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;

namespace UncannyValet.Patrons
{
    /// <summary>Owns every ticket. Reacts to park, tick and curb events; pays out the tip on close.</summary>
    public class TicketRegistry
    {
        private readonly IEventBus _bus;
        private readonly IGameClock _clock;
        private readonly ITipCalculator _tips;
        private readonly Dictionary<int, Ticket> _byTicket = new Dictionary<int, Ticket>();
        private readonly Dictionary<int, Ticket> _byCar = new Dictionary<int, Ticket>();
        private readonly List<Ticket> _scratch = new List<Ticket>();
        private int _nextId = 1;

        public int OpenCount { get; private set; }

        public TicketRegistry(IEventBus bus, IGameClock clock, ITipCalculator tips)
        {
            _bus = bus; _clock = clock; _tips = tips;
            _bus.Subscribe<CarParked>(OnCarParked);
            _bus.Subscribe<NightTick>(OnNightTick);
            _bus.Subscribe<CarAtCurb>(OnCarAtCurb);
            _bus.Subscribe<CarEntered>(OnCarEntered);
        }

        public void Dispose()
        {
            _bus.Unsubscribe<CarParked>(OnCarParked);
            _bus.Unsubscribe<NightTick>(OnNightTick);
            _bus.Unsubscribe<CarAtCurb>(OnCarAtCurb);
            _bus.Unsubscribe<CarEntered>(OnCarEntered);
        }

        public bool TryGet(TicketId id, out Ticket ticket) => _byTicket.TryGetValue(id.Value, out ticket);

        public Ticket Open(CarId car, PatronRequest request)
        {
            var ticket = new Ticket(new TicketId(_nextId++), car, request);
            _byTicket[ticket.TicketId.Value] = ticket;
            _byCar[car.Value] = ticket;
            OpenCount++;
            _bus.Publish(new TicketOpened(ticket.TicketId, car, request.StayMinutes));
            return ticket;
        }

        private void OnCarParked(CarParked e)
        {
            if (!_byCar.TryGetValue(e.Car.Value, out var t)) return;
            t.AtCurb = false;
            t.MarkParked(_clock.MinuteOfNight);
        }

        private void OnCarEntered(CarEntered e)
        {
            if (_byCar.TryGetValue(e.Car.Value, out var t)) t.AtCurb = false;
        }

        private void OnNightTick(NightTick e)
        {
            _scratch.Clear();
            _scratch.AddRange(_byTicket.Values);
            foreach (var t in _scratch)
            {
                if (!t.StayEnded(e.MinuteOfNight)) continue;
                t.MarkPickupRequested(e.MinuteOfNight);
                _bus.Publish(new PickupRequested(t.TicketId, t.CarId));
                if (t.AtCurb) Close(t);
            }
        }

        private void OnCarAtCurb(CarAtCurb e)
        {
            if (!_byCar.TryGetValue(e.Car.Value, out var t)) return;
            t.AtCurb = true;
            if (t.State == TicketState.PickupRequested) Close(t);
        }

        private void Close(Ticket t)
        {
            var wait = _clock.MinuteOfNight - t.PickupRequestMinute;
            var tip = _tips.Calculate(new TipInput(t.Request.BaseTip, wait, t.Request.Impatient));
            t.MarkClosed();
            OpenCount--;
            _byTicket.Remove(t.TicketId.Value);
            _byCar.Remove(t.CarId.Value);
            _bus.Publish(new TicketClosed(t.TicketId, t.CarId, wait, tip));
            _bus.Publish(new TipEarned(t.TicketId, tip));
        }
    }
}
