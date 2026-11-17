using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UnityEngine;

namespace UncannyValet.Pedestrians
{
    /// <summary>Turns a person's phase into what they do on screen: hide, appear at the door, or start the matching walk.</summary>
    public sealed class WalkerRouter
    {
        private readonly IPedestrianPaths _paths;
        private readonly ICarFactory _cars;
        private readonly ITuning _tuning;
        private readonly WanderPicker _wander;
        private readonly List<Vector2> _route = new List<Vector2>(12);
        private readonly List<Vector2> _leg = new List<Vector2>(12);

        /// <summary>
        /// The lot's free-route switch is read from <paramref name="tuning"/> at the start of every walk to the door (null: crosswalks only).
        /// <paramref name="wander"/> picks the roaming legs of ambient people (null: they go straight to the door).
        /// </summary>
        public WalkerRouter(IPedestrianPaths paths, ICarFactory cars, ITuning tuning = null, WanderPicker wander = null)
        {
            _paths = paths;
            _cars = cars;
            _tuning = tuning;
            _wander = wander;
        }

        /// <summary>Applies the walker's current brain phase and remembers it as applied.</summary>
        public void Apply(PedestrianWalker w)
        {
            var phase = w.Brain.Phase;
            w.Applied = phase;
            switch (phase)
            {
                case PedestrianPhase.Roaming: StartRoam(w); break;
                case PedestrianPhase.Arriving:
                case PedestrianPhase.ToDoor: StartToDoor(w); break;
                case PedestrianPhase.ToCurb: StartToCurb(w); break;
                case PedestrianPhase.Leaving: StartLeaving(w); break;
                case PedestrianPhase.Waiting: FaceCurb(w); break;
                default: w.SetVisible(false); break;
            }
        }

        // A patron waiting at the curb always faces the pickup zone; every re-apply of the Waiting phase restores it.
        private void FaceCurb(PedestrianWalker w) => w.FaceToward(WalkAnimMath.CurbLookPoint(w.Position, _paths.CurbCenter));

        // One roaming leg along the person's sidewalk/crosswalk route, to a random spot that is not in a keep-out area.
        // With no spot to go to, the person gives up roaming and heads for the door.
        private void StartRoam(PedestrianWalker w)
        {
            _paths.RoadRoute(w.RouteIndex, false, _route);
            if (_wander == null || !_wander.TryPick(_route, w.RoamFraction, out var to))
            {
                w.Brain.GoToDoor();
                Apply(w);
                return;
            }
            RoamPath.Slice(_route, w.RoamFraction, to, _leg);
            w.RoamFraction = to;
            w.SetVisible(true);
            w.Walk(_leg, w.StartFraction);
            w.StartFraction = 0f;
        }

        private void StartToDoor(PedestrianWalker w)
        {
            _paths.RouteToDoor(w.Position, _route);
            WalkerRouteRule.Choose(WalkerRouteRule.FreeRoutes(_tuning), _route);
            w.SetVisible(true);
            w.Walk(_route);
        }

        private void StartToCurb(PedestrianWalker w)
        {
            _paths.RouteToCurb(w.Slot, _route);
            w.SetVisible(true);
            w.Walk(_route);
        }

        // A delivered patron walks to the car and gets in.
        private void StartLeaving(PedestrianWalker w)
        {
            if (!_cars.TryGet(w.Car, out var car) || car.Transform == null)
            {
                w.Brain.Dismiss(false);
                w.Applied = w.Brain.Phase;
                w.SetVisible(false);
                return;
            }
            _route.Clear();
            _route.Add(w.Position);
            _route.Add(car.Transform.position);
            w.Walk(_route);
        }
    }
}
