using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Director
{
    /// <summary>One dilemma rule: reads the lot snapshot and says which archetype to favour. One small class per rule.</summary>
    public interface IDilemmaRule
    {
        /// <summary>Which rule this is, for events and the debug overlay.</summary>
        DilemmaKind Kind { get; }

        /// <summary>The boost when the trigger holds (flagged unsolvable when the player could not get out of it); null when it does not hold.</summary>
        DilemmaBoost? Evaluate(in LotSnapshot lot);
    }
}
