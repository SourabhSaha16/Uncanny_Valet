using UncannyValet.Core.Ids;

namespace UncannyValet.Core.Events
{
    // Economy events, grouped in one file by domain.

    /// <summary>A blessing was earned (already multiplied by the favour). Amount is never negative; 0 means a missed blessing.</summary>
    public readonly struct BlessingEarned : IGameEvent
    {
        public readonly TicketId Ticket;
        public readonly int Amount;
        public BlessingEarned(TicketId ticket, int amount) { Ticket = ticket; Amount = amount; }
    }

    /// <summary>The shift's blessings, favour multiplier or curse count changed.</summary>
    public readonly struct BlessingsChanged : IGameEvent
    {
        public readonly int Blessings;
        public readonly int FavourMultiplier;
        public readonly int Curses;
        public BlessingsChanged(int blessings, int favourMultiplier, int curses)
        {
            Blessings = blessings; FavourMultiplier = favourMultiplier; Curses = curses;
        }
    }

    /// <summary>A curse cost the shift blessings and reset the favour. An angry patron is the first source.</summary>
    public readonly struct CurseApplied : IGameEvent
    {
        /// <summary>The ticket that caused it, or TicketId.None for a source without a ticket.</summary>
        public readonly TicketId Ticket;
        /// <summary>Blessings the curse asked to take off (the total never goes below 0).</summary>
        public readonly int Amount;
        public CurseApplied(TicketId ticket, int amount) { Ticket = ticket; Amount = amount; }
    }
}
