using UncannyValet.Core.Ids;
using UnityEngine;

namespace UncannyValet.Core.Events
{
    /// <summary>
    /// A car ran into a pedestrian and knocked them away (published by Pedestrians once per knockback). The car is
    /// the one that hit; <see cref="ByPlayerCar"/> is true when the valet is driving it; <see cref="IsPatron"/> is true for a
    /// patron walker (not the ambient crowd). Speed is the hit speed in world units per second.
    /// </summary>
    public readonly struct PedestrianHit : IGameEvent
    {
        public readonly PedestrianId Pedestrian;
        public readonly bool IsPatron;
        public readonly Vector2 Point;
        public readonly float Speed;
        public readonly bool ByPlayerCar;
        public readonly CarId Car;

        public PedestrianHit(PedestrianId pedestrian, bool isPatron, Vector2 point, float speed, bool byPlayerCar, CarId car)
        { Pedestrian = pedestrian; IsPatron = isPatron; Point = point; Speed = speed; ByPlayerCar = byPlayerCar; Car = car; }
    }
}
