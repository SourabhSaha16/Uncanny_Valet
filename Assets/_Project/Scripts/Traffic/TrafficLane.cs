namespace UncannyValet.Traffic
{
    /// <summary>The four lanes: a pair on each road, one driving left to right and one right to left.</summary>
    public enum TrafficLane
    {
        /// <summary>The entry road's lot-side lane, left to right. Carries arriving patron cars and ambient cars.</summary>
        East,
        /// <summary>The exit road's lot-side lane, left to right. Carries delivered cars leaving and ambient cars.</summary>
        Top,
        /// <summary>The entry road's opposite lane, right to left. Ambient cars only.</summary>
        EastBack,
        /// <summary>The exit road's opposite lane, right to left. Ambient cars only.</summary>
        TopBack,
    }
}
