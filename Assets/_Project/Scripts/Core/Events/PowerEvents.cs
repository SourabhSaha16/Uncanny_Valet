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

    /// <summary>The power meter changed. Level is 0 to 3 (0 = no power unlocked); Fill01 is the bar fill from 0 to 1.</summary>
    public readonly struct PowerMeterChanged : IGameEvent
    {
        public readonly int Level;
        public readonly float Fill01;
        public PowerMeterChanged(int level, float fill01) { Level = level; Fill01 = fill01; }
    }

    /// <summary>The player spent the meter and a power started. Modules react by doing their part of it.</summary>
    public readonly struct PowerActivated : IGameEvent
    {
        public readonly PowerKind Power;
        public PowerActivated(PowerKind power) { Power = power; }
    }

    /// <summary>A timed power ran out. Park Smash is instant and never ends this way.</summary>
    public readonly struct PowerEnded : IGameEvent
    {
        public readonly PowerKind Power;
        public PowerEnded(PowerKind power) { Power = power; }
    }
}
