using UncannyValet.Core.Contracts;
using System.Collections.Generic;
using UnityEngine;

namespace UncannyValet.Traffic
{
    /// <summary>Drives a car along a lot route: cruises, and can brake smoothly to a stop at the route's end.</summary>
    public sealed class RouteRunner
    {
        private readonly LanePath _path = new LanePath();
        private float _distance;

        /// <summary>Current speed. Set it to carry speed over from the lane.</summary>
        public float Speed { get; set; }

        /// <summary>Distance left to the end of the route.</summary>
        public float Remaining => _path.Length - _distance;

        /// <summary>Starts a new route from its first point at rest.</summary>
        public void Begin(IReadOnlyList<Vector2> points)
        {
            _path.Rebuild(points);
            _distance = 0f;
            Speed = 0f;
        }

        /// <summary>Moves along the route. With <paramref name="stopAtEnd"/> the speed is capped so the car comes to rest exactly at the end.</summary>
        public void Step(float cruiseSpeed, bool stopAtEnd, float acceleration, float brake, float dt)
        {
            float target = stopAtEnd ? Mathf.Min(cruiseSpeed, ApproachSpeed.Cap(Remaining, 0f, brake)) : cruiseSpeed;
            Speed = CarFollowing.Approach(Speed, target, acceleration, brake, dt);
            _distance = Mathf.Min(_path.Length, _distance + Speed * dt);
        }

        /// <summary>True when within <paramref name="tolerance"/> of the end.</summary>
        public bool AtEnd(float tolerance) => Remaining <= tolerance;

        /// <summary>The pose on the route for a phase.</summary>
        public TripPose Pose(TripPhase phase, float speed) =>
            new TripPose(_path.PositionAt(_distance), _path.HeadingAt(_distance), speed, phase);
    }
}
