using System.Collections.Generic;
using UnityEngine;

namespace UncannyValet.Pedestrians
{
    /// <summary>
    /// Creates the pedestrians once, from a <see cref="PedestrianConfig"/> and the routes the lot provides, and keeps
    /// them for the whole session (a pool of walkers that never despawn). Variety is deterministic.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PedestrianCrowd : MonoBehaviour
    {
        [SerializeField] private PedestrianConfig _config;

        private readonly List<PedestrianWalker> _walkers = new List<PedestrianWalker>(10);

        /// <summary>How many pedestrians exist.</summary>
        public int Count => _walkers.Count;

        /// <summary>Creates the walkers on the given world-space routes. Does nothing if they already exist.</summary>
        public void Begin(Vector2[][] routes)
        {
            if (_walkers.Count > 0) return;
            if (_config == null || !_config.IsComplete)
            {
                Debug.LogError("PedestrianCrowd on " + name + " has no complete PedestrianConfig; rebuild with Setup/Build Everything.", this);
                return;
            }
            if (routes == null || routes.Length == 0) return;
            for (var i = 0; i < _config.Count; i++) _walkers.Add(Create(i, routes));
        }

        private PedestrianWalker Create(int index, Vector2[][] routes)
        {
            var routeIndex = PedestrianPlan.RouteFor(index, routes.Length);
            var route = routes[routeIndex];
            var go = new GameObject("Pedestrian " + index);
            go.transform.SetParent(transform, false);
            var walker = go.AddComponent<PedestrianWalker>();
            var look = PedestrianPlan.Look(index, _config.Skins, _config.Hairs, _config.Outfits);
            var speed = PedestrianPlan.Speed(index, _config.MinSpeed, _config.MaxSpeed);
            var start = PedestrianPlan.StartFraction(index, routes.Length, _config.Count) * new RouteWalker(route).CycleLength;
            walker.Setup(_config, look, route, speed, start);
            return walker;
        }
    }
}
