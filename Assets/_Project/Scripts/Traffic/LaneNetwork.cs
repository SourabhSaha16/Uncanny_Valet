using UncannyValet.Core.Contracts;

namespace UncannyValet.Traffic
{
    /// <summary>The four lane paths, indexed by <see cref="TrafficLane"/>, built from the road layout.</summary>
    public sealed class LaneNetwork
    {
        private readonly LanePath[] _paths = new LanePath[TrafficLanes.Count];

        public LaneNetwork()
        {
            for (int i = 0; i < _paths.Length; i++) _paths[i] = new LanePath();
        }

        /// <summary>The path of <paramref name="lane"/>.</summary>
        public LanePath Path(TrafficLane lane) => _paths[(int)lane];

        /// <summary>Rebuilds every path from <paramref name="road"/>.</summary>
        public void Rebuild(IRoadLayout road)
        {
            Path(TrafficLane.East).Rebuild(road.EastLane);
            Path(TrafficLane.Top).Rebuild(road.TopLane);
            Path(TrafficLane.EastBack).Rebuild(road.EastBackLane);
            Path(TrafficLane.TopBack).Rebuild(road.TopBackLane);
        }
    }
}
