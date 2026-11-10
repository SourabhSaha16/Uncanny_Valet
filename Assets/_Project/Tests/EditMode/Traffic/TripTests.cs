using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Traffic;
using UnityEngine;

namespace UncannyValet.Tests.Traffic
{
    public class TripTests
    {
        private const float Dt = 0.02f;
        private const float CarLength = 2f;
        private const int MaxSteps = 6000;

        private sealed class FakeRoad : IRoadLayout
        {
            public IReadOnlyList<Vector2> EastLane { get; } = new[] { new Vector2(-10f, 0f), new Vector2(30f, 0f) };
            public IReadOnlyList<Vector2> WestLane { get; } = new[] { new Vector2(30f, -2f), new Vector2(-10f, -2f) };
            public float QueueTurnDistance => 20f;
            public float CurbMergeDistance => 20f;

            public void QueueEntryRoute(int slot, List<Vector2> into)
            {
                into.Clear();
                into.Add(new Vector2(10f, 0f));
                into.Add(new Vector2(10f, 5f));
            }

            public void CurbExitRoute(List<Vector2> into)
            {
                into.Clear();
                into.Add(new Vector2(10f, 5f));
                into.Add(new Vector2(10f, 0f));
            }
        }

        private LaneOccupancy _occupancy;
        private TripRouter _router;
        private TripSpeeds _speeds;

        [SetUp]
        public void SetUp()
        {
            var road = new FakeRoad();
            var east = new LanePath();
            east.Rebuild(road.EastLane);
            _occupancy = new LaneOccupancy(16);
            var follow = new FollowSettings(5f, 0.6f, 0.8f, 3f, 6f);
            var merge = new MergeGapRule(2.5f, 2.5f, 0.8f);
            var tuning = new TripTuning(CarLength, 2f, 2.5f, 2f, 0.02f);
            _speeds = new TripSpeeds(5f, 2.5f);
            _router = new TripRouter(new TripContext(road, east, _occupancy, follow, merge, tuning, _speeds), 4);
        }

        private TripPose RunUntilFinished(int trip, out int steps)
        {
            TripPose pose = default;
            steps = 0;
            while (steps < MaxSteps && _router.Step(trip, Dt, out pose))
            {
                steps++;
                if (pose.Phase == TripPhase.Finished) return pose;
            }
            return pose;
        }

        [Test]
        public void Entry_OnAnEmptyRoad_StopsAtTheEndOfTheQueueRoute()
        {
            int trip = _router.BeginEntry(0);
            TripPose pose = RunUntilFinished(trip, out _);
            Assert.AreEqual(TripPhase.Finished, pose.Phase);
            Assert.AreEqual(new Vector2(10f, 5f).x, pose.Position.x, 0.05f);
            Assert.AreEqual(5f, pose.Position.y, 0.05f);
            Assert.AreEqual(0, _occupancy.Count);
        }

        [Test]
        public void Entry_WaitsOffScreenWhileTheLaneStartIsBlocked()
        {
            _occupancy.Add(TrafficLane.East, 1f, 0f, CarLength);
            int trip = _router.BeginEntry(0);
            for (int i = 0; i < 200; i++)
            {
                _router.Step(trip, Dt, out TripPose pose);
                Assert.AreEqual(TripPhase.Waiting, pose.Phase);
                Assert.AreEqual(-10f, pose.Position.x, 0.001f);
            }
        }

        [Test]
        public void Entry_SlowsToTurnSpeedBeforeTheTurn()
        {
            int trip = _router.BeginEntry(0);
            float speedAtTurn = float.MaxValue;
            for (int i = 0; i < MaxSteps; i++)
            {
                _router.Step(trip, Dt, out TripPose pose);
                if (pose.Phase == TripPhase.OnRoute) { speedAtTurn = pose.Speed; break; }
            }
            Assert.LessOrEqual(speedAtTurn, 2.2f);
        }

        [Test]
        public void Exit_OnAnEmptyRoad_MergesAndLeavesAtTheRightEnd()
        {
            int trip = _router.BeginExit();
            TripPose pose = RunUntilFinished(trip, out _);
            Assert.AreEqual(TripPhase.Finished, pose.Phase);
            Assert.AreEqual(30f, pose.Position.x, 0.05f);
            Assert.AreEqual(0, _occupancy.Count);
        }

        [Test]
        public void Exit_YieldsWhileTheMergePointIsBlocked_ThenGoes()
        {
            int blocker = _occupancy.Add(TrafficLane.East, 21f, 0f, CarLength);
            int trip = _router.BeginExit();
            TripPose pose = default;
            for (int i = 0; i < 1000; i++) _router.Step(trip, Dt, out pose);
            Assert.AreEqual(TripPhase.Yielding, pose.Phase);
            Assert.AreEqual(0f, pose.Speed, 0.001f);
            Assert.AreEqual(0f, pose.Position.y, 0.1f);

            _occupancy.Remove(blocker);
            Assert.AreEqual(TripPhase.Finished, RunUntilFinished(trip, out _).Phase);
        }

        [Test]
        public void Exit_DoesNotMergeIntoAFastCarClosingFromBehind()
        {
            _occupancy.Add(TrafficLane.East, 14f, 5f, CarLength);
            int trip = _router.BeginExit();
            bool mergedWhileBlocked = false;
            for (int i = 0; i < 400; i++)
            {
                _router.Step(trip, Dt, out TripPose pose);
                mergedWhileBlocked |= pose.Phase == TripPhase.OnLane;
            }
            Assert.IsFalse(mergedWhileBlocked);
        }

        [Test]
        public void Entry_ARaisedEntrySpeedGetsTheCarInSooner()
        {
            RunUntilFinished(_router.BeginEntry(0), out int normal);
            _speeds.SetEntrySpeed(10f);
            RunUntilFinished(_router.BeginEntry(0), out int fast);
            Assert.Less(fast, normal);
        }

        [Test]
        public void Entry_ALoweredEntrySpeedSlowsTheTurn()
        {
            _speeds.SetEntrySpeed(2.5f);
            int trip = _router.BeginEntry(0);
            float speedAtTurn = float.MaxValue;
            for (int i = 0; i < MaxSteps; i++)
            {
                _router.Step(trip, Dt, out TripPose pose);
                if (pose.Phase == TripPhase.OnRoute) { speedAtTurn = pose.Speed; break; }
            }
            Assert.LessOrEqual(speedAtTurn, 1.2f);
        }

        [Test]
        public void Exit_ARaisedExitSpeedGetsTheCarAwaySooner()
        {
            RunUntilFinished(_router.BeginExit(), out int normal);
            _speeds.SetExitSpeed(5f);
            RunUntilFinished(_router.BeginExit(), out int fast);
            Assert.Less(fast, normal);
        }

        [Test]
        public void TheExitSpeedDoesNotChangeEntryAndTheEntrySpeedDoesNotChangeExit()
        {
            RunUntilFinished(_router.BeginEntry(0), out int entryBefore);
            RunUntilFinished(_router.BeginExit(), out int exitBefore);
            _speeds.SetExitSpeed(10f);
            RunUntilFinished(_router.BeginEntry(0), out int entryAfter);
            _speeds.SetExitSpeed(2.5f);
            _speeds.SetEntrySpeed(10f);
            RunUntilFinished(_router.BeginExit(), out int exitAfter);
            Assert.AreEqual(entryBefore, entryAfter);
            Assert.AreEqual(exitBefore, exitAfter);
        }

        [Test]
        public void TripSpeedsScaleIsTheTunedSpeedOverTheConfigSpeed()
        {
            var speeds = new TripSpeeds(5f, 2.5f);
            Assert.AreEqual(1f, speeds.EntryScale);
            Assert.AreEqual(1f, speeds.ExitScale);
            speeds.SetEntrySpeed(10f);
            speeds.SetExitSpeed(1.25f);
            Assert.AreEqual(2f, speeds.EntryScale, 1e-4f);
            Assert.AreEqual(0.5f, speeds.ExitScale, 1e-4f);
            speeds.SetEntrySpeed(0f);
            Assert.AreEqual(TripSpeeds.MinScale, speeds.EntryScale, 1e-4f);
        }

        [Test]
        public void FinishedTrip_IsReleased()
        {
            int trip = _router.BeginExit();
            RunUntilFinished(trip, out _);
            Assert.IsFalse(_router.Step(trip, Dt, out _));
            Assert.AreNotEqual(TripId.None, _router.BeginExit());
        }

        [Test]
        public void Cancel_RemovesTheLaneRegistration()
        {
            int trip = _router.BeginEntry(0);
            for (int i = 0; i < 100; i++) _router.Step(trip, Dt, out _);
            Assert.AreEqual(1, _occupancy.Count);
            _router.Cancel(trip);
            Assert.AreEqual(0, _occupancy.Count);
        }

        [Test]
        public void ExhaustedPool_ReturnsNone()
        {
            for (int i = 0; i < 4; i++) Assert.AreNotEqual(TripId.None, _router.BeginEntry(0));
            Assert.AreEqual(TripId.None, _router.BeginEntry(0));
        }
    }
}
