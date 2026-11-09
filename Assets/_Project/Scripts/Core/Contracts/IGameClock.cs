namespace UncannyValet.Core.Contracts
{
    /// <summary>The shift clock. Implemented by the Night module.</summary>
    public interface IGameClock
    {
        /// <summary>Real seconds left in the shift.</summary>
        float SecondsRemaining { get; }

        /// <summary>Real seconds a whole shift lasts.</summary>
        float ShiftLengthSeconds { get; }

        /// <summary>Game minutes since the shift started. Patron timers count in these.</summary>
        float ElapsedMinutes { get; }

        bool IsRunning { get; }
    }
}
