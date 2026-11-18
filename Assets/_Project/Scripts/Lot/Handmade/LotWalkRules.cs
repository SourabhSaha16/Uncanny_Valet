using System.Collections.Generic;
using UnityEngine;

namespace UncannyValet.Lot.Handmade
{
    /// <summary>Validator rules 15 to 20, 22 and 23: doors, wait and spawn points, the walk graph, the valet start, hazards, the view and the tile layers.</summary>
    internal static class LotWalkRules
    {
        public static void Check(HandmadeLotData d, List<LotIssue> issues)
        {
            var hasDoor = d.TryGetDoor(DoorKind.Main, out var main);
            if (!hasDoor) LotFindings.Error(issues, LotRule.Doors, "the lot needs a main door");
            if (d.WaitPoints.Count == 0) LotFindings.Error(issues, LotRule.Doors, "the lot needs patron wait points at the curb");
            if (d.Requirements.AmbientPedestrians > 0 && d.SpawnPoints.Count == 0)
                LotFindings.Error(issues, LotRule.SpawnPoints, "ambient pedestrians are on, so the lot needs ambient spawn points");
            Graph(d, main.Position, hasDoor, issues);
            Valet(d, issues);
            for (var i = 0; i < d.Hazards.Count; i++)
                if (d.Hazards[i].Rect.Size.x <= 0f) LotFindings.Error(issues, LotRule.HazardSize, "the hazard patch has no size", d.Hazards[i].Name);
            for (var i = 0; i < d.TileLayers.Count; i++)
                if (d.TileLayers[i].MustBeSolid && !d.TileLayers[i].HasCollider)
                    LotFindings.Warn(issues, LotRule.TileCollider, d.TileLayers[i].Kind + " tilemap has no collider, cars and the valet will drive through it", d.TileLayers[i].Name);
            View(d, issues);
        }

        private static void Graph(HandmadeLotData d, Vector2 door, bool hasDoor, List<LotIssue> issues)
        {
            for (var i = 0; i < d.WalkLinks.Count; i++) Link(d, d.WalkLinks[i], issues);
            if (!hasDoor) return;
            if (d.WalkNodes.Count == 0)
            {
                LotFindings.Error(issues, LotRule.WalkConnected, "the lot has no walk nodes, so nobody can reach the main door");
                return;
            }
            var graph = new WalkGraph(d.WalkNodes, d.WalkLinks);
            var target = graph.Nearest(door);
            for (var i = 0; i < d.SpawnPoints.Count; i++) Reach(graph, target, d.SpawnPoints[i].Position, d.SpawnPoints[i].Name, issues);
            for (var i = 0; i < d.WaitPoints.Count; i++) Reach(graph, target, d.WaitPoints[i].Position, d.WaitPoints[i].Name, issues);
            for (var i = 0; i < d.Bays.Count; i++) Reach(graph, target, d.Bays[i].Rect.Center, d.Bays[i].Name, issues);
            for (var i = 0; i < d.Slots.Count; i++) Reach(graph, target, d.Slots[i].Rect.Center, d.Slots[i].Name, issues);
        }

        private static void Reach(WalkGraph graph, int target, Vector2 from, string name, List<LotIssue> issues)
        {
            if (!graph.Connected(graph.Nearest(from), target))
                LotFindings.Error(issues, LotRule.WalkConnected, "no walk reaches the main door from here", name);
        }

        private static void Link(HandmadeLotData d, WalkLinkData link, List<LotIssue> issues)
        {
            if (link.A < 0 || link.B < 0 || link.A >= d.WalkNodes.Count || link.B >= d.WalkNodes.Count || link.A == link.B)
            {
                LotFindings.Error(issues, LotRule.WalkCrossesSolid, "the walk link does not join two different walk nodes", link.Name);
                return;
            }
            var a = d.WalkNodes[link.A].Position;
            var b = d.WalkNodes[link.B].Position;
            for (var s = 0; s < d.Solids.Count; s++)
                if (d.Solids[s].Rect.IntersectsSegment(a, b))
                    LotFindings.Error(issues, LotRule.WalkCrossesSolid, "the walk link crosses solid " + d.Solids[s].Name, link.Name);
        }

        private static void Valet(HandmadeLotData d, List<LotIssue> issues)
        {
            if (d.ValetStarts.Count != 1)
            {
                LotFindings.Error(issues, LotRule.ValetStart, "the lot needs exactly one valet start, found " + d.ValetStarts.Count);
                return;
            }
            for (var s = 0; s < d.Solids.Count; s++)
                if (d.Solids[s].Rect.Contains(d.ValetStarts[0].Position))
                    LotFindings.Error(issues, LotRule.ValetStart, "the valet would start inside solid " + d.Solids[s].Name, d.ValetStarts[0].Name);
        }

        private static void View(HandmadeLotData d, List<LotIssue> issues)
        {
            if (!d.HasView) return;
            var play = LotBounds.Gameplay(d);
            if (play.xMin < d.View.xMin || play.yMin < d.View.yMin || play.xMax > d.View.xMax || play.yMax > d.View.yMax)
                LotFindings.Warn(issues, LotRule.ViewTooSmall, "the camera view does not hold all the gameplay markers");
        }
    }
}
