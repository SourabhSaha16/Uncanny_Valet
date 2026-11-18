using System.Collections.Generic;
using UnityEngine;

namespace UncannyValet.Lot.Handmade
{
    /// <summary>Validator rules 1 to 9: the root, the curb, the queue slots and the bays.</summary>
    internal static class LotPieceRules
    {
        private const float RotationTolerance = 0.5f;

        public static void Check(HandmadeLotData d, List<LotIssue> issues)
        {
            if (d.RootCount != 1) LotFindings.Error(issues, LotRule.SingleRoot, "the lot needs exactly one lot root, found " + d.RootCount);
            Curb(d, issues);
            Queue(d, issues);
            Bays(d, issues);
            Overlaps(d, issues);
        }

        private static void Curb(HandmadeLotData d, List<LotIssue> issues)
        {
            if (d.Curbs.Count != 1)
            {
                LotFindings.Error(issues, LotRule.OneCurb, "the lot needs exactly one curb zone, found " + d.Curbs.Count);
                return;
            }
            var curb = d.Curbs[0];
            if (Mathf.Abs(Mathf.DeltaAngle(curb.Rect.RotationDegrees, 0f)) > RotationTolerance)
                LotFindings.Error(issues, LotRule.CurbUnrotated, "the curb zone must not be rotated (cars are judged facing up the screen)", curb.Name);
        }

        private static void Queue(HandmadeLotData d, List<LotIssue> issues)
        {
            var count = d.Slots.Count;
            var req = d.Requirements;
            if (count < req.MinQueueSlots || count > req.MaxQueueSlots)
                LotFindings.Error(issues, LotRule.QueueNumbering, "the lot has " + count + " queue slots, it needs " + req.MinQueueSlots + " to " + req.MaxQueueSlots);
            var indices = new List<int>();
            for (var i = 0; i < count; i++) indices.Add(d.Slots[i].Index);
            if (count > 0 && !LotFindings.IsRun(indices, 0))
                LotFindings.Error(issues, LotRule.QueueNumbering, "queue slot indices must run 0, 1, 2 ... with no gap or repeat");
            for (var i = 1; i < count; i++)
                if (Mathf.Abs(Mathf.DeltaAngle(d.Slots[0].Rect.RotationDegrees, d.Slots[i].Rect.RotationDegrees)) > RotationTolerance)
                    LotFindings.Error(issues, LotRule.QueueSharedRotation, "all queue slots must share one rotation", d.Slots[i].Name);
            SlotTouchesCurb(d, issues);
        }

        private static void SlotTouchesCurb(HandmadeLotData d, List<LotIssue> issues)
        {
            if (d.Curbs.Count != 1) return;
            for (var i = 0; i < d.Slots.Count; i++)
            {
                if (d.Slots[i].Index != 0) continue;
                if (d.Curbs[0].Rect.Distance(d.Slots[i].Rect) > d.Requirements.MaxCurbGap)
                    LotFindings.Error(issues, LotRule.SlotTouchesCurb, "queue slot 0 must touch the curb zone (gap at most " + d.Requirements.MaxCurbGap + ")", d.Slots[i].Name);
                return;
            }
        }

        private static void Bays(HandmadeLotData d, List<LotIssue> issues)
        {
            var found = new List<int>[3] { new List<int>(), new List<int>(), new List<int>() };
            for (var i = 0; i < d.Bays.Count; i++)
            {
                found[(int)d.Bays[i].Kind].Add(d.Bays[i].Number);
                var size = d.Bays[i].Rect.Size;
                if (size.x < d.Requirements.MinBaySize.x || size.y < d.Requirements.MinBaySize.y)
                    LotFindings.Error(issues, LotRule.BayTooSmall, "the bay is smaller than a car (" + size.x + " x " + size.y + ")", d.Bays[i].Name);
            }
            if (found[(int)BayKind.Regular].Count == 0) LotFindings.Error(issues, LotRule.BayNumbering, "the lot needs at least one regular bay");
            for (var k = 0; k < found.Length; k++)
                if (!LotFindings.IsRun(found[k], 1))
                    LotFindings.Error(issues, LotRule.BayNumbering, (BayKind)k + " bay numbers must run 1, 2, 3 ... with no gap or repeat");
            if (d.Requirements.NeedsCharging && found[(int)BayKind.Charging].Count == 0)
                LotFindings.Error(issues, LotRule.BayNumbering, "the lot's modifiers need an EV charging bay and it has none");
            if (d.Requirements.NeedsWash && found[(int)BayKind.Wash].Count == 0)
                LotFindings.Error(issues, LotRule.BayNumbering, "the lot's modifiers need a wash bay and it has none");
        }

        private static void Overlaps(HandmadeLotData d, List<LotIssue> issues)
        {
            var names = new List<string>();
            var rects = new List<OrientedRect>();
            for (var i = 0; i < d.Bays.Count; i++) { names.Add(d.Bays[i].Name); rects.Add(d.Bays[i].Rect); }
            for (var i = 0; i < d.Slots.Count; i++) { names.Add(d.Slots[i].Name); rects.Add(d.Slots[i].Rect); }
            for (var i = 0; i < d.Curbs.Count; i++) { names.Add(d.Curbs[i].Name); rects.Add(d.Curbs[i].Rect); }
            for (var a = 0; a < rects.Count; a++)
                for (var b = a + 1; b < rects.Count; b++)
                    if (rects[a].Overlaps(rects[b]))
                        LotFindings.Error(issues, LotRule.PiecesOverlap, "overlaps " + names[b], names[a]);
        }
    }
}
