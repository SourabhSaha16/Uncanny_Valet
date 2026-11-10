using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Director
{
    /// <summary>A non-EV squats in an EV bay: send an EV, so using the bay as overflow costs the player.</summary>
    public sealed class BaySquattedRule : IDilemmaRule
    {
        private readonly DilemmaRuleSettings _settings;

        public BaySquattedRule(DilemmaRuleSettings settings) { _settings = settings; }

        public DilemmaKind Kind => DilemmaKind.BaySquatted;

        public DilemmaBoost? Evaluate(in LotSnapshot lot)
        {
            if (lot.CountSlots(StationKind.Charging, StationSlotState.Squatted) == 0) return null;
            return new DilemmaBoost(CarArchetype.Ev, _settings.BaySquatted, DilemmaSolvable.Check(lot, _settings));
        }
    }
}
