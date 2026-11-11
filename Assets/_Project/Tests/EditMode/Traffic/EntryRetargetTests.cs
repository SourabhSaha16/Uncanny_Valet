using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Traffic;
using UnityEngine;

namespace UncannyValet.Tests.Traffic
{
    /// <summary>The entry route is re-targeted while the car drives in, so a queue slot that frees up is used.</summary>
    public class EntryRetargetTests
    {
        private const float Dt = 0.02f;
        private const int MaxSteps = 6000;
        private const float SlotSpacing = 2f;
        private const float SlotZeroY = 6f;

        private sealed class ColumnRoad : IRoadLayout
        {
            public IReadOnlyList<Vector2> EastLane { get; } = new[] { new Vector2(-10f, 0f), new Vector2(30f, 0f) };
            public IReadOnlyList<Vector2> TopLane { get; } = new[] { new Vector2(-10f, 10f), new Vector2(30f, 10f) };
            public IReadOnlyList<Vector2> EastBackLane { get; } = new[] { new Vector2(30f, -3f), new Vector2(-10f, -3f) };
            public IReadOnlyList<Vector2> TopBackLane { get; } = new[] { new Vector2(30f, 13f), new Vector2(-10f, 13f) };
            public float QueueTurnDistance => 20f;
            public float CurbMergeDistance => 20f;

            // A straight column up from the lane; slot 0 is the furthest along it.
            public void QueueEntryRoute(int slot, List<Vector2> into)
            {
                into.Clear();
                into.Add(new Vector2(10f, 0f));
                into.Add(new Vector2(10f, SlotZeroY - SlotSpacing * slot));
            }

            public void CurbExitRoute(List<Vector2> into) => into.Clear();
        }

        private sealed class ScriptedSlots : IQueueSlotSource
        {
            public int Answer = -1;
            public int Asked;
            public int Reevaluate(int currentSlot)
            {
                Asked++;
                return Answer < 0 ? currentSlot : Answer;
            }
        }

        private ScriptedSlots _slots;
        private TripRouter _router;

        [SetUp]
        public void SetUp()
        {
            var road = new ColumnRoad();
            var east = new LanePath();
            east.Rebuild(road.EastLane);
            var top = new LanePath();
            top.Rebuild(road.TopLane);
            var follow = new FollowSettings(5f, 0.6f, 0.8f, 3f, 6f);
            var merge = new MergeGapRule(2.5f, 2.5f, 0.8f);
            var tuning = new TripTuning(2f, 2f, 2.5f, 2f, 0.02f);
            _slots = new ScriptedSlots();
            var context = new TripContext(road, east, top, new LaneOccupancy(16), follow, merge, tuning, new TripSpeeds(5f, 2.5f))
            {
                Slots = _slots,
            };
            _router = new TripRouter(context, 4);
        }

        private TripPose Run(int trip, System.Action<TripPose> each = null)
        {
            TripPose pose = default;
            for (var i = 0; i < MaxSteps && _router.Step(trip, Dt, out pose); i++)
            {
                each?.Invoke(pose);
                if (pose.Phase == TripPhase.Finished) break;
            }
            return pose;
        }

        [Test]
        public void WithoutAChange_TheCarStopsAtItsAssignedSlot()
        {
            var pose = Run(_router.BeginEntry(2));
            Assert.AreEqual(TripPhase.Finished, pose.Phase);
            Assert.AreEqual(SlotZeroY - 2 * SlotSpacing, pose.Position.y, 0.05f);
            Assert.Greater(_slots.Asked, 0, "the trip keeps asking while it drives");
        }

        [Test]
        public void SlotFreedWhileOnTheRoute_TheCarDrivesOnToTheFreeSlot()
        {
            var trip = _router.BeginEntry(1);
            var pose = Run(trip, p => { if (p.Phase == TripPhase.OnRoute) _slots.Answer = 0; });
            Assert.AreEqual(TripPhase.Finished, pose.Phase);
            Assert.AreEqual(SlotZeroY, pose.Position.y, 0.05f);
        }

        [Test]
        public void SlotFreedWhileStillOnTheEastLane_TheCarTurnsIntoTheFreeSlot()
        {
            _slots.Answer = 0;
            var pose = Run(_router.BeginEntry(2));
            Assert.AreEqual(SlotZeroY, pose.Position.y, 0.05f);
        }

        [Test]
        public void ASlotFurtherBack_IsIgnored()
        {
            _slots.Answer = 2;
            var pose = Run(_router.BeginEntry(0));
            Assert.AreEqual(SlotZeroY, pose.Position.y, 0.05f, "a car never turns back to a slot it has driven past");
        }

        [Test]
        public void TheCarNeverJumpsWhenItsRouteChanges()
        {
            var trip = _router.BeginEntry(2);
            var last = Vector2.zero;
            var maxJump = 0f;
            Run(trip, p =>
            {
                if (p.Phase == TripPhase.OnRoute) _slots.Answer = 0;
                if (last != Vector2.zero) maxJump = Mathf.Max(maxJump, Vector2.Distance(last, p.Position));
                last = p.Position;
            });
            Assert.Less(maxJump, 0.5f);
        }

        [Test]
        public void ACarThatWasCancelledLeavesNothingBehind()
        {
            var trip = _router.BeginEntry(1);
            _router.Step(trip, Dt, out _);
            _router.Cancel(trip);
            Assert.IsFalse(_router.Step(trip, Dt, out _));
            Assert.AreNotEqual(TripId.None, _router.BeginEntry(0), "the pooled trip is free again");
        }
    }
}
