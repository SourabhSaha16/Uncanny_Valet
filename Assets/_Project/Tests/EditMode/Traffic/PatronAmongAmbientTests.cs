using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Lot;
using UncannyValet.Traffic;
using UnityEngine;

namespace UncannyValet.Tests.Traffic
{
    /// <summary>Patron cars driving in and out among full two-way ambient traffic: no overlaps, and the delivered car gets out promptly.</summary>
    public class PatronAmongAmbientTests
    {
        private const float Dt = 0.02f;
        private const float CarLength = 2.2f;
        private const float CarWidth = 1.1f;
        private const float TripSeconds = 40f;
        private const float MaxExitWait = 15f;

        private LotConfig _lot;
        private LotGeometry _geo;
        private TrafficConfig _config;
        private AmbientTraffic _ambient;
        private TripRouter _router;
        private ViewWindow _view;

        [SetUp]
        public void SetUp()
        {
            _lot = ScriptableObject.CreateInstance<LotConfig>();
            _geo = new LotGeometry(_lot);
            var road = new RoadLayout(_geo, _lot.Site, Vector2.zero);
            _config = ScriptableObject.CreateInstance<TrafficConfig>();
            var lanes = new LaneNetwork();
            lanes.Rebuild(road);
            var occupancy = new LaneOccupancy(_config.MaxCars * 3);
            var follow = new FollowSettings(_config.CruiseSpeed, _config.MinGap, _config.Headway, _config.Acceleration, _config.Brake);
            var merge = new MergeGapRule(_config.MergeGapAhead, _config.MergeGapBehind, _config.Headway);
            var zone = new YieldZone(_config.MergeGapBehind);
            var density = new AmbientDensity(_config.Seed, 1, _config.SpeedScaleRange, _config.SpawnGapSeconds, 6, 8);
            _ambient = new AmbientTraffic(_config, lanes, occupancy, null, follow, merge, zone, density);
            var tuning = new TripTuning(CarLength, 2f, 2.5f, 2f, 0.02f);
            var context = new TripContext(road, lanes.Path(TrafficLane.East), lanes.Path(TrafficLane.Top), occupancy, follow, merge, tuning, new TripSpeeds(5f, 2.5f), zone);
            _router = new TripRouter(context, 2);
            _view = new ViewWindow(Rect.MinMaxRect(_geo.Bounds.xMin, _geo.EastBackLaneY - 1.2f, _geo.Bounds.xMax, _geo.TopBackLaneY + 1.2f));
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_config);
            Object.DestroyImmediate(_lot);
        }

        private bool OverlapsAnyAmbient(Vector2 patron)
        {
            foreach (AmbientVehicle v in _ambient.Vehicles)
            {
                if (v.Active && Mathf.Abs(v.Position.x - patron.x) < CarLength && Mathf.Abs(v.Position.y - patron.y) < CarWidth) return true;
            }
            return false;
        }

        // Runs one trip to its end; returns the seconds until it reached the lane (exit) or its slot (entry), or -1 if it never did.
        private float RunTrip(int trip, TripPhase target)
        {
            float reached = -1f;
            for (float t = 0f; t < TripSeconds; t += Dt)
            {
                _ambient.Step(Dt, _view);
                if (!_router.Step(trip, Dt, out TripPose pose)) break;
                Assert.IsFalse(OverlapsAnyAmbient(pose.Position), $"patron car overlapped an ambient car at {pose.Position} ({pose.Phase}, {t}s)");
                if (reached < 0f && pose.Phase == target) reached = t;
            }
            return reached;
        }

        [Test]
        public void ArrivingCarsDriveInAmongTheTraffic_WithoutOverlap()
        {
            for (int i = 0; i < 4; i++)
            {
                for (float t = 0f; t < 6f; t += Dt) _ambient.Step(Dt, _view);
                int trip = _router.BeginEntry(i % 3);
                Assert.GreaterOrEqual(RunTrip(trip, TripPhase.Finished), 0f, $"entry {i} never finished");
                _router.Cancel(trip);
            }
        }

        [Test]
        public void DeliveredCarsMergeAmongTheTraffic_PromptlyAndWithoutOverlap()
        {
            for (int i = 0; i < 6; i++)
            {
                for (float t = 0f; t < 4f + i; t += Dt) _ambient.Step(Dt, _view);
                int trip = _router.BeginExit();
                float joined = RunTrip(trip, TripPhase.OnLane);
                Assert.That(joined, Is.InRange(0f, MaxExitWait), $"exit {i}");
                _router.Cancel(trip);
            }
        }
    }
}
