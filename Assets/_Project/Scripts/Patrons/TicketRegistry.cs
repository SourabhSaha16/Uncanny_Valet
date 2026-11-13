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
        private readonly CurbCall _call;
        private readonly LiveTicketTuning _live;
        private readonly PerfectBonusLedger _perfect;
        private readonly DamageLedger _damage;
        private readonly ReturnShifter _returns;
        private readonly StayStarter _stays;
        private readonly List<Ticket> _scratch = new List<Ticket>();

        /// <summary>Game minutes a summoned patron may walk before the pickup is requested anyway; 0 = no walk (see <see cref="CurbCall"/>).</summary>
        public float CurbWalkTimeoutMinutes { get => _call.TimeoutMinutes; set => _call.TimeoutMinutes = value; }

        public int OpenCount => _book.Count;

        /// <summary>Tickets whose car has not been parked yet.</summary>
        public int AwaitingParkCount => _book.AwaitingParkCount;

        public TicketRegistry(IEventBus bus, IGameClock clock, ITipCalculator tips, IShiftScoreView score = null)
            : this(bus, clock, tips, ServiceTuning.Default, score) { }

        /// <summary>Registry whose charge, wash and window numbers follow the live tuning.</summary>
        public TicketRegistry(IEventBus bus, IGameClock clock, ITipCalculator tips, PatronTuning tuning,
            IShiftScoreView score = null)
            : this(bus, clock, tips, tuning.Service, score)
        {
            _live = new LiveTicketTuning(bus, tuning, _book);
        }

        public TicketRegistry(IEventBus bus, IGameClock clock, ITipCalculator tips, ServiceTuning tuning,
            IShiftScoreView score = null, PlatePool plates = null)
        {
            _bus = bus; _clock = clock;
            _timers = new TicketTimerPublisher(bus);
            _book = new TicketBook(tuning, plates);
            _perfect = new PerfectBonusLedger(bus);
            _damage = new DamageLedger(bus);
            _settler = new TicketSettler(bus, clock, tips, _timers, _book, score, _perfect, _damage);
            _stations = new StationService(bus, _book);
            _returns = new ReturnShifter(bus, clock, _book);
            _stays = new StayStarter(bus, clock, _book);
            _call = new CurbCall(bus);
            _bus.Subscribe<CarParked>(OnCarParked);
            _bus.Subscribe<ShiftTick>(OnShiftTick);
            _bus.Subscribe<CarAtCurb>(OnCarAtCurb);
            _bus.Subscribe<PatronReachedCurb>(OnPatronReachedCurb);
            _bus.Subscribe<CarEntered>(OnCarEntered);
            _bus.Subscribe<CarExited>(OnCarExited);
            _bus.Subscribe<ShiftEnded>(OnShiftEnded);
            _bus.Subscribe<ShiftStarted>(OnShiftStarted);
        }

        public void Dispose()
        {
            _stations.Dispose();
            _returns.Dispose();
            _stays.Dispose();
            _perfect.Dispose();
            _damage.Dispose();
            _live?.Dispose();
            _bus.Unsubscribe<CarParked>(OnCarParked);
            _bus.Unsubscribe<ShiftTick>(OnShiftTick);
            _bus.Unsubscribe<CarAtCurb>(OnCarAtCurb);
            _bus.Unsubscribe<PatronReachedCurb>(OnPatronReachedCurb);
            _bus.Unsubscribe<CarEntered>(OnCarEntered);
            _bus.Unsubscribe<CarExited>(OnCarExited);
            _bus.Unsubscribe<ShiftEnded>(OnShiftEnded);
            _bus.Unsubscribe<ShiftStarted>(OnShiftStarted);
        }

        public bool TryGet(TicketId id, out Ticket ticket) => _book.TryGet(id, out ticket);

        /// <summary>Finds the open ticket of a car.</summary>
        public bool TryGetByCar(CarId car, out Ticket ticket) => _book.TryGetByCar(car, out ticket);

        /// <summary>Opens a ticket for a car and gives it a tag colour.</summary>
        public Ticket Open(CarId car, PatronRequest request)
        {
            var ticket = Create(car, request);
            _bus.Publish(new TicketOpened(ticket.TicketId, car, request.StayMinutes));
            return ticket;
        }

        /// <summary>Like <see cref="Open"/> but announces nothing; follow with <see cref="AnnounceArrival"/>.</summary>
        public Ticket Create(CarId car, PatronRequest request)
        {
            return _book.Add(car, request);
        }

        /// <summary>Announces a created ticket in listener order: CarSpawned, TicketOpened, tag, plate, traits, then initial levels.</summary>
        public void AnnounceArrival(Ticket t)
        {
            _bus.Publish(new CarSpawned(t.CarId, t.TicketId));
            _bus.Publish(new TicketOpened(t.TicketId, t.CarId, t.StayMinutes));
            _bus.Publish(new CarTagAssigned(t.CarId, t.Tag));
            _bus.Publish(new CarPlateAssigned(t.CarId, t.Plate));
            _bus.Publish(new TicketTraitsAssigned(t.TicketId, t.CarId, t.IsEv, t.IsDirty));
            _stations.Announce(t);
        }

        private void OnCarParked(CarParked e)
        {
            if (!_book.TryGetByCar(e.Car, out var t)) return;
            t.AtCurb = false;
            t.MarkParked();
        }

        private void OnCarEntered(CarEntered e)
        {
            if (!_book.TryGetByCar(e.Car, out var t)) return;
            t.AtCurb = false;
            t.Driven = true;
        }

        private void OnCarExited(CarExited e)
        {
            if (!_book.TryGetByCar(e.Car, out var t)) return;
            t.Driven = false;
        }

        private void OnCarAtCurb(CarAtCurb e)
        {
            if (!_book.TryGetByCar(e.Car, out var t)) return;
            t.AtCurb = true;
            if (t.State == TicketState.PickupRequested) Close(t);
        }

        private void OnShiftTick(ShiftTick e)
        {
            _book.CopyTo(_scratch);
            foreach (var t in _scratch) Advance(t, e.ElapsedMinutes);
        }

        private void Advance(Ticket t, float minute)
        {
            if (t.StayEnded(minute))
            {
                if (_call.ShouldRequest(t, minute)) RequestPickup(t, minute);
            }
            else if (t.State == TicketState.PickupRequested)
            {
                _timers.Tick(t, TimerKind.Pickup, minute);
            }
        }

        private void OnPatronReachedCurb(PatronReachedCurb e)
        {
            if (_book.TryGetByCar(e.Car, out var t) && CurbCall.AcceptsArrival(t)) RequestPickup(t, _clock.ElapsedMinutes);
        }

        private void RequestPickup(Ticket t, float minute)
        {
            t.MarkPickupRequested(minute);
            t.PickupTimer.Start(minute);
            _bus.Publish(new PickupRequested(t.TicketId, t.CarId));
            if (t.AtCurb) Close(t);
        }

        private void Close(Ticket t) => _settler.Pay(t);

        private void OnShiftEnded(ShiftEnded e) => CloseAllUnpaid();

        // A new shift starts clean: nothing should be left, but make sure of it and restart the tag colours.
        private void OnShiftStarted(ShiftStarted e)
        {
            CloseAllUnpaid();
            _book.ResetTags();
        }

        /// <summary>Shift over: every open ticket ends with wait 0 and tip 0 (no TipEarned). Safe to repeat.</summary>
        private void CloseAllUnpaid()
        {
            _book.CopyTo(_scratch);
            foreach (var t in _scratch)
            {
                if (!t.MarkForceClosed()) continue;
                _timers.Stop(t, TimerKind.Pickup);
                _settler.Void(t);
            }
        }
    }
}
