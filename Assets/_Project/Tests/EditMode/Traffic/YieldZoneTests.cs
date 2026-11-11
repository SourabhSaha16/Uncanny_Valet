using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Lot;
using UncannyValet.Traffic;
using UnityEngine;

namespace UncannyValet.Tests.Traffic
{
    /// <summary>The virtual leader that makes exit-road traffic slow down for a delivered car that wants to merge.</summary>
    public class YieldZoneTests
    {
        private const float Dt = 0.02f;
        private const float CarLength = 2.2f;
        private const float CarWidth = 1.1f;
        private const float AmbientSpeed = 6.25f;
        private const float RunSeconds = 40f;

        private readonly FollowSettings _follow = new FollowSettings(5f, 0.6f, 0.8f, 3f, 6f);

        [Test]
        public void InactiveZone_NeverTouchesTraffic()
        {
            var zone = new YieldZone(2.5f);
            Assert.IsFalse(zone.TryGap(TrafficLane.Top, 0f, CarLength, 5f, _follow, Dt, out float gap));
            Assert.IsTrue(float.IsPositiveInfinity(gap));
        }

        [Test]
        public void OnlyTheExitLaneIsAffected()
        {
            var zone = new YieldZone(2.5f);
            zone.Request(1, 20f);
            Assert.IsFalse(zone.TryGap(TrafficLane.East, 0f, CarLength, 5f, _follow, Dt, out _));
            Assert.IsFalse(zone.TryGap(TrafficLane.TopBack, 0f, CarLength, 5f, _follow, Dt, out _));
            Assert.IsTrue(zone.TryGap(TrafficLane.Top, 0f, CarLength, 5f, _follow, Dt, out _));
        }

        [Test]
        public void TheLineStandsTheMergeGapBehindTheMergePoint()
        {
            var zone = new YieldZone(2.5f);
            zone.Request(1, 20f);
            zone.TryGap(TrafficLane.Top, 10f, CarLength, 0f, _follow, Dt, out float gap);
            Assert.AreEqual(17.5f - 10f - CarLength, gap, 1e-4f);
        }

        [Test]
        public void ACarTooFastToStop_CarriesOn()
        {
            var zone = new YieldZone(2.5f);
            zone.Request(1, 20f);
            Assert.IsFalse(zone.TryGap(TrafficLane.Top, 13.5f, CarLength, 9f, _follow, Dt, out _));
        }

        [Test]
        public void ACarPastTheLine_CarriesOn()
        {
            var zone = new YieldZone(2.5f);
            zone.Request(1, 20f);
            Assert.IsFalse(zone.TryGap(TrafficLane.Top, 18f, CarLength, 0f, _follow, Dt, out _));
        }

        [Test]
        public void OneOwnerAtATime_AndReleaseFreesIt()
        {
            var zone = new YieldZone(2.5f);
            Assert.IsTrue(zone.Request(1, 20f));
            Assert.IsFalse(zone.Request(2, 20f));
            zone.Release(2);
            Assert.IsTrue(zone.Active, "a stranger cannot release it");
            zone.Release(1);
            Assert.IsFalse(zone.Active);
            Assert.IsTrue(zone.Request(2, 20f));
        }

        [Test]
        public void ACarBrakingForTheZone_StopsBeforeTheLine()
        {
            var zone = new YieldZone(2.5f);
            var occupancy = new LaneOccupancy(4);
            zone.Request(1, 40f);
            var car = new LaneState { Id = occupancy.Add(TrafficLane.Top, 0f, AmbientSpeed, CarLength), Lane = TrafficLane.Top, Speed = AmbientSpeed };
            for (float t = 0f; t < 20f; t += Dt)
            {
                LaneMotion.Step(ref car, CarLength, AmbientSpeed, float.PositiveInfinity, occupancy, _follow, Dt, zone);
                Assert.LessOrEqual(car.Distance + CarLength * 0.5f, 37.5f - CarLength * 0.5f + 1e-3f, "front bumper stays behind the virtual car");
            }
            Assert.Less(car.Speed, 0.05f);
        }

        // Ambient cars stream along the exit lane (one every SpawnEvery seconds) while one delivered car leaves the curb.
        private sealed class Scene
        {
            public readonly List<LaneState> Ambient = new List<LaneState>();
            public float JoinedAt = float.PositiveInfinity;
            public bool Overlapped;
        }

        private Scene Run(float spawnEvery, bool trafficYields)
        {
            var lot = ScriptableObject.CreateInstance<LotConfig>();
            var road = new RoadLayout(new LotGeometry(lot), lot.Site, Vector2.zero);
            var top = new LanePath();
            top.Rebuild(road.TopLane);
            var east = new LanePath();
            east.Rebuild(road.EastLane);
            var occupancy = new LaneOccupancy(32);
            var merge = new MergeGapRule(2.5f, 2.5f, 0.8f);
            var zone = new YieldZone(2.5f);
            var tuning = new TripTuning(CarLength, 2f, 2.5f, 2f, 0.02f);
            var router = new TripRouter(new TripContext(road, east, top, occupancy, _follow, merge, tuning, new TripSpeeds(5f, 2.5f), zone), 2);
            var scene = new Scene();
            int trip = router.BeginExit();
            float startAt = Mathf.Max(0f, road.CurbMergeDistance - 30f);
            float nextSpawn = 0f;
            for (float t = 0f; t < RunSeconds; t += Dt)
            {
                if (t >= nextSpawn && SpawnGate.IsOpen(occupancy, merge, TrafficLane.Top, startAt, CarLength))
                {
                    scene.Ambient.Add(new LaneState { Id = occupancy.Add(TrafficLane.Top, startAt, AmbientSpeed, CarLength), Lane = TrafficLane.Top, Distance = startAt, Speed = AmbientSpeed });
                    nextSpawn = t + spawnEvery;
                }
                bool running = router.Step(trip, Dt, out TripPose pose);
                if (running && pose.Phase == TripPhase.OnLane && float.IsPositiveInfinity(scene.JoinedAt)) scene.JoinedAt = t;
                StepAmbient(scene, occupancy, trafficYields ? zone : null, top, running ? pose.Position : (Vector2?)null);
                if (!running) break;
            }
            Object.DestroyImmediate(lot);
            return scene;
        }

        private void StepAmbient(Scene scene, LaneOccupancy occupancy, YieldZone zone, LanePath top, Vector2? patron)
        {
            for (int i = 0; i < scene.Ambient.Count; i++)
            {
                LaneState car = scene.Ambient[i];
                LaneMotion.Step(ref car, CarLength, AmbientSpeed, float.PositiveInfinity, occupancy, _follow, Dt, zone);
                scene.Ambient[i] = car;
                if (!patron.HasValue) continue;
                Vector2 at = top.PositionAt(car.Distance);
                if (Mathf.Abs(at.x - patron.Value.x) < CarLength && Mathf.Abs(at.y - patron.Value.y) < CarWidth) scene.Overlapped = true;
            }
        }

        [Test]
        public void DenseTraffic_WithYielding_TheLeavingCarMergesPromptlyAndNeverOverlaps()
        {
            Scene scene = Run(1.5f, true);
            Assert.IsFalse(scene.Overlapped, "an ambient car overlapped the leaving car");
            Assert.Less(scene.JoinedAt, 12f, "the leaving car waited too long for a gap");
        }

        [Test]
        public void DenseTraffic_WithoutYielding_TheLeavingCarWouldWait()
        {
            Scene scene = Run(1.5f, false);
            Assert.Greater(scene.JoinedAt, 12f, "the stream is dense enough that no natural gap opens");
        }

        [Test]
        public void SparseTraffic_WithYielding_NeverOverlaps()
        {
            for (float every = 2f; every < 9f; every += 1.1f)
            {
                Scene scene = Run(every, true);
                Assert.IsFalse(scene.Overlapped, $"spawn every {every}s");
                Assert.Less(scene.JoinedAt, 15f, $"spawn every {every}s");
            }
        }
    }
}
