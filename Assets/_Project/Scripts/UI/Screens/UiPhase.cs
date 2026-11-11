namespace UncannyValet.UI.Screens
{
    /// <summary>Which screen the game UI is showing.</summary>
    public enum UiPhase
    {
        /// <summary>The title screen; the shift clock has not started.</summary>
        Title,
        /// <summary>The how-to-play screen, shown once per page load after the title; the shift clock has not started.</summary>
        HowToPlay,
        /// <summary>A shift is running: the HUD.</summary>
        Playing,
        /// <summary>The shift ended: the results screen.</summary>
        Results
    }
}
