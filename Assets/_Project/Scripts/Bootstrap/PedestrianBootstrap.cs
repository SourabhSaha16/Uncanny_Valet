using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Lot;
using UncannyValet.Pedestrians;

namespace UncannyValet.Bootstrap
{
    /// <summary>Registers the pedestrian services the decor provides and starts the crowd.</summary>
    public static class PedestrianBootstrap
    {
        /// <summary>Does nothing when the crowd, the decor or its walking paths are missing.</summary>
        public static void Start(PedestrianCrowd crowd, LotDecor decor)
        {
            if (crowd == null || decor == null || decor.Paths == null) return;
            Services.Register<IPedestrianPaths>(decor.Paths);
            Services.Register<IPatronLocator>(crowd);
            crowd.gameObject.SetActive(true);
            crowd.Begin();
        }
    }
}
