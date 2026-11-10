using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Director
{
    /// <summary>Every EV bay Blocked (fully charged EVs): send an EV, so the player must evict a charged one to a regular spot.</summary>
    public sealed class BaysBlockedRule : IDilemmaRule
    {
        private readonly DilemmaRuleSettings _settings;

        public BaysBlockedRule(DilemmaRuleSettings settings) { _settings = settings; }

        public DilemmaKind Kind => DilemmaKind.BaysBlocked;

        public DilemmaBoost? Evaluate(in LotSnapshot lot)
        {
            if (!DilemmaSolvable.AllIn(lot, StationKind.Charging, StationSlotState.Blocked)) return null;
            return new DilemmaBoost(CarArchetype.Ev, _settings.BaysBlocked, DilemmaSolvable.Check(lot, _settings));
        }
    }
}
