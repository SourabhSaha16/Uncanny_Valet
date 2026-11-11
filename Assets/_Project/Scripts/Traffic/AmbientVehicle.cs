using UnityEngine;

namespace UncannyValet.Traffic
{
    /// <summary>One ambient car: its lane state, its wanted speed, its last position and its visual. Pooled by the traffic, never reallocated.</summary>
    public sealed class AmbientVehicle
    {
        public bool Active;
        public LaneState State;
        public float DesiredSpeed;
        public Vector2 Position;
        public TrafficCar View;
    }
}
