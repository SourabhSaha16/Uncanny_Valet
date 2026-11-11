using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Patrons
{
    /// <summary>Ends tickets: pays a delivered one through the tip calculator, or voids one with no tip.</summary>
    public sealed class TicketSettler
    {
        private const int NoCombo = 1;

        private readonly IEventBus _bus;
        private readonly IGameClock _clock;
        private readonly ITipCalculator _tips;
        private readonly TicketTimerPublisher _timers;
        private readonly TicketBook _book;
        private readonly IShiftScoreView _score;
        private readonly PerfectBonusLedger _perfect;

        /// <param name="score">Supplies the combo multiplier; null means no combo (1x).</param>
        public TicketSettler(IEventBus bus, IGameClock clock, ITipCalculator tips, TicketTimerPublisher timers,
            TicketBook book, IShiftScoreView score = null,
            PerfectBonusLedger perfect = null)
        {
            _perfect = perfect; _bus = bus; _clock = clock; _tips = tips; _timers = timers; _book = book; _score = score;
        }

        /// <summary>The car was handed back: announce the delivery, explain and pay the tip, then close.</summary>
        public void Pay(Ticket t)
        {
            var combo = _score != null ? _score.ComboMultiplier : NoCombo;
            Settle(t, combo, false, true);
        }

        /// <summary>
        /// Park Smash: pay the full tip as if the car came back instantly (speed bonus maxed) at a combo multiplier fixed
        /// by the caller, and close the ticket without a delivery (no CarDelivered, so the patron vanishes and the car leaves at once).
        /// Returns the tip paid.
        /// </summary>
        public int PaySmash(Ticket t, int comboMultiplier) => Settle(t, comboMultiplier, true, false);

        private int Settle(Ticket t, int combo, bool maxSpeed, bool delivered)
        {
            var wait = _clock.ElapsedMinutes - t.PickupRequestMinute;
            var patience = t.PickupExpired ? 0f : 1f - t.PickupTimer.Elapsed01;
            var input = new TipInput(t.Request.BaseTip, wait, t.Request.Impatient, t.PickupExpired,
                t.ChargedEv, t.CleanedDirty, t.UncleanedDirty, patienceRemaining01: patience, comboMultiplier: combo,
                perfectParks: _perfect?.Take(t.TicketId) ?? 0, maxSpeedBonus: maxSpeed);
            var breakdown = _tips.Explain(input);
            t.MarkClosed();
            _timers.Stop(t, TimerKind.Pickup);
            _book.Remove(t);
            _bus.Publish(new TipExplained(t.TicketId, t.CarId, breakdown));
            if (delivered) _bus.Publish(new CarDelivered(t.TicketId, t.CarId));
            _bus.Publish(new TicketClosed(t.TicketId, t.CarId, wait, breakdown.Total));
            _bus.Publish(new TipEarned(t.TicketId, breakdown.Total));
            return breakdown.Total;
        }

        /// <summary>The ticket ends without a delivery (patron left, shift over): tip 0, not a penalty.</summary>
        public void Void(Ticket t)
        {
            _book.Remove(t);
            _perfect?.Clear(t.TicketId);
            var none = new TipBreakdown(0, 0, 0, 0, 0, TipZeroReason.None);
            _bus.Publish(new TipExplained(t.TicketId, t.CarId, none));
            _bus.Publish(new TicketClosed(t.TicketId, t.CarId, 0f, 0));
        }
    }
}
