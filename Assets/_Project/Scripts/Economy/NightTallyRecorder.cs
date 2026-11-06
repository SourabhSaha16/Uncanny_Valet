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

        /// <summary>Create from Bootstrap with the shared bus, the debt view and RunConfig.Nights. Call Dispose on teardown.</summary>
        public NightTallyRecorder(IEventBus bus, IDebtView debt, int totalNights)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _debt = debt ?? throw new ArgumentNullException(nameof(debt));
            _tally = new NightTally(totalNights);
            _bus.Subscribe<NightStarted>(OnNightStarted);
            _bus.Subscribe<TicketOpened>(OnTicketOpened);
            _bus.Subscribe<CarParked>(OnCarParked);
            _bus.Subscribe<TicketClosed>(OnTicketClosed);
            _bus.Subscribe<NightEnded>(OnNightEnded);
        }

        public void Dispose()
        {
            _bus.Unsubscribe<NightStarted>(OnNightStarted);
            _bus.Unsubscribe<TicketOpened>(OnTicketOpened);
            _bus.Unsubscribe<CarParked>(OnCarParked);
            _bus.Unsubscribe<TicketClosed>(OnTicketClosed);
            _bus.Unsubscribe<NightEnded>(OnNightEnded);
        }

        private void OnNightStarted(NightStarted e) => _tally.Start(e.Night, _debt.DebtRemaining);
        private void OnTicketOpened(TicketOpened e) => _tally.RecordTicketOpened(e.Ticket, e.Car);
        private void OnCarParked(CarParked e) => _tally.RecordCarParked(e.Car);
        private void OnTicketClosed(TicketClosed e) => _tally.RecordTicketClosed(e.Ticket, e.Car, e.Tip);

        private void OnNightEnded(NightEnded e)
        {
            if (!_tally.IsActive) return;
            _bus.Publish(new NightSummaryReady(_tally.End()));
        }
    }
}
