using UncannyValet.Core.Ids;
using UnityEngine;

namespace UncannyValet.Core.Contracts
{
    /// <summary>Creates and removes cars. Implemented by the Cars module.</summary>
    public interface ICarFactory
    {
        /// <summary>Spawns a car at a position and rotation (degrees). Returns the new car.</summary>
        IDrivable Spawn(CarId id, Vector2 position, float rotationDegrees);

        /// <summary>
        /// Spawns a car off the lot, behind <paramref name="stopPosition"/>, and drives it in on its own until it
        /// stops there facing <paramref name="rotationDegrees"/>. The valet cannot enter it until it has arrived.
        /// With a road (<see cref="IEntryRouter"/> registered) the car instead waits off-screen and drives the road to
        /// arrival slot <paramref name="arrivalSlot"/>.
        /// </summary>
        IDrivable SpawnArriving(CarId id, Vector2 stopPosition, float rotationDegrees, int arrivalSlot);

        /// <summary>
        /// Drives a car out of view on its own, then removes it. A car at the curb leaves along the road
        /// (<see cref="IExitRouter"/>); any other car drives straight out. Same as Despawn if the car is missing.
        /// </summary>
        void DriveAwayAndDespawn(CarId id);

        bool TryGet(CarId id, out IDrivable car);

        void Despawn(CarId id);
    }
}
