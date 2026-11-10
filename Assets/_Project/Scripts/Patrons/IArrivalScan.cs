using System.Collections.Generic;

namespace UncannyValet.Patrons
{
    /// <summary>Reports which arrival queue slots have a car physically standing in them (swappable in tests).</summary>
    public interface IArrivalScan
    {
        /// <summary>One flag per slot for the first <paramref name="maxSlots"/> slots. Valid until the next scan.</summary>
        IReadOnlyList<bool> Scan(int maxSlots);
    }
}
