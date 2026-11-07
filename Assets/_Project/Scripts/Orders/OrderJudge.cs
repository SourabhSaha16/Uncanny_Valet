using System;
using UncannyValet.Core.Events;

namespace UncannyValet.Orders
{
    /// <summary>Pure pass/fail: an order passes if the car visited the target spot after the order was issued.</summary>
    public sealed class OrderJudge
    {
        private readonly CarVisitHistory _history;

        public OrderJudge(CarVisitHistory history)
        {
            _history = history ?? throw new ArgumentNullException(nameof(history));
        }

        /// <summary>Passed or Failed; Cancelled is decided by the caller, never here.</summary>
        public OrderOutcome Judge(in Order order)
        {
            return _history.HasVisited(order.Car, order.TargetSpot, order.IssuedAtMinute)
                ? OrderOutcome.Passed
                : OrderOutcome.Failed;
        }
    }
}
