namespace UncannyValet.Core.Contracts
{
    /// <summary>Night time. Implemented by the Night module.</summary>
    public interface IGameClock
    {
        int Night { get; }

        /// <summary>Game minutes since the shift started.</summary>
        float MinuteOfNight { get; }

        bool IsRunning { get; }
    }
}
