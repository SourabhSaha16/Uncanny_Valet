namespace UncannyValet.EditorTools.Import
{
    /// <summary>How a texture is cut into sprites.</summary>
    public enum SliceMode
    {
        Single,
        Grid,
        Auto
    }

    /// <summary>Result of <see cref="SlicePlanner.Plan"/>: the mode and, for Grid, the cell size in pixels.</summary>
    public readonly struct SlicePlan
    {
        public readonly SliceMode Mode;
        public readonly int CellWidth;
        public readonly int CellHeight;

        public SlicePlan(SliceMode mode, int cellWidth = 0, int cellHeight = 0)
        {
            Mode = mode;
            CellWidth = cellWidth;
            CellHeight = cellHeight;
        }
    }
}
