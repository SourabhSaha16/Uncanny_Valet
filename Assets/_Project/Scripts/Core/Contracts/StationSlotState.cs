namespace UncannyValet.Core.Contracts
{
    /// <summary>What a station slot (the wash slot or an EV bay) holds right now.</summary>
    public enum StationSlotState
    {
        /// <summary>Empty.</summary>
        Free,

        /// <summary>Holds a car that needs it and is mid-service.</summary>
        Working,

        /// <summary>Holds a car whose service is done (washed car, fully charged EV).</summary>
        Blocked,

        /// <summary>Holds a car that does not need it (a plain car in an EV bay).</summary>
        Squatted,

        /// <summary>Empty, but a car that needs it is being driven in the lot.</summary>
        Reserved
    }
}
