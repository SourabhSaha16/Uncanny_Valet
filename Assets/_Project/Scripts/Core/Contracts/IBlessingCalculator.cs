using UncannyValet.Core.Events;

namespace UncannyValet.Core.Contracts
{
    /// <summary>Pure blessing maths. Never returns less than 0. Implemented by the Economy module.</summary>
    public interface IBlessingCalculator
    {
        int Calculate(in BlessingInput input);

        /// <summary>The same blessing with its parts; Total equals Calculate for the same input.</summary>
        BlessingBreakdown Explain(in BlessingInput input);
    }
}
