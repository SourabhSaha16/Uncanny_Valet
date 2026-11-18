using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Lot.Handmade;
using UncannyValet.UI;
using UncannyValet.Valet;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace UncannyValet.Tests.PlayMode
{
    /// <summary>
    /// One patron through the DEV hand-made Midgard lot (#194), on the real bootstrap: the car drives in along the authored entry road, the valet "takes" it (the car is
    /// put into a bay and later at the curb, as a robot valet would), the patron walks to the door and back to the curb, the car is delivered and leaves along the authored exit road.
    /// </summary>
    public sealed class HandmadeShiftPlayModeTests
    {
        private const string ScenePath = "Assets/_Project/Scenes/Lot_MidgardHandmade.unity";
        private const float PhaseSeconds = 150f;
        private const float OnRoadTolerance = 1.2f;
        private const float AtPointTolerance = 2.5f;

        private readonly List<string> _problems = new List<string>();

        private void OnLog(string message, string stack, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) _problems.Add(type + ": " + message + "\n" + stack);
        }

        [UnityTearDown]
        public IEnumerator RemoveTheScene()
        {
            Application.logMessageReceived -= OnLog;
            var roots = SceneManager.GetActiveScene().GetRootGameObjects();
            for (var i = 0; i < roots.Length; i++) UnityEngine.Object.Destroy(roots[i]);
            var host = UnityEngine.Object.FindFirstObjectByType<UncannyValet.Run.RunHost>();
            if (host != null) UnityEngine.Object.Destroy(host.gameObject);
            yield return null;
            Services.Reset();
        }

        [UnityTest]
        public IEnumerator Patron_ArrivesParks_IsPickedUp_AndLeavesAlongTheAuthoredRoads()
        {
            Application.logMessageReceived += OnLog;
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            yield return null;
            var lot = UnityEngine.Object.FindFirstObjectByType<HandmadeLot>();
            Assert.IsTrue(lot.IsBuilt, "the hand-made lot built");
            var data = HandmadeLotReader.Read(lot);
            var bus = Services.Get<IEventBus>();
            var seen = new Seen(bus);
            var ui = UnityEngine.Object.FindFirstObjectByType<GameUi>();
            typeof(GameUi).GetMethod("StartPlaying", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(ui, null);

            // 1. The patron's car drives in along the entry road and stops at the pickup slot.
            var onEntry = false;
            yield return WaitFor(() => { onEntry |= Near(Car(seen.Spawned), data.EntryRoads[0].Lane, OnRoadTolerance); return seen.Arrived.Count > 0; }, "the patron car to arrive");
            var id = seen.Arrived[0];
            Assert.IsTrue(onEntry, "the car drove along the authored entry road");
            Assert.Less(Vector2.Distance(Position(id), lot.ArrivalPoint), AtPointTolerance, "it stopped at queue slot 0");

            // 2. The valet comes to the car; the patron steps out and walks to the authored main door.
            var valet = UnityEngine.Object.FindFirstObjectByType<KeyboardInputSource>().transform;
            valet.position = lot.ArrivalPoint;
            yield return WaitFor(() => seen.SteppedOut.Contains(id), "the patron to step out");
            var atDoor = false;
            data.TryGetDoor(DoorKind.Main, out var door);
            yield return WaitFor(() => { atDoor |= Vector2.Distance(WalkerNear(door.Position), door.Position) < AtPointTolerance; return seen.InDiner.Contains(id); }, "the patron to reach the door");
            Assert.IsTrue(atDoor, "a pedestrian stood at the main door");

            // 3. The car goes into a bay.
            Assert.IsTrue(Services.Get<ISpotGeometry>().TryGetSpot(lot.SpotIds[0], out var bay));
            Drivable(id).SetDriver(true); // the car publishes CarEntered and the seat takes the valet
            Place(id, bay.Center, bay.RotationDegrees);
            Drivable(id).SetDriver(false); // CarExited: nobody in the parked car
            yield return WaitFor(() => seen.Parked.Contains(id), "the car to count as parked");

            // 4. The patron asks for the car and walks to the authored curb.
            yield return WaitFor(() => seen.PickupRequested.Contains(id), "the pickup request");
            yield return WaitFor(() => seen.AtCurb.Contains(id), "the patron to reach the curb");

            // 5. The car comes to the curb, is delivered, and drives out along the authored exit road.
            Drivable(id).SetDriver(true); // the car publishes CarEntered and the seat takes the valet
            Place(id, data.Curbs[0].Rect.Center, data.Curbs[0].Rect.RotationDegrees);
            Drivable(id).SetDriver(false); // CarExited: nobody in the parked car
            yield return WaitFor(() => seen.Delivered.Contains(id), "the delivery at the curb");
            var onExit = false;
            yield return WaitFor(() => { onExit |= Near(Car(id), data.ExitRoads[0].Lane, OnRoadTolerance); return seen.Departed.Contains(id) && float.IsNaN(Position(id).x); }, "the car to drive off and leave the lot");
            Assert.IsTrue(onExit, "the car drove along the authored exit road");

            seen.Dispose(bus);
            Application.logMessageReceived -= OnLog;
            Assert.IsEmpty(_problems, string.Join("\n---\n", _problems));
        }

        private static IEnumerator WaitFor(Func<bool> done, string what)
        {
            var until = Time.time + PhaseSeconds;
            while (!done())
            {
                if (Time.time > until) Assert.Fail("timed out waiting for " + what);
                yield return null;
            }
        }

        private static IDrivable Drivable(CarId id)
        {
            Services.Get<ICarFactory>().TryGet(id, out var car);
            return car;
        }

        private static Vector2 Position(CarId id) => Services.Get<ICarFactory>().TryGet(id, out var car) ? (Vector2)car.Transform.position : new Vector2(float.NaN, float.NaN);

        private static Vector2? Car(List<CarId> ids) => ids.Count == 0 ? (Vector2?)null : Position(ids[ids.Count - 1]);

        private static Vector2? Car(CarId id) => Position(id);

        private static bool Near(Vector2? point, Vector2[] line, float tolerance)
            => point.HasValue && !float.IsNaN(point.Value.x) && RouteMath.DistanceToPolyline(point.Value, line) < tolerance;

        // The walker (any pedestrian object) nearest to the point; the pool objects are named "Pedestrian..." and "Patron...".
        private static Vector2 WalkerNear(Vector2 point)
        {
            var best = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
            foreach (var walker in UnityEngine.Object.FindObjectsByType<UncannyValet.Pedestrians.PedestrianWalker>(FindObjectsSortMode.None))
                if (Vector2.Distance(walker.transform.position, point) < Vector2.Distance(best, point)) best = walker.transform.position;
            return best;
        }

        private static void Place(CarId id, Vector2 at, float rotation)
        {
            Services.Get<ICarFactory>().TryGet(id, out var car);
            var body = car.Transform.GetComponent<Rigidbody2D>();
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
            body.position = at;
            body.rotation = rotation;
            car.Transform.position = at;
        }

        private sealed class Seen
        {
            public readonly List<CarId> Spawned = new List<CarId>();
            public readonly List<CarId> Arrived = new List<CarId>();
            public readonly HashSet<CarId> SteppedOut = new HashSet<CarId>();
            public readonly HashSet<CarId> InDiner = new HashSet<CarId>();
            public readonly HashSet<CarId> Parked = new HashSet<CarId>();
            public readonly HashSet<CarId> PickupRequested = new HashSet<CarId>();
            public readonly HashSet<CarId> AtCurb = new HashSet<CarId>();
            public readonly HashSet<CarId> Delivered = new HashSet<CarId>();
            public readonly HashSet<CarId> Departed = new HashSet<CarId>();

            public Seen(IEventBus bus)
            {
                bus.Subscribe<CarSpawned>(OnSpawned);
                bus.Subscribe<CarArrived>(e => Arrived.Add(e.Car));
                bus.Subscribe<PatronStepsOut>(e => SteppedOut.Add(e.Car));
                bus.Subscribe<PatronEnteredDiner>(e => InDiner.Add(e.Car));
                bus.Subscribe<CarParked>(e => Parked.Add(e.Car));
                bus.Subscribe<PickupRequested>(e => PickupRequested.Add(e.Car));
                bus.Subscribe<PatronReachedCurb>(e => AtCurb.Add(e.Car));
                bus.Subscribe<CarDelivered>(e => Delivered.Add(e.Car));
                bus.Subscribe<CarDeparted>(e => Departed.Add(e.Car));
            }

            private void OnSpawned(CarSpawned e) => Spawned.Add(e.Car);

            public void Dispose(IEventBus bus) => bus.Unsubscribe<CarSpawned>(OnSpawned);
        }
    }
}
