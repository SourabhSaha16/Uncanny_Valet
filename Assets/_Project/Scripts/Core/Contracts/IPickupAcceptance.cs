using UncannyValet.Core.Ids;

namespace UncannyValet.Core.Contracts
{
    /// <summary>Whether handing a car back at the curb would be accepted right now. Implemented by the Patrons module.</summary>
    public interface IPickupAcceptance
    {
        /// <summary>True when the car's patron is waiting for it, so delivering it closes the ticket.</summary>
        bool WillAccept(CarId car);
    }
}
