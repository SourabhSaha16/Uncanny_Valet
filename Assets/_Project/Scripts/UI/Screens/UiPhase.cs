namespace UncannyValet.UI.Screens
{
    /// <summary>Which screen the game UI is showing.</summary>
    public enum UiPhase
    {
        /// <summary>The title screen; the shift clock has not started.</summary>
        Title,
        /// <summary>A shift is running: the HUD.</summary>
        Playing,
        /// <summary>The shift ended: the results screen.</summary>
        Results
    }
}
