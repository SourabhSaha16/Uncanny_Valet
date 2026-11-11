using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Traffic;
using UnityEngine;

namespace UncannyValet.Tests.Traffic
{
    public class TripObstacleTests
    {
        private const float Dt = 0.02f;
        private const float Creep = 0.8f;
        private const float Timeout = 6f;
        private static readonly FollowSettings Follow = new FollowSettings(5f, 0.6f, 0.8f, 3f, 6f);

        private static float Cap(TripObstacle o, float dt = Dt) => o.SpeedCap(Follow, Creep, Timeout, dt);

        [Test]
        public void ClearRoad_IsUncapped()
        {
            var o = new TripObstacle();
            Assert.IsTrue(float.IsPositiveInfinity(Cap(o)));
        }

        [Test]
        public void ObstacleInsideTheMinimumGap_StopsTheCar()
        {
            var o = new TripObstacle();
            o.Report(0.3f);
            Assert.LessOrEqual(Cap(o), TripObstacle.StoppedSpeed);
        }

        [Test]
        public void FarObstacle_AllowsSpeedThatCanStillStop()
        {
            var o = new TripObstacle();
            o.Report(6f);
            float cap = Cap(o);
            Assert.Greater(cap, 1f);
            Assert.LessOrEqual(cap * cap / (2f * Follow.Brake), 6f - Follow.MinGap + 0.01f, "can brake to a stop before the minimum gap");
        }

        [Test]
        public void BlockedLongerThanTheTimeout_Creeps()
        {
            var o = new TripObstacle();
            o.Report(0.3f);
            float cap = 0f;
            for (float t = 0f; t < Timeout - 0.1f; t += Dt) cap = Cap(o);
            Assert.LessOrEqual(cap, TripObstacle.StoppedSpeed, "still waiting before the timeout");
            for (float t = 0f; t < 0.5f; t += Dt) cap = Cap(o);
            Assert.AreEqual(Creep, cap, 1e-4f);
        }

        [Test]
        public void ClearingTheWay_ResetsTheWait()
        {
            var o = new TripObstacle();
            o.Report(0.3f);
            for (float t = 0f; t < Timeout + 1f; t += Dt) Cap(o);
            o.Report(float.PositiveInfinity);
            Cap(o);
            Assert.AreEqual(0f, o.BlockedFor);
            o.Report(0.3f);
            Assert.LessOrEqual(Cap(o), TripObstacle.StoppedSpeed);
        }

        private sealed class Road : IRoadLayout
        {
            public IReadOnlyList<Vector2> EastLane { get; } = new[] { new Vector2(-10f, 0f), new Vector2(30f, 0f) };
            public IReadOnlyList<Vector2> TopLane { get; } = new[] { new Vector2(-10f, 10f), new Vector2(30f, 10f) };
            public float QueueTurnDistance => 20f;
            public float CurbMergeDistance => 20f;
            public void QueueEntryRoute(int slot, List<Vector2> into) { into.Clear(); into.Add(new Vector2(10f, 0f)); into.Add(new Vector2(10f, 5f)); }
            public void CurbExitRoute(List<Vector2> into) { into.Clear(); into.Add(new Vector2(10f, 5f)); into.Add(new Vector2(10f, 10f)); }
        }

        private static TripRouter MakeRouter(TripTuning tuning)
        {
            var road = new Road();
            var east = new LanePath();
            east.Rebuild(road.EastLane);
            var top = new LanePath();
            top.Rebuild(road.TopLane);
            var merge = new MergeGapRule(2.5f, 2.5f, 0.8f);
            return new TripRouter(new TripContext(road, east, top, new LaneOccupancy(16), Follow, merge, tuning, new TripSpeeds(5f, 2.5f)), 2);
        }

        [Test]
        public void BlockedEntryTrip_HoldsThenResumesWhenCleared_AndStillFinishes()
        {
            var router = MakeRouter(new TripTuning(2f, 2f, 2.5f, 2f, 0.02f));
            int trip = router.BeginEntry(0);
            TripPose pose = default;
            for (int i = 0; i < 300; i++) router.Step(trip, Dt, out pose);
            Vector2 held = pose.Position;
            for (int i = 0; i < 100; i++)
            {
                router.SetObstacleGap(trip, 0.2f);
                router.Step(trip, Dt, out pose);
            }
            Assert.Less(Vector2.Distance(held, pose.Position), 1.5f, "braked to a stop instead of driving on");
            Assert.AreEqual(0f, pose.Speed, 0.01f);
            for (int i = 0; i < 6000 && pose.Phase != TripPhase.Finished; i++)
            {
                router.SetObstacleGap(trip, float.PositiveInfinity);
                router.Step(trip, Dt, out pose);
            }
            Assert.AreEqual(TripPhase.Finished, pose.Phase);
        }

        [Test]
        public void PermanentlyBlockedTrip_StillArrivesAfterTheTimeout()
        {
            var router = MakeRouter(new TripTuning(2f, 2f, 2.5f, 2f, 0.02f, 0.2f, 2f, 1.5f));
            int trip = router.BeginEntry(0);
            TripPose pose = default;
            for (int i = 0; i < 20000 && pose.Phase != TripPhase.Finished; i++)
            {
                router.SetObstacleGap(trip, 0.2f);
                router.Step(trip, Dt, out pose);
            }
            Assert.AreEqual(TripPhase.Finished, pose.Phase);
        }

        [Test]
        public void BlockedExitTrip_StopsOnTheRoute()
        {
            var router = MakeRouter(new TripTuning(2f, 2f, 2.5f, 2f, 0.02f));
            int trip = router.BeginExit();
            TripPose pose = default;
            for (int i = 0; i < 200; i++)
            {
                router.SetObstacleGap(trip, 0.2f);
                router.Step(trip, Dt, out pose);
            }
            Assert.AreEqual(0f, pose.Speed, 0.01f);
            Assert.AreEqual(TripPhase.OnRoute, pose.Phase);
        }
    }
}
