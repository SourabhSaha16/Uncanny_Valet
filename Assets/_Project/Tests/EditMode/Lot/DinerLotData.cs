using System.Collections.Generic;
using UncannyValet.Lot;
using UncannyValet.Lot.Handmade;
using UnityEngine;

namespace UncannyValet.Tests.Lot
{
    /// <summary>Builds <see cref="HandmadeLotData"/> from the parametric Diner (geometry, roads, walks) so the authored layers can be compared with it.</summary>
    public static class DinerLotData
    {
        public static HandmadeLotData From(LotConfig c, LotGeometry g, RoadLayout road, PedestrianPathMap paths, Vector2 origin)
        {
            var d = new HandmadeLotData();
            AddBays(d, g, origin);
            d.Curbs.Add(new LotPiece("Curb", new OrientedRect(g.Curb.center + origin, g.Curb.size, 0f)));
            for (var i = 0; i < g.QueueSlots.Length; i++)
                d.Slots.Add(new LotPiece("Slot " + i, new OrientedRect(g.QueueSlots[i] + origin, c.QueueCarSize, c.QueueRotationDegrees), i));
            AddRoads(d, road);
            d.Doors.Add(new DoorData("Main", DoorKind.Main, paths.DinerDoor));
            d.Doors.Add(new DoorData("Side", DoorKind.Side, paths.SideDoor));
            for (var i = 0; i < paths.CurbSlotCount; i++) d.WaitPoints.Add(new NamedPoint("Wait " + i, paths.CurbSlot(i)));
            var route = new List<Vector2>();
            for (var i = 0; i < paths.RoadRouteCount; i++)
            {
                paths.RoadRoute(i, false, route);
                d.SpawnPoints.Add(new NamedPoint("Spawn " + i, route[0]));
            }
            var crossing = new Vector2(g.CrossingX, g.AisleY) + origin;
            d.WalkNodes.Add(new NamedPoint("Door", paths.DinerDoor));
            d.WalkNodes.Add(new NamedPoint("Crossing", crossing));
            d.WalkLinks.Add(new WalkLinkData("Door to crossing", 0, 1));
            return d;
        }

        private static void AddBays(HandmadeLotData d, LotGeometry g, Vector2 origin)
        {
            for (var i = 0; i < g.Spots.Length; i++) d.Bays.Add(Bay("Bay " + (i + 1), i + 1, BayKind.Regular, g.Spots[i], origin));
            for (var i = 0; i < g.ChargingSpots.Length; i++) d.Bays.Add(Bay("EV " + (i + 1), i + 1, BayKind.Charging, g.ChargingSpots[i], origin));
            for (var i = 0; i < g.WashStations.Length; i++) d.Bays.Add(Bay("Wash " + (i + 1), i + 1, BayKind.Wash, g.WashStations[i], origin));
        }

        private static BayData Bay(string name, int number, BayKind kind, Rect r, Vector2 origin)
            => new BayData(name, number, kind, new OrientedRect(r.center + origin, r.size, 0f));

        private static void AddRoads(HandmadeLotData d, RoadLayout road)
        {
            var first = new List<Vector2>();
            road.QueueEntryRoute(0, first);
            var bend = first.GetRange(1, first.Count - 2).ToArray();
            var lane = Copy(road.EastLane);
            var turn = lane[0] + Vector2.right * road.QueueTurnDistance;
            d.EntryRoads.Add(new EntryRoadData("Entry", lane, turn, bend));
            var curbExit = new List<Vector2>();
            road.CurbExitRoute(curbExit);
            d.ExitRoads.Add(new ExitRoadData("Exit", Copy(road.TopLane), curbExit.ToArray()));
            d.EntryBackLanes.Add(new NamedRoute("Entry back", Copy(road.EastBackLane)));
            d.ExitBackLanes.Add(new NamedRoute("Exit back", Copy(road.TopBackLane)));
        }

        private static Vector2[] Copy(IReadOnlyList<Vector2> points)
        {
            var copy = new Vector2[points.Count];
            for (var i = 0; i < copy.Length; i++) copy[i] = points[i];
            return copy;
        }
    }
}
