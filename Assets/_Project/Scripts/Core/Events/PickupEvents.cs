using UncannyValet.Core.Ids;

namespace UncannyValet.Core.Events
{
    // Valet pickup spot and queue events (issues 126 and 127), grouped in one file by domain.

    /// <summary>
    /// The patron of this car may step out now: the valet came up to the car in the pickup spot (or took the car some
    /// other way). Published by Patrons; Pedestrians let the patron walk to the diner.
    /// </summary>
    public readonly struct PatronStepsOut : IGameEvent
    {
        public readonly CarId Car;
        public PatronStepsOut(CarId car) { Car = car; }
    }

    /// <summary>The patron of this car walked into the diner. Published by Pedestrians; Patrons starts the stay clock from it.</summary>
    public readonly struct PatronEnteredDiner : IGameEvent
    {
        public readonly CarId Car;
        public PatronEnteredDiner(CarId car) { Car = car; }
    }

    /// <summary>A queued car finished driving up to its new queue position (published by Cars; the car has stopped).</summary>
    public readonly struct CarRepositioned : IGameEvent
    {
        public readonly CarId Car;
        public CarRepositioned(CarId car) { Car = car; }
    }

    /// <summary>
    /// The valet drove this car out of the pickup spot (slot 0 of the queue), or took it from somewhere that is not the pickup
    /// spot. Published by Patrons; the stay clock starts at the later of this and <see cref="PatronEnteredDiner"/> (issue 139).
    /// </summary>
    public readonly struct CarLeftPickup : IGameEvent
    {
        public readonly CarId Car;
        public CarLeftPickup(CarId car) { Car = car; }
    }
}
