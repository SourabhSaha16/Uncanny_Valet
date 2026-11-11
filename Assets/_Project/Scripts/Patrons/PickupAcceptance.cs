using UncannyValet.Core.Contracts;
using UncannyValet.Core.Ids;

namespace UncannyValet.Patrons
{
    /// <summary>Answers whether a car at the curb would be taken: its patron has asked for it (any wash or charge state).</summary>
    public sealed class PickupAcceptance : IPickupAcceptance
    {
        private readonly TicketRegistry _registry;

        public PickupAcceptance(TicketRegistry registry) { _registry = registry; }

        /// <inheritdoc />
        public bool WillAccept(CarId car)
            => _registry.TryGetByCar(car, out var t) && t.State == TicketState.PickupRequested;
    }
}
