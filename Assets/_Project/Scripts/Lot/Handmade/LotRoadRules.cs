using System.Collections.Generic;
using UnityEngine;

namespace UncannyValet.Lot.Handmade
{
    /// <summary>Validator rules 10 to 14 and 21: the entry and exit roads, the car routes against the solids and the ambient lanes.</summary>
    internal static class LotRoadRules
    {
        public static void Check(HandmadeLotData d, List<LotIssue> issues)
        {
            Entry(d, issues);
            Exit(d, issues);
            if (d.Requirements.AmbientTraffic && (!HasLane(d.EntryBackLanes) || !HasLane(d.ExitBackLanes)))
                LotFindings.Error(issues, LotRule.AmbientLanes, "ambient traffic is on, so the lot needs an entry-back lane and an exit-back lane with 2 or more points");
            var routes = Routes(d);
            for (var r = 0; r < routes.Count; r++)
            {
                for (var s = 0; s < d.Solids.Count; s++)
                    if (RouteMath.Crosses(routes[r].Points, d.Solids[s].Rect))
                        LotFindings.Error(issues, LotRule.RouteCrossesSolid, "the car route crosses solid " + d.Solids[s].Name, routes[r].Name);
                for (var s = 0; s < d.SceneryWithoutCollider.Count; s++)
                    if (RouteMath.Crosses(routes[r].Points, d.SceneryWithoutCollider[s].Rect))
                        LotFindings.Warn(issues, LotRule.SceneryOnRoute, "this prop sits on the car route " + routes[r].Name + " but has no collider", d.SceneryWithoutCollider[s].Name);
            }
        }

        private static void Entry(HandmadeLotData d, List<LotIssue> issues)
        {
            if (d.EntryRoads.Count != 1)
            {
                LotFindings.Error(issues, LotRule.EntryRoad, "the lot needs exactly one car entry road, found " + d.EntryRoads.Count);
                return;
            }
            var road = d.EntryRoads[0];
            if (road.Lane.Length < 2) LotFindings.Error(issues, LotRule.EntryRoad, "the entry road needs 2 or more points", road.Name);
            else if (LotBounds.View(d).Contains(road.Lane[0]))
                LotFindings.Error(issues, LotRule.EntryRoad, "the entry road must start off screen", road.Name);
            if (RouteMath.DistanceToPolyline(road.QueueTurn, road.Lane) > d.Requirements.LaneTolerance)
                LotFindings.Error(issues, LotRule.QueueTurn, "the queue-turn point must lie on the entry lane", road.Name);
            var last = road.Bend.Length > 0 ? road.Bend[road.Bend.Length - 1] : road.QueueTurn;
            for (var i = 0; i < d.Slots.Count; i++)
            {
                if (d.Slots[i].Index != 0) continue;
                if (Mathf.Abs(d.Slots[i].Rect.ToLocal(last).x) > d.Slots[i].Rect.Size.x * 0.5f)
                    LotFindings.Error(issues, LotRule.QueueTurn, "the bend must end in line with queue slot 0 so cars drive straight into it", road.Name);
            }
        }

        private static void Exit(HandmadeLotData d, List<LotIssue> issues)
        {
            if (d.ExitRoads.Count != 1)
            {
                LotFindings.Error(issues, LotRule.ExitRoad, "the lot needs exactly one car exit road, found " + d.ExitRoads.Count);
                return;
            }
            var road = d.ExitRoads[0];
            if (road.Lane.Length < 2 || road.CurbExit.Length < 2)
            {
                LotFindings.Error(issues, LotRule.ExitRoad, "the exit lane and the curb-exit route each need 2 or more points", road.Name);
                return;
            }
            if (d.Curbs.Count == 1 && !d.Curbs[0].Rect.Contains(road.CurbExit[0]))
                LotFindings.Error(issues, LotRule.ExitRoad, "the curb-exit route must start inside the curb zone", road.Name);
            if (RouteMath.DistanceToPolyline(road.CurbExit[road.CurbExit.Length - 1], road.Lane) > d.Requirements.LaneTolerance)
                LotFindings.Error(issues, LotRule.ExitRoad, "the curb-exit route must end on the exit lane", road.Name);
        }

        private static bool HasLane(List<NamedRoute> lanes)
        {
            for (var i = 0; i < lanes.Count; i++)
                if (lanes[i].Points.Length >= 2) return true;
            return false;
        }

        // Every driving route as authored (corners not yet rounded).
        private static List<NamedRoute> Routes(HandmadeLotData d)
        {
            var routes = new List<NamedRoute>();
            for (var i = 0; i < d.EntryRoads.Count; i++)
            {
                routes.Add(new NamedRoute(d.EntryRoads[i].Name, d.EntryRoads[i].Lane));
                var raw = new List<Vector2> { d.EntryRoads[i].QueueTurn };
                raw.AddRange(d.EntryRoads[i].Bend);
                if (d.Slots.Count > 0) raw.Add(d.SlotCentres()[d.Slots.Count - 1]);
                routes.Add(new NamedRoute(d.EntryRoads[i].Name + " queue route", raw.ToArray()));
            }
            for (var i = 0; i < d.ExitRoads.Count; i++)
            {
                routes.Add(new NamedRoute(d.ExitRoads[i].Name, d.ExitRoads[i].Lane));
                routes.Add(new NamedRoute(d.ExitRoads[i].Name + " curb exit", d.ExitRoads[i].CurbExit));
            }
            for (var i = 0; i < d.EntryBackLanes.Count; i++) routes.Add(d.EntryBackLanes[i]);
            for (var i = 0; i < d.ExitBackLanes.Count; i++) routes.Add(d.ExitBackLanes[i]);
            return routes;
        }
    }
}
