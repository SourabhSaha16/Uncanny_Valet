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

        /// <summary>Where the valet stands when the night starts.</summary>
        Vector2 ValetStartPoint { get; }

        IReadOnlyList<int> SpotIds { get; }

        bool IsSpotFree(int spotId);
    }
}
