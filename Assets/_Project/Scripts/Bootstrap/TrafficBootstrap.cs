using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Traffic;
using UnityEngine;

namespace UncannyValet.Bootstrap
{
    /// <summary>Starts the road traffic once the lot has registered the road.</summary>
    public static class TrafficBootstrap
    {
        /// <summary>
        /// The module reads IRoadLayout in its Start and registers the trip routers. Returns the link between the F2 panel and the traffic
        /// (dispose it with the scene), or null when the scene has no traffic module (an error is logged).
        /// </summary>
        public static TrafficTuningLink Start(IEventBus bus, TrafficModule traffic, Object context)
        {
            if (traffic == null)
            {
                Debug.LogError("GameBootstrap has no TrafficModule: rebuild with Uncanny Valet/Setup/Build Everything. Cars drive in and out without a road.", context);
                return null;
            }
            var link = new TrafficTuningLink(bus, traffic.SetMinOnScreen, traffic.SetSpeedScale,
                traffic.SetEntrySpeed, traffic.SetExitSpeed, traffic.SetTargetOnScreen);
            PassSeededDensity(traffic);
            traffic.gameObject.SetActive(true);
            return link;
        }

        // The lot may seed its own density (a lot without ambient cars seeds 0); the module starts from its config, so hand it the seeded values.
        private static void PassSeededDensity(TrafficModule traffic)
        {
            if (!Services.TryGet<ITuning>(out var tuning)) return;
            if (tuning.TryGet(TuningKey.TrafficMinOnScreen, out var min)) traffic.SetMinOnScreen(min);
            if (tuning.TryGet(TuningKey.TrafficTargetOnScreen, out var target)) traffic.SetTargetOnScreen(target);
        }
    }
}
