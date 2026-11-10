using UnityEngine;

namespace UncannyValet.Core.Contracts
{
    /// <summary>The pose a trip wants its car to have this step. The car's own module applies it.</summary>
    public readonly struct TripPose
    {
        public readonly Vector2 Position;
        /// <summary>Degrees, 0 = nose up (same convention as a car's transform rotation).</summary>
        public readonly float HeadingDegrees;
        public readonly float Speed;
        public readonly TripPhase Phase;

        public TripPose(Vector2 position, float headingDegrees, float speed, TripPhase phase)
        {
            Position = position;
            HeadingDegrees = headingDegrees;
            Speed = speed;
            Phase = phase;
        }
    }
}
