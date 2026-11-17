using UncannyValet.Core.Events;

namespace UncannyValet.Patrons
{
    /// <summary>Publishes the arrival of a new ticket's car in the order listeners rely on.</summary>
    public static class TicketAnnouncer
    {
        /// <summary>CarSpawned, TicketOpened, tag, plate, modifiers, then each modifier's starting progress.</summary>
        public static void Announce(IEventBus bus, StationService stations, Ticket t)
        {
            bus.Publish(new CarSpawned(t.CarId, t.TicketId));
            bus.Publish(new TicketOpened(t.TicketId, t.CarId, t.StayMinutes));
            bus.Publish(new CarTagAssigned(t.CarId, t.Tag));
            bus.Publish(new CarPlateAssigned(t.CarId, t.Plate));
            bus.Publish(new TicketModifiersAssigned(t.TicketId, t.CarId, t.Request.Modifiers));
            stations.Announce(t);
        }
    }
}
