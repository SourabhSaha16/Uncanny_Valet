using System.Collections.Generic;
using UnityEngine;

namespace UncannyValet.Lot.Handmade
{
    /// <summary>
    /// Reads the marker components under a <see cref="HandmadeLot"/> into plain <see cref="HandmadeLotData"/> in world coordinates. This and
    /// <see cref="HandmadeSceneryReader"/> are the only code that touches scene objects; the validator and the authored layers work on the data.
    /// Markers are read in hierarchy order, wait points by their order number.
    /// </summary>
    public static class HandmadeLotReader
    {
        /// <summary>Reads everything under <paramref name="root"/> (inactive objects included).</summary>
        public static HandmadeLotData Read(HandmadeLot root)
        {
            var data = new HandmadeLotData
            {
                RootCount = root.GetComponentsInChildren<HandmadeLot>(true).Length,
                Requirements = root.Requirements,
                HasView = root.HasAuthoredView,
                View = root.AuthoredView,
            };
            ReadPieces(root, data);
            ReadRoads(root, data);
            ReadPeople(root, data);
            HandmadeSceneryReader.Read(root, data);
            return data;
        }

        private static void ReadPieces(HandmadeLot root, HandmadeLotData data)
        {
            foreach (var bay in root.GetComponentsInChildren<LotBay>(true))
                data.Bays.Add(new BayData(bay.name, bay.Number, bay.Kind, Rect(bay.transform, bay.Size)));
            foreach (var curb in root.GetComponentsInChildren<CurbZoneMarker>(true))
                data.Curbs.Add(new LotPiece(curb.name, Rect(curb.transform, curb.Size)));
            foreach (var slot in root.GetComponentsInChildren<QueueSlot>(true))
                data.Slots.Add(new LotPiece(slot.name, Rect(slot.transform, slot.CarSize), slot.Index));
        }

        private static void ReadRoads(HandmadeLot root, HandmadeLotData data)
        {
            foreach (var road in root.GetComponentsInChildren<CarEntryRoad>(true))
            {
                var t = road.transform;
                data.EntryRoads.Add(new EntryRoadData(road.name, World(t, road.Lane), t.TransformPoint(road.QueueTurn), World(t, road.Bend)));
            }
            foreach (var road in root.GetComponentsInChildren<CarExitRoad>(true))
                data.ExitRoads.Add(new ExitRoadData(road.name, World(road.transform, road.Lane), World(road.transform, road.CurbExit)));
            foreach (var lane in root.GetComponentsInChildren<AmbientLane>(true))
            {
                var route = new NamedRoute(lane.name, World(lane.transform, lane.Points));
                (lane.Side == AmbientLaneSide.EntryBack ? data.EntryBackLanes : data.ExitBackLanes).Add(route);
            }
        }

        private static void ReadPeople(HandmadeLot root, HandmadeLotData data)
        {
            foreach (var door in root.GetComponentsInChildren<LotDoor>(true))
                data.Doors.Add(new DoorData(door.name, door.Kind, door.transform.position));
            var waits = new List<PatronWaitPoint>(root.GetComponentsInChildren<PatronWaitPoint>(true));
            waits.Sort((a, b) => a.Order.CompareTo(b.Order));
            foreach (var wait in waits) data.WaitPoints.Add(new NamedPoint(wait.name, wait.transform.position));
            foreach (var spawn in root.GetComponentsInChildren<AmbientSpawnPoint>(true))
                data.SpawnPoints.Add(new NamedPoint(spawn.name, spawn.transform.position));
            var nodes = new Dictionary<WalkNode, int>();
            foreach (var node in root.GetComponentsInChildren<WalkNode>(true))
            {
                nodes[node] = data.WalkNodes.Count;
                data.WalkNodes.Add(new NamedPoint(node.name, node.transform.position));
            }
            foreach (var link in root.GetComponentsInChildren<WalkLink>(true))
                data.WalkLinks.Add(new WalkLinkData(link.name, IndexOf(nodes, link.A), IndexOf(nodes, link.B), link.Crosswalk));
            foreach (var start in root.GetComponentsInChildren<ValetStart>(true))
                data.ValetStarts.Add(new NamedPoint(start.name, start.transform.position));
        }

        private static int IndexOf(Dictionary<WalkNode, int> nodes, WalkNode node)
            => node != null && nodes.TryGetValue(node, out var index) ? index : -1;

        /// <summary>The rectangle of a marker: its position, the given size and its z rotation.</summary>
        internal static OrientedRect Rect(Transform t, Vector2 size) => new OrientedRect(t.position, size, t.eulerAngles.z);

        private static Vector2[] World(Transform t, Vector2[] local)
        {
            var world = new Vector2[local.Length];
            for (var i = 0; i < world.Length; i++) world[i] = t.TransformPoint(local[i]);
            return world;
        }
    }
}
