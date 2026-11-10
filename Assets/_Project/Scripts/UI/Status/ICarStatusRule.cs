namespace UncannyValet.UI.Status
{
    /// <summary>One condition that gives a car a status. The resolver tries the rules in priority order.</summary>
    public interface ICarStatusRule
    {
        /// <summary>The status this rule hands out.</summary>
        CarStatus Status { get; }

        /// <summary>True when the ticket's state meets the condition.</summary>
        bool Applies(TicketRow row);
    }
}
