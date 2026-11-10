namespace UncannyValet.Traffic
{
    /// <summary>One ambient car the spawner wants on the road.</summary>
    public readonly struct SpawnRequest
    {
        public readonly TrafficLane Lane;
        /// <summary>Index into the config's look list (sprite and tint combination).</summary>
        public readonly int LookIndex;
        /// <summary>Multiplier on the lane's desired speed, so cars do not all drive alike.</summary>
        public readonly float SpeedScale;

        public SpawnRequest(TrafficLane lane, int lookIndex, float speedScale)
        {
            Lane = lane;
            LookIndex = lookIndex;
            SpeedScale = speedScale;
        }
    }
}
