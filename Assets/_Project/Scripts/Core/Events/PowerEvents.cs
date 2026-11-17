using UncannyValet.Core.Ids;

namespace UncannyValet.Core.Events
{
    // Valet power events, grouped in one file by domain (the allowed exception to one type per file).

    /// <summary>A car was delivered to a waiting patron at the curb with a tight angle, inside the pickup zone, and early.</summary>
    public readonly struct PerfectDropOff : IGameEvent
    {
        public readonly CarId Car;
        public readonly TicketId Ticket;
        public PerfectDropOff(CarId car, TicketId ticket) { Car = car; Ticket = ticket; }
    }

    /// <summary>
    /// The shared power bar changed. Value is 0 to 100; OpenBuckets is 0 to 3 (the bolts that can be spent); SpentBucket
    /// is the bucket (1 to 3) a power just used, or 0 when the change was a fill or a reset.
    /// </summary>
    public readonly struct PowerBarChanged : IGameEvent
    {
        public readonly float Value;
        public readonly int OpenBuckets;
        public readonly int SpentBucket;
        public PowerBarChanged(float value, int openBuckets, int spentBucket = 0)
        {
            Value = value; OpenBuckets = openBuckets; SpentBucket = spentBucket;
        }
    }

    /// <summary>
    /// The player spent a bucket of the power bar and a power started at the given level (1 earned, 2 upgraded). Modules
    /// react by doing their part of it and read the level's numbers through IPowerView.
    /// </summary>
    public readonly struct PowerActivated : IGameEvent
    {
        public readonly StoryPower Power;
        public readonly int Level;
        public PowerActivated(StoryPower power, int level) { Power = power; Level = level; }
    }

    /// <summary>
    /// Thunder stride was used by the valet: Origin is where the dash or boost starts, Direction is the unit way it goes (the
    /// way he moves or faces on foot, the way the car faces when driving) and InCar says which use it is. The camera kick reacts.
    /// </summary>
    public readonly struct ThunderStrideStarted : IGameEvent
    {
        public readonly UnityEngine.Vector2 Origin;
        public readonly UnityEngine.Vector2 Direction;
        public readonly bool InCar;
        public ThunderStrideStarted(UnityEngine.Vector2 origin, UnityEngine.Vector2 direction, bool inCar)
        {
            Origin = origin; Direction = direction; InCar = inCar;
        }
    }

    /// <summary>A power with a duration ran out (or was ended by a new lot). Instant powers never publish it.</summary>
    public readonly struct PowerEnded : IGameEvent
    {
        public readonly StoryPower Power;
        public PowerEnded(StoryPower power) { Power = power; }
    }

    /// <summary>The HUD flashed a power's icon for its new level at the start of a lot; the run state remembers it so it flashes once.</summary>
    public readonly struct PowerIconFlashed : IGameEvent
    {
        public readonly StoryPower Power;
        public readonly int Level;
        public PowerIconFlashed(StoryPower power, int level) { Power = power; Level = level; }
    }
}
