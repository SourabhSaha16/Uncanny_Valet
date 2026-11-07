using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Patrons
{
    /// <summary>Ends tickets: pays a delivered one through the tip calculator, or voids one with no tip.</summary>
    public sealed class TicketSettler
    {
        private readonly IEventBus _bus;
        private readonly IGameClock _clock;
        private readonly ITipCalculator _tips;
        private readonly TicketTimerPublisher _timers;
        private readonly TicketBook _book;

        public TicketSettler(IEventBus bus, IGameClock clock, ITipCalculator tips, TicketTimerPublisher timers, TicketBook book)
        {
            _bus = bus; _clock = clock; _tips = tips; _timers = timers; _book = book;
        }

        /// <summary>The car was handed back: announce the delivery, explain and pay the tip, then close.</summary>
        public void Pay(Ticket t)
        {
            var wait = _clock.MinuteOfNight - t.PickupRequestMinute;
            var input = new TipInput(t.Request.BaseTip, wait, t.Request.Impatient, t.PickupExpired,
                t.ChargedEv, t.CleanedDirty, t.UncleanedDirty);
            var breakdown = _tips.Explain(input);
            t.MarkClosed();
            _timers.Stop(t, TimerKind.Pickup);
            _book.Remove(t);
            _bus.Publish(new TipExplained(t.TicketId, t.CarId, breakdown));
            _bus.Publish(new CarDelivered(t.TicketId, t.CarId));
            _bus.Publish(new TicketClosed(t.TicketId, t.CarId, wait, breakdown.Total));
            _bus.Publish(new TipEarned(t.TicketId, breakdown.Total));
        }

        /// <summary>The ticket ends without a delivery (patron left, night over): tip 0, not a penalty.</summary>
        public void Void(Ticket t)
        {
            _book.Remove(t);
            var none = new TipBreakdown(0, 0, 0, 0, 0, TipZeroReason.None);
            _bus.Publish(new TipExplained(t.TicketId, t.CarId, none));
            _bus.Publish(new TicketClosed(t.TicketId, t.CarId, 0f, 0));
        }
    }
}
