using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;

namespace UncannyValet.Core.Contracts
{
    /// <summary>One station slot (the wash slot or one EV bay) and what it holds.</summary>
    public readonly struct StationSlot
    {
        public readonly StationKind Kind;
        public readonly StationSlotState State;
        /// <summary>The car in it; <see cref="CarId.None"/> when free or reserved.</summary>
        public readonly CarId Car;

        public StationSlot(StationKind kind, StationSlotState state, CarId car)
        {
            Kind = kind; State = state; Car = car;
        }
    }
}
