using System.Collections.Generic;
using UncannyValet.Core.Contracts;

namespace UncannyValet.Lot
{
    /// <summary>Lookup of each built bay's inner painted rectangle by spot id (the Lot's ISpotGeometry).</summary>
    public sealed class SpotGeometry : ISpotGeometry
    {
        private readonly Dictionary<int, SpotBounds> _bays = new Dictionary<int, SpotBounds>();

        /// <summary>Records (or replaces) the rectangle of a bay.</summary>
        public void Set(int spotId, SpotBounds bounds) => _bays[spotId] = bounds;

        /// <inheritdoc />
        public bool TryGetSpot(int spotId, out SpotBounds bay) => _bays.TryGetValue(spotId, out bay);
    }
}
