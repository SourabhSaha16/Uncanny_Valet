using UncannyValet.Core.Events;

namespace UncannyValet.Patrons
{
    /// <summary>Advances a ticket's timers and announces progress, expiry and stops on the bus.</summary>
    public sealed class TicketTimerPublisher
    {
        private readonly IEventBus _bus;

        public TicketTimerPublisher(IEventBus bus) { _bus = bus; }

        /// <summary>Ticks one timer for this game minute. Returns true if it expired on this call.</summary>
        public bool Tick(Ticket t, TimerKind kind, float minute)
        {
            var timer = t.PickupTimer;
            if (!timer.IsActive || timer.IsExpired) return false;
            var expired = timer.Tick(minute);
            _bus.Publish(new TicketTimerChanged(t.TicketId, t.CarId, kind, timer.Elapsed01));
            if (expired) _bus.Publish(new TicketTimerExpired(t.TicketId, t.CarId, kind));
            return expired;
        }

        /// <summary>Stops one timer and announces it, unless it was never running.</summary>
        public void Stop(Ticket t, TimerKind kind)
        {
            var timer = t.PickupTimer;
            if (timer.Stop()) _bus.Publish(new TicketTimerStopped(t.TicketId, t.CarId));
        }
    }
}
