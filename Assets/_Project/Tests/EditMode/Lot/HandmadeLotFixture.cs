using System.Collections.Generic;
using UncannyValet.Lot.Handmade;
using UnityEngine;

namespace UncannyValet.Tests.Lot
{
    /// <summary>
    /// A small valid hand-made lot as data (no errors, no warnings): a curb, two queue slots below it, two regular bays to its right, a wall on the
    /// left with a main and a side door, an entry road below, an exit road above and a walk graph joining everything. Rule tests change one thing.
    /// </summary>
    public static class HandmadeLotFixture
    {
        public static readonly Vector2 BaySize = new Vector2(2.6f, 4.4f);
        public static readonly Vector2 CarSlot = new Vector2(2.2f, 4.5f);

        public static HandmadeLotData Valid()
        {
            var d = new HandmadeLotData();
            d.Bays.Add(new BayData("Bay 1", 1, BayKind.Regular, new OrientedRect(new Vector2(6f, 0f), BaySize, 0f)));
            d.Bays.Add(new BayData("Bay 2", 2, BayKind.Regular, new OrientedRect(new Vector2(10f, 0f), BaySize, 0f)));
            d.Curbs.Add(new LotPiece("Curb", new OrientedRect(Vector2.zero, new Vector2(3f, 5f), 0f)));
            d.Slots.Add(new LotPiece("Slot 0", new OrientedRect(new Vector2(0f, -5.55f), CarSlot, 0f), 0));
            d.Slots.Add(new LotPiece("Slot 1", new OrientedRect(new Vector2(0f, -10.2f), CarSlot, 0f), 1));
            d.Solids.Add(new LotPiece("Wall", new OrientedRect(new Vector2(-8f, 0f), new Vector2(1f, 10f), 0f)));
            d.EntryRoads.Add(new EntryRoadData("Entry", new[] { new Vector2(-30f, -14f), new Vector2(30f, -14f) },
                new Vector2(-2.4f, -14f), new[] { new Vector2(0f, -11.6f) }));
            d.ExitRoads.Add(new ExitRoadData("Exit", new[] { new Vector2(-30f, 12f), new Vector2(30f, 12f) },
                new[] { Vector2.zero, new Vector2(0f, 10f), new Vector2(2f, 12f) }));
            d.EntryBackLanes.Add(new NamedRoute("Entry back", new[] { new Vector2(30f, -17f), new Vector2(-30f, -17f) }));
            d.ExitBackLanes.Add(new NamedRoute("Exit back", new[] { new Vector2(30f, 15f), new Vector2(-30f, 15f) }));
            d.Doors.Add(new DoorData("Main door", DoorKind.Main, new Vector2(-7f, 0f)));
            d.Doors.Add(new DoorData("Side door", DoorKind.Side, new Vector2(-7f, 4f)));
            d.WaitPoints.Add(new NamedPoint("Wait 1", new Vector2(-3f, 0.5f)));
            d.WaitPoints.Add(new NamedPoint("Wait 2", new Vector2(-3f, -0.5f)));
            d.SpawnPoints.Add(new NamedPoint("Spawn", new Vector2(14f, 4f)));
            AddGraph(d.WalkNodes, d.WalkLinks);
            d.ValetStarts.Add(new NamedPoint("Valet", new Vector2(3f, -1f)));
            return d;
        }

        public static List<LotIssue> Check(HandmadeLotData data)
        {
            var issues = new List<LotIssue>();
            HandmadeLotRules.Validate(data, issues);
            return issues;
        }

        public static bool Has(List<LotIssue> issues, LotRule rule, LotSeverity severity)
        {
            foreach (var issue in issues)
                if (issue.Rule == rule && issue.Severity == severity) return true;
            return false;
        }

        private static void AddGraph(List<NamedPoint> nodes, List<WalkLinkData> links)
        {
            nodes.Add(new NamedPoint("N0", new Vector2(-6f, 0f)));
            nodes.Add(new NamedPoint("N1", new Vector2(-3f, 0f)));
            nodes.Add(new NamedPoint("N2", new Vector2(3f, 4f)));
            nodes.Add(new NamedPoint("N3", new Vector2(12f, 4f)));
            nodes.Add(new NamedPoint("N4", new Vector2(8f, -3f)));
            for (var i = 0; i < 4; i++) links.Add(new WalkLinkData("L" + i, i, i + 1));
            links.Add(new WalkLinkData("L4", 4, 1, true));
        }
    }
}
