using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Patrons
{
    /// <summary>
    /// Applies a changed tuning value to open tickets. Charge and wash minutes reach tickets created afterwards;
    /// park and pickup windows also reach timers already running (progress so far is kept).
    /// </summary>
    public sealed class LiveTicketTuning
    {
        private readonly IEventBus _bus;
        private readonly PatronTuning _tuning;
        private readonly TicketBook _book;
        private readonly List<Ticket> _scratch = new List<Ticket>();

        public LiveTicketTuning(IEventBus bus, PatronTuning tuning, TicketBook book)
        {
            _bus = bus; _tuning = tuning; _book = book;
            _bus.Subscribe<TuningChanged>(OnChanged);
        }

        public void Dispose() => _bus.Unsubscribe<TuningChanged>(OnChanged);

        private void OnChanged(TuningChanged e)
        {
            switch (e.Key)
            {
                case TuningKey.ChargeMinutes:
                case TuningKey.WashMinutes:
                    _book.Tuning = _tuning.Service;
                    break;
                case TuningKey.ParkWindowMinutes:
                    _book.CopyTo(_scratch);
                    foreach (var t in _scratch) t.ParkTimer.SetLength(e.Value);
                    break;
                case TuningKey.PickupWindowMinutes:
                    _book.CopyTo(_scratch);
                    foreach (var t in _scratch) t.PickupTimer.SetLength(e.Value);
                    break;
            }
        }
    }
}
