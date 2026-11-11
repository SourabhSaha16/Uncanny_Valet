using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Lot;
using UncannyValet.Traffic;
using UnityEngine;

namespace UncannyValet.Tests.Traffic
{
    /// <summary>The whole ambient system on the real road geometry, without visuals: density, directions, spawning and despawning off screen, no overlaps.</summary>
    public class AmbientTrafficTests
    {
        private const float Dt = 0.02f;
        private const float CarLength = 2.2f;
        private const float WarmUpSeconds = 3f;

        private LotConfig _lot;
        private LotGeometry _geo;
        private RoadLayout _road;
        private TrafficConfig _config;
        private LaneNetwork _lanes;
        private LaneOccupancy _occupancy;
        private AmbientTraffic _ambient;

        [SetUp]
        public void SetUp()
        {
            _lot = ScriptableObject.CreateInstance<LotConfig>();
            _geo = new LotGeometry(_lot);
            _road = new RoadLayout(_geo, _lot.Site, Vector2.zero);
            _config = ScriptableObject.CreateInstance<TrafficConfig>();
            _lanes = new LaneNetwork();
            _lanes.Rebuild(_road);
            _occupancy = new LaneOccupancy(_config.MaxCars * 3);
            var follow = new FollowSettings(_config.CruiseSpeed, _config.MinGap, _config.Headway, _config.Acceleration, _config.Brake);
            var merge = new MergeGapRule(_config.MergeGapAhead, _config.MergeGapBehind, _config.Headway);
            var density = new AmbientDensity(_config.Seed, 1, _config.SpeedScaleRange, _config.SpawnGapSeconds, _config.MinOnScreen, _config.TargetOnScreen);
            _ambient = new AmbientTraffic(_config, _lanes, _occupancy, null, follow, merge, new YieldZone(_config.MergeGapBehind), density);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_config);
            Object.DestroyImmediate(_lot);
        }

        // What the camera shows: the lot's width and height down to the entry road (the exit road above is out of view).
        private ViewWindow EntryRoadView() =>
            new ViewWindow(Rect.MinMaxRect(_geo.Bounds.xMin, _geo.EastBackLaneY - 1.2f, _geo.Bounds.xMax, _geo.Bounds.yMax));

        private ViewWindow AllRoadsView() =>
            new ViewWindow(Rect.MinMaxRect(_geo.Bounds.xMin, _geo.EastBackLaneY - 1.2f, _geo.Bounds.xMax, _geo.TopBackLaneY + 1.2f));

        [Test]
        public void TheRoadsAreBusyFromTheFirstFrame()
        {
            _ambient.Step(Dt, EntryRoadView());
            Assert.That(_ambient.OnScreen, Is.InRange(_config.MinOnScreen, _config.TargetOnScreen));
        }

        [Test]
        public void ThreeToFourCarsStayOnScreenForTenMinutes()
        {
            ViewWindow view = EntryRoadView();
            int frames = 0;
            int under = 0;
            for (float t = 0f; t < 600f; t += Dt)
            {
                _ambient.Step(Dt, view);
                Assert.LessOrEqual(_ambient.OnScreen, _config.TargetOnScreen + 1, $"overfilled at {t}s");
                if (t < WarmUpSeconds) continue;
                frames++;
                if (_ambient.OnScreen < _config.MinOnScreen) under++;
            }
            Assert.Less(under / (float)frames, 0.01f, "fewer than the minimum on screen for more than 1% of the time");
        }

        [Test]
        public void CarsDriveBothWaysOnBothVisibleRoads()
        {
            ViewWindow view = AllRoadsView();
            var seen = new HashSet<TrafficLane>();
            for (float t = 0f; t < 240f; t += Dt)
            {
                _ambient.Step(Dt, view);
                foreach (AmbientVehicle v in _ambient.Vehicles)
                {
                    if (v.Active) seen.Add(v.State.Lane);
                }
            }
            CollectionAssert.AreEquivalent(new[] { TrafficLane.East, TrafficLane.Top, TrafficLane.EastBack, TrafficLane.TopBack }, seen);
        }

        [Test]
        public void OppositeLaneCars_DriveRightToLeft_AndTheOthersLeftToRight()
        {
            ViewWindow view = AllRoadsView();
            var last = new Dictionary<int, float>();
            for (float t = 0f; t < 120f; t += Dt)
            {
                _ambient.Step(Dt, view);
                foreach (AmbientVehicle v in _ambient.Vehicles)
                {
                    if (!v.Active) continue;
                    if (last.TryGetValue(v.State.Id, out float x))
                    {
                        float moved = v.Position.x - x;
                        if (TrafficLanes.IsLeftward(v.State.Lane)) Assert.LessOrEqual(moved, 1e-4f, v.State.Lane.ToString());
                        else Assert.GreaterOrEqual(moved, -1e-4f, v.State.Lane.ToString());
                    }
                    last[v.State.Id] = v.Position.x;
                }
            }
        }

        [Test]
        public void LanesWhoseHeightIsOutOfViewGetNoCars()
        {
            ViewWindow view = EntryRoadView();
            for (float t = 0f; t < 120f; t += Dt)
            {
                _ambient.Step(Dt, view);
                foreach (AmbientVehicle v in _ambient.Vehicles)
                {
                    Assert.IsFalse(v.Active && (v.State.Lane == TrafficLane.Top || v.State.Lane == TrafficLane.TopBack), "a car on the exit road while it is out of view");
                }
            }
        }

        [Test]
        public void NewCarsAppearOffScreen_AndLeaveWhenOffScreen()
        {
            ViewWindow view = EntryRoadView();
            var known = new HashSet<int>();
            _ambient.Step(Dt, view);
            foreach (AmbientVehicle v in _ambient.Vehicles) if (v.Active) known.Add(v.State.Id);
            for (float t = 0f; t < 240f; t += Dt)
            {
                _ambient.Step(Dt, view);
                foreach (AmbientVehicle v in _ambient.Vehicles)
                {
                    if (!v.Active) continue;
                    if (known.Add(v.State.Id))
                        Assert.IsFalse(view.Contains(v.Position, CarLength * 0.5f - 0.05f), $"a car popped in at {v.Position}");
                    Assert.IsTrue(view.Contains(v.Position, _config.DespawnMargin + 2f), $"a car lingered off screen at {v.Position}");
                }
            }
        }

        [Test]
        public void NoTwoCarsOnALaneEverOverlap()
        {
            ViewWindow view = AllRoadsView();
            for (float t = 0f; t < 300f; t += Dt)
            {
                _ambient.Step(Dt, view);
                foreach (AmbientVehicle a in _ambient.Vehicles)
                {
                    if (!a.Active) continue;
                    foreach (AmbientVehicle b in _ambient.Vehicles)
                    {
                        if (!b.Active || a == b || a.State.Lane != b.State.Lane) continue;
                        Assert.GreaterOrEqual(Mathf.Abs(a.State.Distance - b.State.Distance), CarLength, $"overlap on {a.State.Lane} at {t}s");
                    }
                }
            }
        }

        [Test]
        public void TargetZero_SwitchesAmbientTrafficOff()
        {
            var density = new AmbientDensity(1, 1, _config.SpeedScaleRange, _config.SpawnGapSeconds, 0, 0);
            var follow = new FollowSettings(5f, 0.6f, 0.8f, 3f, 6f);
            var off = new AmbientTraffic(_config, _lanes, new LaneOccupancy(8), null, follow, new MergeGapRule(2.5f, 2.5f, 0.8f), new YieldZone(2.5f), density);
            for (int i = 0; i < 500; i++) off.Step(Dt, EntryRoadView());
            Assert.AreEqual(0, off.Alive);
        }
    }
}
