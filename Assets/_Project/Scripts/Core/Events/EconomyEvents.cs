using UncannyValet.Core.Ids;

namespace UncannyValet.Core.Events
{
    // Economy events, grouped in one file by domain.

    /// <summary>A tip was earned (already multiplied by the combo). Amount is never negative; 0 means a missed tip.</summary>
    public readonly struct TipEarned : IGameEvent
    {
        public readonly TicketId Ticket;
        public readonly int Amount;
        public TipEarned(TicketId ticket, int amount) { Ticket = ticket; Amount = amount; }
    }

    /// <summary>The shift score, combo multiplier, complaint count or stars changed.</summary>
    public readonly struct ScoreChanged : IGameEvent
    {
        public readonly int Score;
        public readonly int ComboMultiplier;
        public readonly int Complaints;
        public readonly int Stars;
        public ScoreChanged(int score, int comboMultiplier, int complaints, int stars)
        {
            Score = score; ComboMultiplier = comboMultiplier; Complaints = complaints; Stars = stars;
        }
    }

    /// <summary>An angry patron cost the shift a complaint penalty and reset the combo.</summary>
    public readonly struct ComplaintFiled : IGameEvent
    {
        public readonly TicketId Ticket;
        public readonly int Penalty;
        public ComplaintFiled(TicketId ticket, int penalty) { Ticket = ticket; Penalty = penalty; }
    }
}
