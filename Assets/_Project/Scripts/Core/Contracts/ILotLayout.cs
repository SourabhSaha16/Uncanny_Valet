using System.Collections.Generic;
using UnityEngine;

namespace UncannyValet.Core.Contracts
{
    /// <summary>Where things are in the lot. Implemented by the Lot module.</summary>
    public interface ILotLayout
    {
        /// <summary>Where new patron cars appear.</summary>
        Vector2 ArrivalPoint { get; }

        float ArrivalRotationDegrees { get; }

        /// <summary>Ordered spots where waiting cars queue. Slot 0 is the ArrivalPoint.</summary>
        IReadOnlyList<Vector2> ArrivalSlots { get; }

        /// <summary>Where the valet stands when the night starts.</summary>
        Vector2 ValetStartPoint { get; }

        IReadOnlyList<int> SpotIds { get; }

        /// <summary>Regular numbered spots the lot has (ids 1..n), open or not; the stations are not counted.</summary>
        int RegularSpotCount { get; }

        /// <summary>EV charging bays the lot has (ids n + 1..), open or not.</summary>
        int ChargingSpotCount { get; }

        /// <summary>True when the lot has a wash slot (a lot with its services closed has none).</summary>
        bool HasWashStation { get; }

        bool IsSpotFree(int spotId);

        /// <summary>True for the charging spots (specialised spots only open on charging nights).</summary>
        bool IsChargingSpot(int spotId);
    }
}
