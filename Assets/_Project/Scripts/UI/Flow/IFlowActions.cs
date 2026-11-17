using UncannyValet.Core.Ids;

namespace UncannyValet.UI.Flow
{
    /// <summary>What the flow asks the game to do. Bootstrap implements it (the UI cannot touch scenes, the clock or the run state).</summary>
    public interface IFlowActions
    {
        /// <summary>Starts the shift of the lot that is loaded (its intro card was dismissed).</summary>
        void StartShift();

        /// <summary>Starts the shift again from the results screen.</summary>
        void RetryShift();

        /// <summary>Loads the lot's scene; its intro card shows once it is built.</summary>
        void LoadLot(LotId lot);

        /// <summary>Begins a fresh run: powers, upgrades, cleared lots and best results are forgotten.</summary>
        void StartNewRun();
    }
}
