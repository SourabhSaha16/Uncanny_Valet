using System.Collections.Generic;
using UnityEngine;

namespace UncannyValet.Lot.Handmade
{
    /// <summary>
    /// The authored lot as plain data in world coordinates: everything the scene markers say, with no Unity objects. The
    /// <see cref="HandmadeLotReader"/> fills it from a scene; <see cref="HandmadeLotRules"/> checks it and the authored layers read it.
    /// Lists are in authored order (wait points and spawn points in marker order, queue slots by index).
    /// </summary>
    public sealed class HandmadeLotData
    {
        /// <summary>How many lot roots the scene holds (exactly one is allowed).</summary>
        public int RootCount { get; set; } = 1;
        public LotRequirements Requirements { get; set; } = new LotRequirements();
        /// <summary>The camera rectangle the author set; used only when <see cref="HasView"/>.</summary>
        public Rect View { get; set; }
        public bool HasView { get; set; }

        public List<BayData> Bays { get; } = new List<BayData>();
        public List<LotPiece> Curbs { get; } = new List<LotPiece>();
        public List<LotPiece> Slots { get; } = new List<LotPiece>();
        public List<LotPiece> Solids { get; } = new List<LotPiece>();
        public List<LotPiece> Hazards { get; } = new List<LotPiece>();
        /// <summary>Props that are marked solid but carry no collider.</summary>
        public List<LotPiece> SceneryWithoutCollider { get; } = new List<LotPiece>();
        public List<EntryRoadData> EntryRoads { get; } = new List<EntryRoadData>();
        public List<ExitRoadData> ExitRoads { get; } = new List<ExitRoadData>();
        public List<NamedRoute> EntryBackLanes { get; } = new List<NamedRoute>();
        public List<NamedRoute> ExitBackLanes { get; } = new List<NamedRoute>();
        public List<DoorData> Doors { get; } = new List<DoorData>();
        public List<NamedPoint> WaitPoints { get; } = new List<NamedPoint>();
        public List<NamedPoint> SpawnPoints { get; } = new List<NamedPoint>();
        public List<NamedPoint> WalkNodes { get; } = new List<NamedPoint>();
        public List<WalkLinkData> WalkLinks { get; } = new List<WalkLinkData>();
        public List<NamedPoint> ValetStarts { get; } = new List<NamedPoint>();
        public List<TileLayerData> TileLayers { get; } = new List<TileLayerData>();

        /// <summary>The queue slot centres ordered by index (slot 0 first).</summary>
        public Vector2[] SlotCentres()
        {
            var sorted = new List<LotPiece>(Slots);
            sorted.Sort((a, b) => a.Index.CompareTo(b.Index));
            var centres = new Vector2[sorted.Count];
            for (var i = 0; i < centres.Length; i++) centres[i] = sorted[i].Rect.Center;
            return centres;
        }

        /// <summary>The first door of that kind; false when there is none.</summary>
        public bool TryGetDoor(DoorKind kind, out DoorData door)
        {
            for (var i = 0; i < Doors.Count; i++)
                if (Doors[i].Kind == kind) { door = Doors[i]; return true; }
            door = default;
            return false;
        }
    }
}
