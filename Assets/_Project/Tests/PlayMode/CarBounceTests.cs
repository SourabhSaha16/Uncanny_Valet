using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UncannyValet.Cars;
using UncannyValet.Core;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UnityEngine;
using UnityEngine.TestTools;

namespace UncannyValet.Tests.PlayMode
{
    /// <summary>Car-on-car rubber bounce: a driven car hits a parked one and rebounds, the parked car settles, and cars that are not live are ignored.</summary>
    public class CarBounceTests
    {
        private const float Cruise = 3f;
        private const float RunSeconds = 3f;
        private const float StartGap = 3.2f; // centre to centre; the cars are 2 long, so about 1.2 apart

        private GameObject _factoryObject;
        private CarFactory _factory;
        private CarDefinition _definition;
        private readonly List<CarsCollided> _events = new List<CarsCollided>();
        private IEventBus _bus;

        [SetUp]
        public void SetUp()
        {
            Services.Reset();
            _bus = new EventBus();
            Services.Register<IEventBus>(_bus);
            _bus.Subscribe<CarsCollided>(_events.Add);
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
            _bus.Unsubscribe<CarsCollided>(_events.Add);
            _events.Clear();
            foreach (var car in Object.FindObjectsByType<CarDriver>(FindObjectsSortMode.None)) Object.DestroyImmediate(car.gameObject);
            Object.DestroyImmediate(_factoryObject);
            Services.Reset();
        }

        [UnityTest]
        public IEnumerator DrivenCarHittingAParkedCar_ReboundsAndTheParkedCarSettles()
        {
            var parked = Spawn(1, Vector2.zero);
            var driven = Spawn(2, new Vector2(0f, -StartGap));
            driven.SetDriver(true);
            var body = driven.GetComponent<Rigidbody2D>();
            var parkedBody = parked.GetComponent<Rigidbody2D>();
            var minForward = 0f;
            for (var t = 0f; t < RunSeconds; t += Time.fixedDeltaTime)
            {
                yield return new WaitForFixedUpdate();
                driven.SetInput(_events.Count == 0 && body.linearVelocity.y < Cruise ? 1f : 0f, 0f);
                minForward = Mathf.Min(minForward, body.linearVelocity.y);
            }
            Assert.AreEqual(1, _events.Count, "one bump event for the pair");
            Assert.Greater(_events[0].Strength01, 0f);
            Assert.LessOrEqual(_events[0].Strength01, 1f);
            Assert.Less(minForward, -0.3f, "the driven car rebounds backwards");
            Assert.Greater(parkedBody.position.y, 0.01f, "the parked car is shoved a little");
            Assert.Less(parkedBody.position.y, 1f, "but only a little");
            Assert.Less(parkedBody.linearVelocity.magnitude, 0.05f, "and settles");
        }

        [UnityTest]
        public IEnumerator GentleTouch_IsIgnored()
        {
            Spawn(1, Vector2.zero);
            var driven = Spawn(2, new Vector2(0f, -2.01f));
            driven.SetDriver(true);
            driven.GetComponent<Rigidbody2D>().linearVelocity = Vector2.up * 0.3f;
            yield return new WaitForSeconds(1f);
            Assert.AreEqual(0, _events.Count);
        }

        [UnityTest]
        public IEnumerator CarNotLive_NeitherTriggersNorSuffersTheResponse()
        {
            var arriving = Spawn(1, Vector2.zero);
            arriving.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic; // as CarAutoDrive does
            arriving.enabled = false;
            var driven = Spawn(2, new Vector2(0f, -StartGap));
            driven.SetDriver(true);
            var body = driven.GetComponent<Rigidbody2D>();
            var minForward = 0f;
            for (var t = 0f; t < RunSeconds; t += Time.fixedDeltaTime)
            {
                yield return new WaitForFixedUpdate();
                driven.SetInput(body.linearVelocity.y < Cruise ? 1f : 0f, 0f);
                minForward = Mathf.Min(minForward, body.linearVelocity.y);
            }
            Assert.AreEqual(0, _events.Count);
            Assert.GreaterOrEqual(minForward, -0.05f, "no rebound against a car that is not live");
            Assert.AreEqual(Vector2.zero, arriving.GetComponent<Rigidbody2D>().position);
        }

        private CarDriver Spawn(int id, Vector2 position) => (CarDriver)_factory.Spawn(_definition, new CarId(id), position, 0f, default, false);

        private void Set(string field, Object value)
            => typeof(CarFactory).GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(_factory, value);
    }
}
