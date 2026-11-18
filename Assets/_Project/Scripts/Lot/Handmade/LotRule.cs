namespace UncannyValet.Lot.Handmade
{
    /// <summary>
    /// The validator rules, numbered as in docs/redesign/handmade_lots_plan.md (the number is the value). Append only. Rule 24 is unused and
    /// rule 25 (a Handmade layout kind needs a prefab) is checked by the run integration, which knows the lot definitions.
    /// </summary>
    public enum LotRule
    {
        SingleRoot = 1,
        OneCurb = 2,
        CurbUnrotated = 3,
        QueueNumbering = 4,
        QueueSharedRotation = 5,
        SlotTouchesCurb = 6,
        BayNumbering = 7,
        PiecesOverlap = 8,
        BayTooSmall = 9,
        EntryRoad = 10,
        QueueTurn = 11,
        ExitRoad = 12,
        RouteCrossesSolid = 13,
        AmbientLanes = 14,
        Doors = 15,
        SpawnPoints = 16,
        WalkConnected = 17,
        WalkCrossesSolid = 18,
        ValetStart = 19,
        HazardSize = 20,
        SceneryOnRoute = 21,
        ViewTooSmall = 22,
        TileCollider = 23,
    }
}
