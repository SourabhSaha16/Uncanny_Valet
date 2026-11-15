using System.Collections.Generic;
using UncannyValet.Core.Ids;

namespace UncannyValet.Patrons
{
    /// <summary>Reports which arrival queue slots have a car physically standing in them (swappable in tests).</summary>
    public interface IArrivalScan
    {
        /// <summary>One flag per slot for the first <paramref name="maxSlots"/> slots. Valid until the next scan.</summary>
        IReadOnlyList<bool> Scan(int maxSlots);

        /// <summary>True when a car other than <paramref name="except"/> physically stands in <paramref name="slot"/>.</summary>
        bool IsTaken(int slot, CarId except);
    }
}
