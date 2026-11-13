namespace UncannyValet.Core.Ids
{
    /// <summary>
    /// The story powers a crazy lot can grant (each has a base level and an upgraded level 2). Stored as integers in
    /// assets: append only, never reorder. Distinct from the old meter-level <c>PowerKind</c>.
    /// </summary>
    public enum StoryPower
    {
        /// <summary>No power.</summary>
        None = 0,
        ThunderStride = 1,
        StormWind = 2,
        FreezeTime = 3,
        MjolnirRecall = 4,
    }
}
