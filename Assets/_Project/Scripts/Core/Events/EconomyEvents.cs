using UncannyValet.Core.Ids;

namespace UncannyValet.Core.Events
{
    // Economy events, grouped in one file by domain.

    /// <summary>A tip was earned. Amount is never negative.</summary>
    public readonly struct TipEarned : IGameEvent
    {
        public readonly TicketId Ticket;
        public readonly int Amount;
        public TipEarned(TicketId ticket, int amount) { Ticket = ticket; Amount = amount; }
    }

    /// <summary>The debt or tonight's tips changed.</summary>
    public readonly struct DebtChanged : IGameEvent
    {
        public readonly int DebtRemaining;
        public readonly int TipsTonight;
        public DebtChanged(int debtRemaining, int tipsTonight)
        {
            DebtRemaining = debtRemaining; TipsTonight = tipsTonight;
        }
    }
}
