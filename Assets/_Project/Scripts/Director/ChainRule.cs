using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Director
{
    /// <summary>The wash slot and every EV bay Blocked: send a Dirty EV, so the player must plan the order, wash first or charge first.</summary>
    public sealed class ChainRule : IDilemmaRule
    {
        private readonly DilemmaRuleSettings _settings;

        public ChainRule(DilemmaRuleSettings settings) { _settings = settings; }

        public DilemmaKind Kind => DilemmaKind.Chain;

        public DilemmaBoost? Evaluate(in LotSnapshot lot)
        {
            var washed = DilemmaSolvable.AllIn(lot, StationKind.Wash, StationSlotState.Blocked);
            var charged = DilemmaSolvable.AllIn(lot, StationKind.Charging, StationSlotState.Blocked);
            if (!washed || !charged) return null;
            return new DilemmaBoost(CarArchetype.DirtyEv, _settings.Chain, DilemmaSolvable.Check(lot, _settings));
        }
    }
}
