using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UncannyValet.Cars;
using UncannyValet.Core;
using UncannyValet.Core.Config;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Night;
using UnityEngine;
using UnityEngine.TestTools;

namespace UncannyValet.Tests.PlayMode
{
    /// <summary>Modules that run their own time stop while IWorldTime reports scale 0 and resume without a jump.</summary>
    public class FreezeTimePlayModeTests
    {
        private const float StartY = -14f;

        private sealed class StubTime : IWorldTime
        {
            public float Value = 1f;
            public float Scale => Value;
            public bool IsSlowed => Value < 1f;
        }

        private sealed class StraightTrips : IEntryRouter, ITripDriver, ITripObstacles
        {
            public float Y = StartY;
            public int BeginEntry(int slot) => 1;
            public void SetObstacleGap(int tripId, float gap) { }
            public void Cancel(int tripId) { }

            public bool Step(int tripId, float dt, out TripPose pose)
            {
                Y = Mathf.Min(0f, Y + 4f * dt);
                pose = new TripPose(new Vector2(0f, Y), 0f, 4f, Y >= 0f ? TripPhase.Finished : TripPhase.OnRoute);
                return true;
            }
        }

        private readonly StubTime _time = new StubTime();
        private IEventBus _bus;
        private GameObject _clockObject;
        private ShiftConfig _shift;
        private GameObject _factoryObject;
        private CarFactory _factory;
        private StraightTrips _trips;

        [SetUp]
        public void SetUp()
        {
            Services.Reset();
            _time.Value = 1f;
            _bus = new EventBus();
            Services.Register<IEventBus>(_bus);
            Services.Register<IWorldTime>(_time);
            _trips = new StraightTrips();
            Services.Register<IEntryRouter>(_trips);
            Services.Register<ITripDriver>(_trips);
            Services.Register<ITripObstacles>(_trips);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var car in Object.FindObjectsByType<CarDriver>(FindObjectsSortMode.None)) Object.DestroyImmediate(car.gameObject);
            if (_factoryObject != null) Object.DestroyImmediate(_factoryObject);
            if (_clockObject != null) Object.DestroyImmediate(_clockObject);
            if (_shift != null) Object.DestroyImmediate(_shift);
            Services.Reset();
        }

        [UnityTest]
        public IEnumerator ShiftClock_StopsWhileFrozen_AndRunsAgainAfterwards()
        {
            _shift = ScriptableObject.CreateInstance<ShiftConfig>();
            _clockObject = new GameObject("clock");
            var clock = _clockObject.AddComponent<ShiftClock>();
            clock.Initialize(_bus, _shift);
            var ticks = 0;
            _bus.Subscribe<ShiftTick>(_ => ticks++);
            _time.Value = 0f;
            clock.StartShift();
            yield return new WaitForSeconds(1.5f);
            Assert.AreEqual(_shift.ShiftLengthSeconds, clock.SecondsRemaining, 0.001f, "no seconds counted while frozen");
            Assert.AreEqual(0f, clock.ElapsedMinutes);
            Assert.AreEqual(0, ticks);
            _time.Value = 1f;
            yield return new WaitForSeconds(1.5f);
            Assert.Less(clock.SecondsRemaining, _shift.ShiftLengthSeconds - 0.5f, "runs again");
        }

        [UnityTest]
        public IEnumerator TripCar_StandsStillWhileFrozen_AndDrivesOnWithoutAJump()
        {
            _factoryObject = new GameObject("Cars");
            _factoryObject.SetActive(false);
            _factory = _factoryObject.AddComponent<CarFactory>();
            Set("_defaultDefinition", ScriptableObject.CreateInstance<CarDefinition>());
            Set("_flash", ScriptableObject.CreateInstance<CarFlashConfig>());
            Set("_handling", ScriptableObject.CreateInstance<CarHandlingConfig>());
            _factoryObject.SetActive(true);
            var trip = (CarDriver)_factory.SpawnArriving(new CarId(1), Vector2.zero, 0f, 0);
            var body = trip.GetComponent<Rigidbody2D>();
            _time.Value = 0f;
            yield return new WaitForSeconds(1f);
            var frozenAt = body.position.y;
            yield return new WaitForSeconds(1f);
            Assert.AreEqual(frozenAt, body.position.y, 0.0001f, "a frozen trip car does not move");
            _time.Value = 1f;
            yield return new WaitForSeconds(0.5f);
            Assert.Greater(body.position.y, frozenAt + 0.5f, "it drives on after the freeze");
            Assert.Less(body.position.y, frozenAt + 4f, "without teleporting");
        }

        private void Set(string field, Object value)
            => typeof(CarFactory).GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(_factory, value);
    }
}
