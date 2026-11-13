namespace UncannyValet.Core.Events
{
    /// <summary>The final numbers of one shift, shown on the results screen.</summary>
    public readonly struct ShiftResult
    {
        /// <summary>Blessings at shift end.</summary>
        public readonly int Blessings;
        /// <summary>Blessings the lot needed (0 when no lot goal was registered).</summary>
        public readonly int Threshold;
        /// <summary>True when Blessings reached the Threshold.</summary>
        public readonly bool Cleared;
        public readonly int Curses;
        /// <summary>Highest favour multiplier reached.</summary>
        public readonly int PeakFavour;

        public ShiftResult(int blessings, int threshold, bool cleared, int curses, int peakFavour)
        {
            Blessings = blessings; Threshold = threshold; Cleared = cleared; Curses = curses; PeakFavour = peakFavour;
        }
    }
}
