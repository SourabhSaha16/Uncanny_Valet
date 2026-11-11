using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UncannyValet.Cars;
using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UnityEngine;
using UnityEngine.TestTools;

namespace UncannyValet.Tests.PlayMode
{
    /// <summary>Road trip cars are solid and yield: the player's car rebounds off one without moving it, and it stops behind an obstacle and resumes.</summary>
    public class TripCarSolidTests
    {
        private const float StartY = -14f;
        private const float StopGap = 0.5f;
        private const float BrakeGain = 4f;

        // A straight trip up the y axis that honours the reported obstacle gap like the real router does (speed capped by the gap).
        private sealed class StraightTrips : IEntryRouter, ITripDriver, ITripObstacles
        {
            public float Cruise = 4f;
            public float Y = StartY;
            private float _gap = float.PositiveInfinity;

            public int BeginEntry(int slot) => 1;
            public void SetObstacleGap(int tripId, float gap) => _gap = gap;
            public void Cancel(int tripId) { }

            public bool Step(int tripId, float dt, out TripPose pose)
            {
                float speed = Mathf.Min(Cruise, Mathf.Max(0f, _gap - StopGap) * BrakeGain);
                Y = Mathf.Min(0f, Y + speed * dt);
                pose = new TripPose(new Vector2(0f, Y), 0f, speed, Y >= 0f ? TripPhase.Finished : TripPhase.OnRoute);
                return true;
            }
        }

        private GameObject _factoryObject;
        private CarFactory _factory;
        private CarDefinition _definition;
        private StraightTrips _trips;
        private readonly List<CarsCollided> _bumps = new List<CarsCollided>();
        private readonly List<CarArrived> _arrivals = new List<CarArrived>();
        private IEventBus _bus;

        [SetUp]
        public void SetUp()
        {
            Services.Reset();
            _bus = new EventBus();
            Services.Register<IEventBus>(_bus);
            _bus.Subscribe<CarsCollided>(_bumps.Add);
            _bus.Subscribe<CarArrived>(_arrivals.Add);
            _trips = new StraightTrips();
            Services.Register<IEntryRouter>(_trips);
            Services.Register<ITripDriver>(_trips);
            Services.Register<ITripObstacles>(_trips);
            _definition = ScriptableObject.CreateInstance<CarDefinition>();
            _factoryObject = new GameObject("Cars");
            _factoryObject.SetActive(false);
            _factory = _factoryObject.AddComponent<CarFactory>();
            Set("_defaultDefinition", _definition);
            Set("_flash", ScriptableObject.CreateInstance<CarFlashConfig>());
            Set("_handling", ScriptableObject.CreateInstance<CarHandlingConfig>());
            _factoryObject.SetActive(true);
        }

        [TearDown]
        public void TearDown()
        {
            _bus.Unsubscribe<CarsCollided>(_bumps.Add);
            _bus.Unsubscribe<CarArrived>(_arrivals.Add);
            _bumps.Clear();
            _arrivals.Clear();
            foreach (var car in Object.FindObjectsByType<CarDriver>(FindObjectsSortMode.None)) Object.DestroyImmediate(car.gameObject);
            Object.DestroyImmediate(_factoryObject);
            Services.Reset();
        }

        [UnityTest]
        public IEnumerator DrivenCarPushedIntoATripCar_Rebounds_AndTheTripCarStaysOnItsRoute()
        {
            _trips.Cruise = 0f;
            var trip = (CarDriver)_factory.SpawnArriving(new CarId(1), Vector2.zero, 0f, 0);
            var tripBody = trip.GetComponent<Rigidbody2D>();
            Assert.IsTrue(trip.GetComponent<Collider2D>().enabled, "a trip car is solid");
            Assert.AreEqual(RigidbodyType2D.Kinematic, tripBody.bodyType);
            var driven = (CarDriver)_factory.Spawn(_definition, new CarId(2), new Vector2(0f, StartY + 3.2f), 180f, default, false);
            driven.SetDriver(true);
            var body = driven.GetComponent<Rigidbody2D>();
            var maxRebound = 0f;
            for (var t = 0f; t < 3f; t += Time.fixedDeltaTime)
            {
                yield return new WaitForFixedUpdate();
                driven.SetInput(_bumps.Count == 0 ? 1f : 0f, 0f);
                maxRebound = Mathf.Max(maxRebound, body.linearVelocity.y);
            }
            Assert.AreEqual(1, _bumps.Count, "one bump for the pair");
            Assert.Greater(maxRebound, 0.3f, "the driven car rebounds away from the trip car");
            Assert.AreEqual(StartY, tripBody.position.y, 0.001f, "the trip car is not shoved off its route");
            Assert.AreEqual(0f, tripBody.position.x, 0.001f);
        }

        [UnityTest]
        public IEnumerator TripCar_StopsBehindAnObstacle_AndResumesAndArrivesWhenItIsRemoved()
        {
            var trip = (CarDriver)_factory.SpawnArriving(new CarId(1), Vector2.zero, 0f, 0);
            var tripBody = trip.GetComponent<Rigidbody2D>();
            var parked = (CarDriver)_factory.Spawn(_definition, new CarId(2), new Vector2(0f, -4f), 0f, default, false);
            yield return new WaitForSeconds(5f);
            Assert.Less(tripBody.position.y, -5.9f, "stopped behind the parked car, not through it");
            Assert.AreEqual(0f, _arrivals.Count);
            var stoppedAt = tripBody.position.y;
            yield return new WaitForSeconds(1f);
            Assert.AreEqual(stoppedAt, tripBody.position.y, 0.05f, "still waiting");
            Object.Destroy(parked.gameObject);
            yield return new WaitForSeconds(5f);
            Assert.AreEqual(1, _arrivals.Count, "exactly one arrival once the way is clear");
            Assert.AreEqual(RigidbodyType2D.Dynamic, tripBody.bodyType);
        }

        private void Set(string field, Object value)
            => typeof(CarFactory).GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(_factory, value);
    }
}
