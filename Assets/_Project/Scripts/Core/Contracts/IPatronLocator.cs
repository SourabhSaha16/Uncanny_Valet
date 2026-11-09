using UncannyValet.Core.Ids;
using UnityEngine;

namespace UncannyValet.Core.Contracts
{
    /// <summary>Where the waiting patron of a car stands. Implemented by the Pedestrians module.</summary>
    public interface IPatronLocator
    {
        /// <summary>True and the patron's world position when that car's patron is at the curb.</summary>
        bool TryGetWaitingPatron(CarId car, out Vector2 position);
    }
}
