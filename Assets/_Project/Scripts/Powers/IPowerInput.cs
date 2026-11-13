namespace UncannyValet.Powers
{
    /// <summary>The power keys (1 to 4 on the keyboard). Kept in this module so the Valet input path is untouched; Bootstrap registers the source.</summary>
    public interface IPowerInput
    {
        /// <summary>True only on the frame the key of the power slot (0 to 3, key 1 to 4) was pressed.</summary>
        bool PowerPressed(int slot);
    }
}
