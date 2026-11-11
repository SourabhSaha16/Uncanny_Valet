using System.Collections.Generic;
using UnityEngine;

namespace UncannyValet.Core.Contracts
{
    /// <summary>
    /// The two one-way roads: the entry road below the lot (patron cars drive in along it, left to right) and the exit
    /// road above the lot's top wall (delivered cars leave along it, left to right). Both also carry ambient traffic.
    /// Lane routes are world-space polylines. Implemented by the Lot module, read by the Traffic module.
    /// </summary>
    public interface IRoadLayout
    {
        /// <summary>Entry road lane centre line (below the lot), left (off-screen) to right (off-screen).</summary>
        IReadOnlyList<Vector2> EastLane { get; }

        /// <summary>Exit road lane centre line (above the lot), left (off-screen) to right (off-screen).</summary>
        IReadOnlyList<Vector2> TopLane { get; }

        /// <summary>Distance along the entry lane where a car turns up into the arrival queue.</summary>
        float QueueTurnDistance { get; }

        /// <summary>Distance along the exit lane where cars leaving the curb merge in.</summary>
        float CurbMergeDistance { get; }

        /// <summary>A route from the entry lane's queue turn up to arrival slot <paramref name="slot"/> (written into a caller-owned list, cleared first).</summary>
        void QueueEntryRoute(int slot, List<Vector2> into);

        /// <summary>A route from the curb zone straight up through the top wall and round to the right, ending at the merge point on the exit lane (cleared first).</summary>
        void CurbExitRoute(List<Vector2> into);
    }
}
