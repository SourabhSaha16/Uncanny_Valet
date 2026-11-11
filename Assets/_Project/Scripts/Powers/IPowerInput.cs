namespace UncannyValet.Powers
{
    /// <summary>The activate-power button. Kept in this module so the Valet input path is untouched; Bootstrap may register another source.</summary>
    public interface IPowerInput
    {
        /// <summary>True only on the frame the button was pressed.</summary>
        bool ActivatePressed { get; }
    }
}
