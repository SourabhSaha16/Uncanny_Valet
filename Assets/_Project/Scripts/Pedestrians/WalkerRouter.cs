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
        private readonly List<Vector2> _route = new List<Vector2>(12);

        public WalkerRouter(IPedestrianPaths paths, ICarFactory cars)
        {
            _paths = paths;
            _cars = cars;
        }

        /// <summary>Applies the walker's current brain phase and remembers it as applied.</summary>
        public void Apply(PedestrianWalker w)
        {
            var phase = w.Brain.Phase;
            w.Applied = phase;
            switch (phase)
            {
                case PedestrianPhase.Arriving: StartRoad(w, false); break;
                case PedestrianPhase.ToDoor: StartToDoor(w); break;
                case PedestrianPhase.ToCurb: StartToCurb(w); break;
                case PedestrianPhase.Leaving: StartLeaving(w); break;
                case PedestrianPhase.Waiting: FaceCurb(w); break;
                default: w.SetVisible(false); break;
            }
        }

        // A patron waiting at the curb always faces the pickup zone; every re-apply of the Waiting phase restores it.
        private void FaceCurb(PedestrianWalker w) => w.FaceToward(WalkAnimMath.CurbLookPoint(w.Position, _paths.CurbCenter));

        private void StartRoad(PedestrianWalker w, bool leaving)
        {
            _paths.RoadRoute(w.RouteIndex, leaving, _route);
            w.SetVisible(true);
            w.Walk(_route, w.StartFraction);
            w.StartFraction = 0f;
        }

        private void StartToDoor(PedestrianWalker w)
        {
            _paths.RouteToDoor(w.Position, _route);
            w.SetVisible(true);
            w.Walk(_route);
        }

        private void StartToCurb(PedestrianWalker w)
        {
            _paths.RouteToCurb(w.Slot, _route);
            w.SetVisible(true);
            w.Walk(_route);
        }

        // Ambient people walk out to the road side; a delivered patron walks to the car and gets in.
        private void StartLeaving(PedestrianWalker w)
        {
            if (!w.Brain.IsPatron) { StartRoad(w, true); return; }
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
