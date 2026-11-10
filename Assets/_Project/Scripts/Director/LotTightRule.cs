using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Director
{
    /// <summary>Few free regular spots: favour a Plain car and ask for a parked patron's return to come forward, so delivering first frees a spot.</summary>
    public sealed class LotTightRule : IDilemmaRule
    {
        private readonly DilemmaRuleSettings _settings;

        public LotTightRule(DilemmaRuleSettings settings) { _settings = settings; }

        public DilemmaKind Kind => DilemmaKind.LotTight;

        public DilemmaBoost? Evaluate(in LotSnapshot lot)
        {
            if (lot.TotalSpots <= 0 || lot.FreeSpots > _settings.LotTightSpots) return null;
            return new DilemmaBoost(CarArchetype.Plain, _settings.LotTight, DilemmaSolvable.Check(lot, _settings), true);
        }
    }
}
