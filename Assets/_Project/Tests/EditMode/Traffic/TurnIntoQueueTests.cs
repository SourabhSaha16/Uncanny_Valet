using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Lot;
using UncannyValet.Traffic;
using UnityEngine;

namespace UncannyValet.Tests.Traffic
{
    /// <summary>Regression for issue #3: ambient cars must never drive through a patron car turning into the queue (real road geometry).</summary>
    public class TurnIntoQueueTests
    {
        private const float Dt = 0.02f;
        private const float CarLength = 2.2f;
        private const float CarWidth = 1.1f;
        private const float AmbientSpeed = 6.25f;
        private const int MaxSteps = 3000;

        private LotConfig _lot;
        private RoadLayout _road;
        private LanePath _east;
        private LanePath _top;
        private LaneOccupancy _occupancy;
        private TripRouter _router;
        private FollowSettings _follow;

        [SetUp]
        public void SetUp()
        {
            _lot = ScriptableObject.CreateInstance<LotConfig>();
            _road = new RoadLayout(new LotGeometry(_lot), _lot.Site, Vector2.zero);
            _east = new LanePath();
            _east.Rebuild(_road.EastLane);
            _top = new LanePath();
            _top.Rebuild(_road.TopLane);
            _occupancy = new LaneOccupancy(16);
            _follow = new FollowSettings(5f, 0.6f, 0.8f, 3f, 6f);
            var merge = new MergeGapRule(2.5f, 2.5f, 0.8f);
            var tuning = new TripTuning(CarLength, 2f, 2.5f, 2f, 0.02f);
            _router = new TripRouter(new TripContext(_road, _east, _top, _occupancy, _follow, merge, tuning, new TripSpeeds(5f, 2.5f)), 4);
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_lot);

        private static bool Overlaps(Vector2 a, Vector2 b) =>
            Mathf.Abs(a.x - b.x) < CarLength && Mathf.Abs(a.y - b.y) < CarWidth;

        // Runs an arriving patron car with an ambient car released a few units behind it; returns the closest approach.
        private float Run(float startBehind, out bool finished)
        {
            int trip = _router.BeginEntry(0);
            LaneState ambient = default;
            float closest = float.PositiveInfinity;
            finished = false;
            for (int i = 0; i < MaxSteps && !finished; i++)
            {
                _router.Step(trip, Dt, out TripPose pose);
                float lead = pose.Position.x - _road.EastLane[0].x;
                if (ambient.Id == 0 && pose.Phase == TripPhase.OnLane && lead > startBehind + 1f)
                {
                    float d = lead - startBehind;
                    ambient = new LaneState { Id = _occupancy.Add(TrafficLane.East, d, AmbientSpeed, CarLength), Lane = TrafficLane.East, Distance = d, Speed = AmbientSpeed };
                }
                if (ambient.Id != 0) LaneMotion.Step(ref ambient, CarLength, AmbientSpeed, float.PositiveInfinity, _occupancy, _follow, Dt);
                finished = pose.Phase == TripPhase.Finished;
                if (ambient.Id == 0 || finished) continue;
                Vector2 ambientPos = _east.PositionAt(ambient.Distance);
                Assert.IsFalse(Overlaps(ambientPos, pose.Position), $"ambient at {ambientPos} overlaps patron car at {pose.Position} ({pose.Phase}, step {i})");
                closest = Mathf.Min(closest, Mathf.Abs(ambientPos.x - pose.Position.x));
            }
            return closest;
        }

        [Test]
        public void AmbientCarArrivingWhileACarLeavesTheCurb_NeverOverlapsIt()
        {
            float mergeX = _road.TopLane[0].x + _road.CurbMergeDistance;
            for (float offset = 0f; offset < 14f; offset += 1f)
            {
                SetUp();
                int trip = _router.BeginExit();
                float start = Mathf.Max(0f, _road.CurbMergeDistance - offset - 10f);
                int id = _occupancy.Add(TrafficLane.Top, start, AmbientSpeed, CarLength);
                var ambient = new LaneState { Id = id, Lane = TrafficLane.Top, Distance = start, Speed = AmbientSpeed };
                for (int i = 0; i < MaxSteps; i++)
                {
                    bool running = _router.Step(trip, Dt, out TripPose pose);
                    LaneMotion.Step(ref ambient, CarLength, AmbientSpeed, float.PositiveInfinity, _occupancy, _follow, Dt);
                    if (!running || pose.Phase == TripPhase.Finished || ambient.Distance > mergeX + 30f) break;
                    Vector2 ambientPos = _top.PositionAt(ambient.Distance);
                    Assert.IsFalse(Overlaps(ambientPos, pose.Position), $"offset {offset}: ambient {ambientPos} overlaps leaving car {pose.Position} ({pose.Phase}, step {i})");
                }
                TearDown();
            }
        }

        [Test]
        public void AmbientCarBehindATurningPatronCar_NeverOverlapsIt()
        {
            Run(4f, out bool finished);
            Assert.IsTrue(finished);
        }

        [Test]
        public void AmbientCarRightBehindATurningPatronCar_NeverOverlapsIt()
        {
            Run(CarLength + 0.8f, out bool finished);
            Assert.IsTrue(finished);
        }
    }
}
