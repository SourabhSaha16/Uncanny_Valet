using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UnityEngine;

namespace UncannyValet.Pedestrians
{
    /// <summary>
    /// Pure route choice for walks to or from the diner. On lots where free routes are on (the crazy lots) a person walks the
    /// straight line from where they are to where they are going, so nobody queues behind others at the crosswalk; elsewhere they keep to the crosswalk route.
    /// </summary>
    public static class WalkerRouteRule
    {
        /// <summary>True when the lot's switch (<see cref="TuningKey.PedestrianFreeRoutes"/>) is on. A missing tuning service means off.</summary>
        public static bool FreeRoutes(ITuning tuning)
            => tuning != null && tuning.TryGet(TuningKey.PedestrianFreeRoutes, out var value) && value >= 0.5f;

        /// <summary>
        /// Applies the choice to a crosswalk route in place: with free routes on, keeps only the first and last point (a direct walk);
        /// otherwise leaves the route alone. Routes of fewer than three points are already direct.
        /// </summary>
        public static void Choose(bool freeRoutes, List<Vector2> route)
        {
            if (!freeRoutes || route.Count < 3) return;
            var last = route[route.Count - 1];
            route.RemoveRange(1, route.Count - 1);
            route.Add(last);
        }
    }
}
