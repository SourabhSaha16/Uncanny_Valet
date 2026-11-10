namespace UncannyValet.Pedestrians
{
    /// <summary>What a walking person does this physics step when something may be in their way.</summary>
    public enum PedestrianBlockState
    {
        /// <summary>Nothing in the way: keep walking.</summary>
        Moving,
        /// <summary>Something solid is ahead: stand still and wait for it to clear.</summary>
        Waiting,
        /// <summary>Waited too long: walk on for a short while ignoring blockers (and not blocking others), so no route ever freezes.</summary>
        Passing
    }
}
