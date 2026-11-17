namespace UncannyValet.UI.Flow
{
    /// <summary>Which screen of the story flow the game is on.</summary>
    public enum FlowState
    {
        /// <summary>The title screen.</summary>
        Title = 0,
        /// <summary>The how-to-play screen, shown once per page load.</summary>
        HowToPlay = 1,
        /// <summary>The intro card of the lot about to be played.</summary>
        LotIntro = 2,
        /// <summary>A shift is running.</summary>
        Playing = 3,
        /// <summary>The results screen of the lot just played.</summary>
        Results = 4,
        /// <summary>A reward card for a power or upgrade just earned.</summary>
        Reward = 5,
        /// <summary>The card that says a failed crazy lot threw Thor back to Midgard.</summary>
        SentBack = 6,
        /// <summary>The end card of the run (TO BE CONTINUED or THOR IS HOME).</summary>
        EndCard = 7,
    }
}
