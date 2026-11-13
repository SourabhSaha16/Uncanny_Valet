using UncannyValet.Core.Contracts;
using UncannyValet.Core.Ids;

namespace UncannyValet.Run
{
    /// <summary>The goal of the lot being played: its id and the blessings needed at shift end. Built from the lot's definition when the lot starts.</summary>
    public sealed class LotGoal : ILotGoal
    {
        public LotGoal(LotId lot, int blessingThreshold)
        {
            Lot = lot;
            BlessingThreshold = blessingThreshold;
        }

        public LotId Lot { get; }
        public int BlessingThreshold { get; }
    }
}
