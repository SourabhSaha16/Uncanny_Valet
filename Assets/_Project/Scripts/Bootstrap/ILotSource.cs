using UncannyValet.Core.Contracts;
using UncannyValet.Lot;
using UnityEngine;

namespace UncannyValet.Bootstrap
{
    /// <summary>
    /// What the bootstrap needs from a lot, whoever built it (the generated parametric lot today, a hand-made lot later). The bootstrap code
    /// reads the lot only through this seam, so a new kind of lot is a new implementation, not a change to the bootstrap. Read
    /// <see cref="Road"/> and <see cref="Paths"/> after <see cref="Root"/> is active: the lot builds them while it wakes up.
    /// </summary>
    public interface ILotSource
    {
        /// <summary>Spots, queue slots, valet start and bay counts.</summary>
        ILotLayout Layout { get; }

        /// <summary>Bay rectangles; null when the lot offers none.</summary>
        ISpotGeometry Spots { get; }

        /// <summary>The road the traffic drives; available once <see cref="Root"/> is active.</summary>
        IRoadLayout Road { get; }

        /// <summary>The routes pedestrians walk; null when the lot has none.</summary>
        IPedestrianPaths Paths { get; }

        /// <summary>The Perfect Park and drop-off thresholds of this lot.</summary>
        PerfectSettings Perfect { get; }

        /// <summary>The object holding the lot (inactive until the bootstrap activates it); its renderers are what the camera frames.</summary>
        GameObject Root { get; }

        /// <summary>Widens the measured lot rectangle to what must stay in view (the entry lane, a sidewalk strip); returns it unchanged when nothing needs adding.</summary>
        Rect ExtendView(Rect rect);
    }
}
