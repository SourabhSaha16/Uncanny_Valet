using UncannyValet.Core.Ids;

namespace UncannyValet.Director
{
    /// <summary>A parked car whose patron is still inside the diner, with its return time and window (shift seconds).</summary>
    public struct ReturnCandidate
    {
        public CarId Car;
        /// <summary>The return time the scheduler is working with; starts as <see cref="Start"/>.</summary>
        public float At;
        /// <summary>The return time in the snapshot.</summary>
        public float Start;
        /// <summary>Earliest return inside the window.</summary>
        public float Lo;
        /// <summary>Latest return inside the window.</summary>
        public float Hi;
    }
}
