using System.Collections.Generic;
using UnityEngine;

namespace UncannyValet.Core.Contracts
{
    /// <summary>
    /// The road below the lot. Two lanes: the eastbound lane next to the lot carries patron cars in and out; the
    /// westbound lane carries ambient traffic only. Lane routes are world-space polylines. Implemented by the Lot
    /// module, read by the Traffic module.
    /// </summary>
    public interface IRoadLayout
    {
        /// <summary>Eastbound lane centre line, left (off-screen) to right (off-screen).</summary>
        IReadOnlyList<Vector2> EastLane { get; }

        /// <summary>Westbound lane centre line, right (off-screen) to left (off-screen).</summary>
        IReadOnlyList<Vector2> WestLane { get; }

        /// <summary>Distance along the eastbound lane where a car turns up into the arrival queue.</summary>
        float QueueTurnDistance { get; }

        /// <summary>Distance along the eastbound lane where cars leaving the curb merge in.</summary>
        float CurbMergeDistance { get; }

        /// <summary>A route from the eastbound lane's queue turn up to arrival slot <paramref name="slot"/> (written into a caller-owned list, cleared first).</summary>
        void QueueEntryRoute(int slot, List<Vector2> into);

        /// <summary>A route from the curb zone down to the merge point on the eastbound lane (cleared first).</summary>
        void CurbExitRoute(List<Vector2> into);
    }
}
