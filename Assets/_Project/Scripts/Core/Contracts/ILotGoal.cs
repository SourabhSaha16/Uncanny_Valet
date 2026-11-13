using UncannyValet.Core.Ids;

namespace UncannyValet.Core.Contracts
{
    /// <summary>The goal of the lot being played: the blessings needed at shift end to clear it. Registered when the lot starts.</summary>
    public interface ILotGoal
    {
        /// <summary>The lot being played.</summary>
        LotId Lot { get; }

        /// <summary>Blessings that must be reached by the end of the shift (at least 1).</summary>
        int BlessingThreshold { get; }
    }
}
