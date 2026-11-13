using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Patrons
{
    /// <summary>Ends tickets: pays a delivered one through the blessing calculator, or voids one with no blessing.</summary>
    public sealed class TicketSettler
    {
        private const int NoFavour = 1;

        private readonly IEventBus _bus;
        private readonly IGameClock _clock;
        private readonly IBlessingCalculator _blessings;
        private readonly TicketTimerPublisher _timers;
        private readonly TicketBook _book;
        private readonly IShiftBlessingsView _score;
        private readonly DamageLedger _damage;

        /// <param name="score">Supplies the favour multiplier; null means no favour (1x).</param>
        public TicketSettler(IEventBus bus, IGameClock clock, IBlessingCalculator blessings, TicketTimerPublisher timers,
            TicketBook book, IShiftBlessingsView score = null,
            DamageLedger damage = null)
        {
            _damage = damage; _bus = bus; _clock = clock; _blessings = blessings; _timers = timers; _book = book; _score = score;
        }

        /// <summary>The car was handed back: announce the delivery, explain and pay the blessing, then close.</summary>
        public void Pay(Ticket t)
        {
            var favour = _score != null ? _score.FavourMultiplier : NoFavour;
            Settle(t, favour);
        }

        private void Settle(Ticket t, int favour)
        {
            var wait = _clock.ElapsedMinutes - t.PickupRequestMinute;
            var patience = t.PickupExpired ? 0f : 1f - t.PickupTimer.Elapsed01;
            var input = new BlessingInput(t.Request.BaseBlessing, wait, t.Request.Impatient, t.PickupExpired,
                t.ClearedModifiers, t.OpenModifiers, patienceRemaining01: patience, favourMultiplier: favour,
                damage01: _damage?.Take(t.CarId) ?? 0f);
            var breakdown = _blessings.Explain(input);
            t.MarkClosed();
            _timers.Stop(t, TimerKind.Pickup);
            _book.Remove(t);
            _bus.Publish(new BlessingExplained(t.TicketId, t.CarId, breakdown));
            _bus.Publish(new CarDelivered(t.TicketId, t.CarId));
            _bus.Publish(new TicketClosed(t.TicketId, t.CarId, wait, breakdown.Total));
            _bus.Publish(new BlessingEarned(t.TicketId, breakdown.Total));
        }

        /// <summary>The ticket ends without a delivery (patron left, shift over): blessing 0, not a penalty.</summary>
        public void Void(Ticket t)
        {
            _book.Remove(t);
            _damage?.Clear(t.CarId);
            var none = new BlessingBreakdown(0, 0, 0, 0, BlessingZeroReason.None);
            _bus.Publish(new BlessingExplained(t.TicketId, t.CarId, none));
            _bus.Publish(new TicketClosed(t.TicketId, t.CarId, 0f, 0));
        }
    }
}
