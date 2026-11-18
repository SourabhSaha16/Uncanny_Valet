using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Pedestrians;

namespace UncannyValet.Bootstrap
{
    /// <summary>Registers the pedestrian services the lot provides and starts the crowd.</summary>
    public static class PedestrianBootstrap
    {
        /// <summary>Does nothing when the crowd or the lot's walking paths are missing.</summary>
        public static void Start(PedestrianCrowd crowd, IPedestrianPaths paths)
        {
            if (crowd == null || paths == null) return;
            Services.Register<IPedestrianPaths>(paths);
            Services.Register<IPatronLocator>(crowd);
            crowd.gameObject.SetActive(true);
            crowd.Begin();
        }
    }
}
