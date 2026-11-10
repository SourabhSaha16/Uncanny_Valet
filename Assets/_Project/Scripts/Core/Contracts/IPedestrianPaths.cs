using System.Collections.Generic;
using UnityEngine;

namespace UncannyValet.Core.Contracts
{
    /// <summary>
    /// Where people may walk: the diner door, the curb waiting spots and the routes between the road, the crosswalks,
    /// the lot and the door. All routes are world-space polylines written into a caller-owned list (cleared first).
    /// Implemented by the Lot module.
    /// </summary>
    public interface IPedestrianPaths
    {
        /// <summary>The diner door, where people vanish and appear.</summary>
        Vector2 DinerDoor { get; }

        /// <summary>The middle of the curb zone (the valet pickup spot), which waiting patrons face.</summary>
        Vector2 CurbCenter { get; }

        /// <summary>Number of standing spots at the curb.</summary>
        int CurbSlotCount { get; }

        /// <summary>World position of a curb standing spot.</summary>
        Vector2 CurbSlot(int slot);

        /// <summary>Number of walks from the road side to the door.</summary>
        int RoadRouteCount { get; }

        /// <summary>A walk from off-screen at the road side to the door (or the same walk reversed when <paramref name="leaving"/>).</summary>
        void RoadRoute(int index, bool leaving, List<Vector2> into);

        /// <summary>A walk from a point in the lot (for example a parked car) via the crosswalks to the door.</summary>
        void RouteToDoor(Vector2 from, List<Vector2> into);

        /// <summary>A walk from the door via the crosswalks to a curb standing spot.</summary>
        void RouteToCurb(int slot, List<Vector2> into);
    }
}
