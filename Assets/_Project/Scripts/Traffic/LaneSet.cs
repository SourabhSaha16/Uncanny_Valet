using System;

namespace UncannyValet.Traffic
{
    /// <summary>A set of lanes (one bit per <see cref="TrafficLane"/>, in declaration order).</summary>
    [Flags]
    public enum LaneSet
    {
        None = 0,
        East = 1,
        Top = 2,
        EastBack = 4,
        TopBack = 8,
    }
}
