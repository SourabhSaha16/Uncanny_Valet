using UncannyValet.Core.Ids;
using UnityEngine;

namespace UncannyValet.Core.Contracts
{
    /// <summary>Creates and removes cars. Implemented by the Cars module.</summary>
    public interface ICarFactory
    {
        /// <summary>Spawns a car at a position and rotation (degrees). Returns the new car.</summary>
        IDrivable Spawn(CarId id, Vector2 position, float rotationDegrees);

        bool TryGet(CarId id, out IDrivable car);

        void Despawn(CarId id);
    }
}
