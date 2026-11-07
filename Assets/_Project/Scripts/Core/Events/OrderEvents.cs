using UncannyValet.Core.Ids;

namespace UncannyValet.Core.Events
{
    // Boss order and strike events, grouped in one file by domain.

    /// <summary>The boss sent a message: put this car in this spot (published by Orders).</summary>
    public readonly struct OrderIssued : IGameEvent
    {
        public readonly OrderId Order;
        public readonly CarId Car;
        /// <summary>The car's display letter, so the UI can say "car B".</summary>
        public readonly char Letter;
        public readonly int TargetSpot;
        public OrderIssued(OrderId order, CarId car, char letter, int targetSpot)
        {
            Order = order; Car = car; Letter = letter; TargetSpot = targetSpot;
        }
    }

    /// <summary>
    /// An order was judged, which happens when its car is handed back (published by Orders). On Failed,
    /// a StrikeGiven follows.
    /// </summary>
    public readonly struct OrderResolved : IGameEvent
    {
        public readonly OrderId Order;
        public readonly CarId Car;
        public readonly OrderOutcome Outcome;
        public OrderResolved(OrderId order, CarId car, OrderOutcome outcome)
        {
            Order = order; Car = car; Outcome = outcome;
        }
    }

    /// <summary>
    /// The run's strike count went up (published by Orders). Strikes last for the whole run. When
    /// <see cref="IsFinal"/> is true the run is lost to the boss.
    /// </summary>
    public readonly struct StrikeGiven : IGameEvent
    {
        public readonly int Total;
        public readonly int Max;
        public bool IsFinal => Total >= Max;
        public StrikeGiven(int total, int max) { Total = total; Max = max; }
    }
}
