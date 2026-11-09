namespace UncannyValet.EditorTools.Import
{
    /// <summary>Pure rules mapping an art asset path to a slicing plan (see docs/changelogs/pivot-art.md).</summary>
    public static class SlicePlanner
    {
        public const int CarCell = 128;
        public const int FireTruckCell = 160;
        public const int CharacterCell = 32;
        public const int RoadCell = 32;
        public const int TreeWidth = 48;
        public const int TreeHeight = 64;

        /// <summary>Returns how the texture at <paramref name="path"/> (size w x h) should be sliced.</summary>
        public static SlicePlan Plan(string path, int w, int h)
        {
            if (path.Contains("/Cars/Cars-Sheet")) return new SlicePlan(SliceMode.Grid, CarCell, CarCell);
            if (path.Contains("/Cars/FireTruck")) return new SlicePlan(SliceMode.Grid, FireTruckCell, FireTruckCell);
            if (path.Contains("/Environment/Trees/")) return new SlicePlan(SliceMode.Grid, TreeWidth, TreeHeight);
            if (path.Contains("/Environment/Road/")) return new SlicePlan(SliceMode.Grid, RoadCell, RoadCell);
            if (path.Contains("/Environment/Tiles/") || path.Contains("/Environment/Buildings/"))
                return new SlicePlan(SliceMode.Auto);
            if (path.Contains("/Characters/") && IsMultiCell(w, h))
                return new SlicePlan(SliceMode.Grid, CharacterCell, CharacterCell);
            return new SlicePlan(SliceMode.Single);
        }

        /// <summary>True when sprites cut from this asset pivot at the bottom centre (buildings).</summary>
        public static bool UsesBottomPivot(string path)
        {
            return path.Contains("/Environment/Buildings/") || path.Contains("/Environment/Stations/");
        }

        private static bool IsMultiCell(int w, int h)
        {
            bool aligned = w % CharacterCell == 0 && h % CharacterCell == 0;
            return aligned && (w > CharacterCell || h > CharacterCell);
        }
    }
}
