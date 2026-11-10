using System.Collections.Generic;
using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UnityEngine;

namespace UncannyValet.Pedestrians
{
    /// <summary>
    /// Owns the pooled people. Ambient people walk in from the road side, cross at the crosswalks, vanish into the
    /// diner, come out again and walk away; patrons (see <see cref="PatronWalkers"/>) follow their tickets. Nobody has
    /// a collider. Created once by <see cref="Begin"/>; nothing is allocated per frame.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PedestrianCrowd : MonoBehaviour, IPatronLocator
    {
        [SerializeField] private PedestrianConfig _config;

        private readonly List<PedestrianWalker> _ambient = new List<PedestrianWalker>(10);
        private WalkerRouter _router;
        private IPedestrianPaths _paths;
        private ITuning _tuning;
        private PatronWalkers _patrons;
        private int _serial;

        /// <summary>How many ambient pedestrians exist.</summary>
        public int Count => _ambient.Count;

        /// <summary>How many ambient pedestrians start active; seeds the F2 slider.</summary>
        public int StartCount => _config != null ? _config.StartActive : 0;

        /// <summary>Creates the people. Needs IEventBus, ICarFactory and IPedestrianPaths registered. Does nothing if already begun.</summary>
        public void Begin()
        {
            if (_patrons != null) return;
            if (_config == null || !_config.IsComplete)
            {
                Debug.LogError("PedestrianCrowd on " + name + " has no complete PedestrianConfig; rebuild with Setup/Build Everything.", this);
                return;
            }
            _tuning = Services.Get<ITuning>();
            _paths = Services.Get<IPedestrianPaths>();
            _router = new WalkerRouter(_paths, Services.Get<ICarFactory>());
            for (var i = 0; i < _config.Count; i++) _ambient.Add(CreateAmbient(i));
            var pool = new List<PedestrianWalker>(_config.PatronPoolSize);
            for (var i = 0; i < _config.PatronPoolSize; i++) pool.Add(Create("Patron " + i, _config.Count + i));
            _patrons = new PatronWalkers(Services.Get<IEventBus>(), _paths, Services.Get<ICarFactory>(), _router, _config, pool);
        }

        public bool TryGetWaitingPatron(CarId car, out Vector2 position)
        {
            position = Vector2.zero;
            return _patrons != null && _patrons.TryGetWaitingPatron(car, out position);
        }

        private void OnDisable()
        {
            _patrons?.Dispose();
        }

        private void Update()
        {
            if (_patrons == null) return;
            var delta = Time.deltaTime;
            var active = ActiveAmbient();
            for (var i = 0; i < _ambient.Count; i++) StepAmbient(_ambient[i], delta, i < active);
            _patrons.Sync();
        }

        // The F2 slider: people above the count finish the walk they are on, then stay out of sight.
        private int ActiveAmbient() => ActiveAmbient(_ambient.Count);

        private int ActiveAmbient(int poolSize)
            => _tuning != null && _tuning.TryGet(TuningKey.PedestrianCount, out var value)
                ? Mathf.Clamp(Mathf.RoundToInt(value), 0, poolSize)
                : poolSize;

        private void StepAmbient(PedestrianWalker walker, float delta, bool allowed)
        {
            walker.Brain.Tick(delta);
            if (walker.Brain.SpawnDue)
            {
                if (allowed) BeginAmbientWalk(walker, 0f);
            }
            else if (walker.Brain.Phase != walker.Applied) _router.Apply(walker);
        }

        private void BeginAmbientWalk(PedestrianWalker walker, float startFraction)
        {
            walker.RouteIndex = NextRoute();
            walker.StartFraction = startFraction;
            walker.Brain.BeginAmbient(Between(_config.InsideSeconds));
            _router.Apply(walker);
        }

        private void OnRouteFinished(PedestrianWalker walker)
        {
            if (walker.Brain.IsPatron) { _patrons?.OnRouteFinished(walker); return; }
            walker.Brain.ReachedRouteEnd();
            if (walker.Brain.Phase == PedestrianPhase.Leaving) walker.RouteIndex = NextRoute();
            if (walker.Brain.Phase == PedestrianPhase.Idle) walker.Brain.WaitToSpawn(Between(_config.RespawnSeconds));
            _router.Apply(walker);
        }

        private PedestrianWalker CreateAmbient(int index)
        {
            var walker = Create("Pedestrian " + index, index);
            if (index < ActiveAmbient(_config.Count))
                BeginAmbientWalk(walker, PedestrianPlan.Hash01(index + _config.Count) * _config.OpeningProgressMax);
            else walker.Brain.WaitToSpawn(0f);
            return walker;
        }

        private PedestrianWalker Create(string walkerName, int index)
        {
            var go = new GameObject(walkerName);
            go.transform.SetParent(transform, false);
            var walker = go.AddComponent<PedestrianWalker>();
            walker.Create(_config);
            walker.Dress(PedestrianPlan.Look(index, _config.Skins, _config.Hairs, _config.Outfits),
                PedestrianPlan.Speed(index, _config.MinSpeed, _config.MaxSpeed));
            walker.RouteFinished += OnRouteFinished;
            walker.SetVisible(false);
            return walker;
        }

        private int NextRoute() => _paths.RoadRouteCount <= 0 ? 0 : (int)(PedestrianPlan.Hash01(++_serial) * _paths.RoadRouteCount) % _paths.RoadRouteCount;

        private float Between(Vector2 range) => Mathf.Lerp(range.x, range.y, PedestrianPlan.Hash01(++_serial));
    }
}
