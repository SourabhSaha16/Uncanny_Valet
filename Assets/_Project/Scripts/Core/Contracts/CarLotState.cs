namespace UncannyValet.Core.Contracts
{
    /// <summary>Where an open ticket's car is in its journey; every car is in exactly one.</summary>
    public enum CarLotState
    {
        /// <summary>Driving in to the arrival slot.</summary>
        Arriving,

        /// <summary>Stopped at the arrival slot, park timer running.</summary>
        WaitingToPark,

        /// <summary>The valet is in it.</summary>
        Driven,

        /// <summary>In a regular spot (or left somewhere else), stay not over.</summary>
        Parked,

        /// <summary>In the wash slot or an EV bay.</summary>
        InStation,

        /// <summary>The patron is at the curb, pickup timer running.</summary>
        PickupRequested
    }
}
