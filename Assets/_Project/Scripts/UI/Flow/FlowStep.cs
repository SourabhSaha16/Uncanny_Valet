namespace UncannyValet.UI.Flow
{
    /// <summary>The one thing the flow asks its host to do after an input.</summary>
    public enum FlowStep
    {
        /// <summary>Nothing: only the screen changed.</summary>
        None = 0,
        /// <summary>Start the shift of the loaded lot.</summary>
        StartShift = 1,
        /// <summary>Load the scene of <see cref="FlowMachine.IntroLot"/> (a fresh scene; the intro card shows once it loaded).</summary>
        LoadLot = 2,
        /// <summary>Forget the run (powers, upgrades, cleared lots) and show the title.</summary>
        StartNewRun = 3,
    }
}
