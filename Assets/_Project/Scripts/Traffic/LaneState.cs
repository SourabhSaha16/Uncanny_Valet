namespace UncannyValet.Traffic
{
    /// <summary>A car's place on a lane: its occupancy id, lane, distance along the lane and speed.</summary>
    public struct LaneState
    {
        public int Id;
        public TrafficLane Lane;
        public float Distance;
        public float Speed;
    }
}
