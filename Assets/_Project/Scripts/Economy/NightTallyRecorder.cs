using System;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Economy
{
    /// <summary>Feeds a <see cref="NightTally"/> from the event bus and publishes NightSummaryReady once per night.</summary>
    public sealed class NightTallyRecorder : IDisposable
    {
        private readonly IEventBus _bus;
        private readonly IDebtView _debt;
        private readonly NightTally _tally;
        private readonly Func<int, string> _nextNote;
        private readonly int _totalNights;
        private bool _runLost;

        /// <summary>
        /// Create from Bootstrap with the shared bus, the debt view, RunConfig.Nights and nextNote, which maps a
        /// 1-based night to its "what is new tonight" text (e.g. n => plans.For(n).NewTonight). Null means no
        /// notes. Call Dispose on teardown.
        /// </summary>
        public NightTallyRecorder(IEventBus bus, IDebtView debt, int totalNights, Func<int, string> nextNote = null)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _debt = debt ?? throw new ArgumentNullException(nameof(debt));
            _tally = new NightTally(totalNights);
            _totalNights = totalNights;
            _nextNote = nextNote;
            _bus.Subscribe<TipExplained>(OnTipExplained);
            _bus.Subscribe<NightStarted>(OnNightStarted);
            _bus.Subscribe<TicketOpened>(OnTicketOpened);
            _bus.Subscribe<CarParked>(OnCarParked);
            _bus.Subscribe<TicketClosed>(OnTicketClosed);
            _bus.Subscribe<NightEnded>(OnNightEnded);
            _bus.Subscribe<StrikeGiven>(OnStrikeGiven);
        }

        public void Dispose()
        {
            _bus.Unsubscribe<TipExplained>(OnTipExplained);
            _bus.Unsubscribe<NightStarted>(OnNightStarted);
            _bus.Unsubscribe<TicketOpened>(OnTicketOpened);
            _bus.Unsubscribe<CarParked>(OnCarParked);
            _bus.Unsubscribe<TicketClosed>(OnTicketClosed);
            _bus.Unsubscribe<NightEnded>(OnNightEnded);
            _bus.Unsubscribe<StrikeGiven>(OnStrikeGiven);
        }

        private void OnNightStarted(NightStarted e)
        {
            _runLost = false;
            _tally.Start(e.Night, _debt.DebtRemaining);
        }

        private void OnStrikeGiven(StrikeGiven e)
        {
            if (e.IsFinal && _tally.IsActive) _runLost = true;
        }

        private void OnTicketOpened(TicketOpened e) => _tally.RecordTicketOpened(e.Ticket, e.Car);
        private void OnCarParked(CarParked e) => _tally.RecordCarParked(e.Car);
        private void OnTicketClosed(TicketClosed e) => _tally.RecordTicketClosed(e.Ticket, e.Car, e.Tip);

        private void OnTipExplained(TipExplained e) => _tally.RecordTipExplained(e.Breakdown);

        private void OnNightEnded(NightEnded e)
        {
            if (!_tally.IsActive) return;
            var lost = _runLost;
            _runLost = false;
            var note = _nextNote != null && e.Night < _totalNights && !lost ? _nextNote(e.Night + 1) : "";
            _bus.Publish(new NightSummaryReady(_tally.End(note, lost)));
        }
    }
}
