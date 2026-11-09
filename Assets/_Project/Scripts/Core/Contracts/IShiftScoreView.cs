namespace UncannyValet.Core.Contracts
{
    /// <summary>Read-only view of this shift's score. Implemented by the Economy module's ShiftScore.</summary>
    public interface IShiftScoreView
    {
        /// <summary>Tips earned minus complaint penalties, never below 0.</summary>
        int Score { get; }

        /// <summary>The tip multiplier for the next happy patron, 1 up to the combo cap.</summary>
        int ComboMultiplier { get; }

        /// <summary>Angry patrons so far this shift.</summary>
        int Complaints { get; }

        /// <summary>0 to 3 stars for the score so far.</summary>
        int Stars { get; }

        int OneStarScore { get; }
        int TwoStarScore { get; }
        int ThreeStarScore { get; }
    }
}
