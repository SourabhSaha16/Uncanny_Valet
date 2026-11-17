namespace UncannyValet.Core.Contracts
{
    /// <summary>Read-only view of this shift's blessings. Implemented by the Economy module's ShiftBlessings.</summary>
    public interface IShiftBlessingsView
    {
        /// <summary>Blessings earned minus curses, never below 0.</summary>
        int Blessings { get; }

        /// <summary>The blessing multiplier for the next happy patron, 1 up to the favour cap.</summary>
        int FavourMultiplier { get; }

        /// <summary>Curses so far this shift.</summary>
        int Curses { get; }

        /// <summary>The highest favour multiplier reached so far this shift.</summary>
        int PeakFavour { get; }
    }
}
