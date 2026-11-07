using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Patrons
{
    /// <summary>On TicketClosed the patron drives off: announce it, then the car drives off and is removed.</summary>
    public sealed class PatronDeparture
    {
        private readonly IEventBus _bus;
        private readonly ICarFactory _cars;

        public PatronDeparture(IEventBus bus, ICarFactory cars)
        {
            _bus = bus; _cars = cars;
            _bus.Subscribe<TicketClosed>(OnTicketClosed);
        }

        public void Dispose() => _bus.Unsubscribe<TicketClosed>(OnTicketClosed);

        private void OnTicketClosed(TicketClosed e)
        {
            _bus.Publish(new CarDeparted(e.Car));
            _cars.DriveAwayAndDespawn(e.Car);
        }
    }
}
