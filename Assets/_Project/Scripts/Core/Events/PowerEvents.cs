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

    /// <summary>The player spent a bucket of the power bar and a power started. Modules react by doing their part of it.</summary>
    public readonly struct PowerActivated : IGameEvent
    {
        public readonly PowerKind Power;
        public PowerActivated(PowerKind power) { Power = power; }
    }

    /// <summary>A timed power ran out.</summary>
    public readonly struct PowerEnded : IGameEvent
    {
        public readonly PowerKind Power;
        public PowerEnded(PowerKind power) { Power = power; }
    }
}
