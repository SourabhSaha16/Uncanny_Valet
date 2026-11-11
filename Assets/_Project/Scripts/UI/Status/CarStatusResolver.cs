using System.Collections.Generic;
using UncannyValet.UI.Mood;

namespace UncannyValet.UI.Status
{
    /// <summary>Picks the highest-priority status for a ticket: the first rule in the ordered list that applies, else the fallback.</summary>
    public sealed class CarStatusResolver
    {
        private readonly ICarStatusRule[] _rules;
        private readonly CarStatus _fallback;

        /// <summary>Builds a resolver; <paramref name="rulesInPriorityOrder"/> lists the highest priority first.</summary>
        public CarStatusResolver(IReadOnlyList<ICarStatusRule> rulesInPriorityOrder, CarStatus fallback)
        {
            _rules = new ICarStatusRule[rulesInPriorityOrder.Count];
            for (var i = 0; i < _rules.Length; i++) _rules[i] = rulesInPriorityOrder[i];
            _fallback = fallback;
        }

        /// <summary>The game's rules: Won't tip, Cust. Angry, Pick Up, Washing, Charging, Washed, Charged, then Parked.</summary>
        public static CarStatusResolver CreateDefault(EmotionRule emotions) => new CarStatusResolver(new ICarStatusRule[]
        {
            new WillNotTipStatusRule(emotions),
            new CustAngryStatusRule(emotions),
            new PickUpStatusRule(),
            new WashingStatusRule(),
            new ChargingStatusRule(),
            new WashedStatusRule(),
            new ChargedStatusRule()
        }, CarStatus.Parked);

        /// <summary>The status of the ticket. Allocates nothing.</summary>
        public CarStatus Resolve(TicketRow row)
        {
            for (var i = 0; i < _rules.Length; i++)
                if (_rules[i].Applies(row)) return _rules[i].Status;
            return _fallback;
        }
    }
}
