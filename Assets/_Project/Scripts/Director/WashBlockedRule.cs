using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Director
{
    /// <summary>Wash slot Blocked (a washed car still in it): send a Muddy car, so the player must move the clean one out.</summary>
    public sealed class WashBlockedRule : IDilemmaRule
    {
        private readonly DilemmaRuleSettings _settings;

        public WashBlockedRule(DilemmaRuleSettings settings) { _settings = settings; }

        public DilemmaKind Kind => DilemmaKind.WashBlocked;

        public DilemmaBoost? Evaluate(in LotSnapshot lot)
        {
            if (lot.CountSlots(StationKind.Wash, StationSlotState.Blocked) == 0) return null;
            return new DilemmaBoost(CarArchetype.Muddy, _settings.WashBlocked, DilemmaSolvable.Check(lot, _settings));
        }
    }
}
