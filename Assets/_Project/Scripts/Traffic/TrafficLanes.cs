namespace UncannyValet.Traffic
{
    /// <summary>Facts about the four lanes: how many there are, which way they run, and conversion to <see cref="LaneSet"/>.</summary>
    public static class TrafficLanes
    {
        /// <summary>Number of lanes.</summary>
        public const int Count = 4;

        /// <summary>True for the lanes that drive right to left.</summary>
        public static bool IsLeftward(TrafficLane lane) => lane == TrafficLane.EastBack || lane == TrafficLane.TopBack;

        /// <summary>The set holding only <paramref name="lane"/>.</summary>
        public static LaneSet ToSet(TrafficLane lane) => (LaneSet)(1 << (int)lane);

        /// <summary>True when <paramref name="set"/> contains <paramref name="lane"/>.</summary>
        public static bool Has(LaneSet set, TrafficLane lane) => (set & ToSet(lane)) != 0;
    }
}
