namespace UncannyValet.Traffic
{
    /// <summary>One ambient car: its lane state, its wanted speed and its visual. Pooled by the module, never reallocated.</summary>
    public sealed class AmbientVehicle
    {
        public bool Active;
        public LaneState State;
        public float DesiredSpeed;
        public TrafficCar View;
    }
}
