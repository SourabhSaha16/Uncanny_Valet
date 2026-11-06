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
        private readonly TicketTimerPublisher _timers;
        private readonly TicketBook _book;
        private readonly TicketSettler _settler;
        private readonly StationService _stations;
        private readonly LiveTicketTuning _live;
        private readonly List<Ticket> _scratch = new List<Ticket>();

        public int OpenCount => _book.Count;

        /// <summary>Tickets whose car has not been parked yet.</summary>
        public int AwaitingParkCount => _book.AwaitingParkCount;

        public TicketRegistry(IEventBus bus, IGameClock clock, ITipCalculator tips)
            : this(bus, clock, tips, ServiceTuning.Default) { }

        /// <summary>Registry whose charge, wash and window numbers follow the live tuning.</summary>
        public TicketRegistry(IEventBus bus, IGameClock clock, ITipCalculator tips, PatronTuning tuning)
            : this(bus, clock, tips, tuning.Service)
        {
            _live = new LiveTicketTuning(bus, tuning, _book);
        }

        public TicketRegistry(IEventBus bus, IGameClock clock, ITipCalculator tips, ServiceTuning tuning)
        {
            _bus = bus; _clock = clock;
            _timers = new TicketTimerPublisher(bus);
            _book = new TicketBook(tuning);
            _settler = new TicketSettler(bus, clock, tips, _timers, _book);
            _stations = new StationService(bus, clock, _book);
            _bus.Subscribe<CarParked>(OnCarParked);
            _bus.Subscribe<NightTick>(OnNightTick);
            _bus.Subscribe<CarAtCurb>(OnCarAtCurb);
            _bus.Subscribe<CarEntered>(OnCarEntered);
            _bus.Subscribe<CarExited>(OnCarExited);
            _bus.Subscribe<NightEnded>(OnNightEnded);
            _bus.Subscribe<NightStarted>(OnNightStarted);
        }

        public void Dispose()
        {
            _stations.Dispose();
            _live?.Dispose();
            _bus.Unsubscribe<CarParked>(OnCarParked);
            _bus.Unsubscribe<NightTick>(OnNightTick);
            _bus.Unsubscribe<CarAtCurb>(OnCarAtCurb);
            _bus.Unsubscribe<CarEntered>(OnCarEntered);
            _bus.Unsubscribe<CarExited>(OnCarExited);
            _bus.Unsubscribe<NightEnded>(OnNightEnded);
            _bus.Unsubscribe<NightStarted>(OnNightStarted);
        }

        public bool TryGet(TicketId id, out Ticket ticket) => _book.TryGet(id, out ticket);

        /// <summary>Opens a ticket for a car, gives it a letter and starts its park timer.</summary>
        public Ticket Open(CarId car, PatronRequest request)
        {
            var ticket = Create(car, request);
            _bus.Publish(new TicketOpened(ticket.TicketId, car, request.StayMinutes));
            return ticket;
        }

        /// <summary>Like <see cref="Open"/> but announces nothing; follow with <see cref="AnnounceArrival"/>.</summary>
        public Ticket Create(CarId car, PatronRequest request)
        {
            var ticket = _book.Add(car, request);
            ticket.ParkTimer.Start(_clock.MinuteOfNight);
            return ticket;
        }

        /// <summary>
        /// Announces a created ticket in the order listeners rely on: CarSpawned, TicketOpened, CarLetterAssigned,
        /// TicketTraitsAssigned, and only then the initial charge and clean levels.
        /// </summary>
        public void AnnounceArrival(Ticket t)
        {
            _bus.Publish(new CarSpawned(t.CarId, t.TicketId));
            _bus.Publish(new TicketOpened(t.TicketId, t.CarId, t.StayMinutes));
            _bus.Publish(new CarLetterAssigned(t.CarId, t.Letter));
            _bus.Publish(new TicketTraitsAssigned(t.TicketId, t.CarId, t.IsEv, t.IsDirty));
            _stations.Announce(t);
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
            t.Driven = true;
            t.SyncParkTimer(_clock.MinuteOfNight);
        }

        private void OnCarExited(CarExited e)
        {
            if (!_book.TryGetByCar(e.Car, out var t)) return;
            t.Driven = false;
            t.SyncParkTimer(_clock.MinuteOfNight);
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

        private void Close(Ticket t) => _settler.Pay(t);

        /// <summary>The park window ran out: the patron takes the car. No tip, no penalty.</summary>
        private void Abandon(Ticket t)
        {
            t.MarkAbandoned();
            _settler.Void(t);
        }

        private void OnNightEnded(NightEnded e) => CloseAllUnpaid();

        // A new shift starts clean: nothing should be left, but make sure of it and restart the letters.
        private void OnNightStarted(NightStarted e)
        {
            CloseAllUnpaid();
            _book.ResetLetters();
        }

        /// <summary>Night over: every open ticket ends with wait 0 and tip 0 (no TipEarned). Safe to repeat.</summary>
        private void CloseAllUnpaid()
        {
            _book.CopyTo(_scratch);
            foreach (var t in _scratch)
            {
                if (!t.MarkForceClosed()) continue;
                _timers.Stop(t, TimerKind.Park);
                _timers.Stop(t, TimerKind.Pickup);
                _settler.Void(t);
            }
        }
    }
}
