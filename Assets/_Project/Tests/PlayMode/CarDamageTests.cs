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
    /// <summary>Hard hits damage every car involved (both cars of a crash, a car against a wall); taps and pedestrians do not.</summary>
    public class CarDamageTests
    {
        private const float StartGap = 12f;
        private const float RunSeconds = 4f;

        private GameObject _factoryObject;
        private CarFactory _factory;
        private CarDefinition _definition;
        private IEventBus _bus;
        private readonly List<CarDamaged> _damaged = new List<CarDamaged>();
        private readonly List<CarImpact> _impacts = new List<CarImpact>();

        [SetUp]
        public void SetUp()
        {
            Services.Reset();
            _bus = new EventBus();
            Services.Register<IEventBus>(_bus);
            _bus.Subscribe<CarDamaged>(_damaged.Add);
            _bus.Subscribe<CarImpact>(_impacts.Add);
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
            _bus.Unsubscribe<CarDamaged>(_damaged.Add);
            _bus.Unsubscribe<CarImpact>(_impacts.Add);
            _damaged.Clear();
            _impacts.Clear();
            foreach (var car in Object.FindObjectsByType<CarDriver>(FindObjectsSortMode.None)) Object.DestroyImmediate(car.gameObject);
            foreach (var wall in Object.FindObjectsByType<BoxCollider2D>(FindObjectsSortMode.None)) Object.DestroyImmediate(wall.gameObject);
            Object.DestroyImmediate(_factoryObject);
            Services.Reset();
        }

        [UnityTest]
        public IEnumerator TwoCarsCollidingAtSpeed_BothAreDamaged()
        {
            var parked = Spawn(1, Vector2.zero);
            var driven = Spawn(2, new Vector2(0f, -StartGap));
            driven.SetDriver(true);
            for (var t = 0f; t < RunSeconds; t += Time.fixedDeltaTime)
            {
                yield return new WaitForFixedUpdate();
                driven.SetInput(1f, 0f);
            }
            Assert.IsTrue(_damaged.Exists(d => d.Car == parked.Id && d.Damage01 > 0f), "the parked car is damaged");
            Assert.IsTrue(_damaged.Exists(d => d.Car == driven.Id && d.Damage01 > 0f), "the driven car is damaged");
            Assert.IsTrue(_impacts.Exists(i => i.Car == driven.Id && i.IsPlayerCar && i.Force > 0f));
            Assert.IsTrue(_impacts.Exists(i => i.Car == parked.Id && !i.IsPlayerCar));
            Assert.Greater(driven.GetComponent<CarDamage>().Damage01, 0f);
        }

        [UnityTest]
        public IEnumerator HittingAWallAtSpeed_DamagesTheCar()
        {
            var wall = new GameObject("Wall").AddComponent<BoxCollider2D>();
            wall.size = new Vector2(20f, 1f);
            wall.transform.position = new Vector2(0f, 0f);
            var driven = Spawn(1, new Vector2(0f, -StartGap));
            driven.SetDriver(true);
            for (var t = 0f; t < RunSeconds; t += Time.fixedDeltaTime)
            {
                yield return new WaitForFixedUpdate();
                driven.SetInput(1f, 0f);
            }
            Assert.IsTrue(_damaged.Exists(d => d.Car == driven.Id && d.Delta01 > 0f));
        }

        [UnityTest]
        public IEnumerator AGentleTap_DamagesNothing_AndPedestrianLayerIsIgnored()
        {
            Spawn(1, Vector2.zero);
            var driven = Spawn(2, new Vector2(0f, -2.01f));
            driven.SetDriver(true);
            driven.GetComponent<Rigidbody2D>().linearVelocity = Vector2.up * 0.3f;
            var walker = new GameObject("Walker") { layer = PhysicsLayers.Pedestrian };
            walker.transform.position = new Vector2(5f, 0f);
            walker.AddComponent<BoxCollider2D>();
            yield return new WaitForSeconds(1f);
            Assert.AreEqual(0, _damaged.Count);
            Assert.AreEqual(0, _impacts.Count);
            Object.DestroyImmediate(walker);
        }

        private CarDriver Spawn(int id, Vector2 position) => (CarDriver)_factory.Spawn(_definition, new CarId(id), position, 0f, default, false);

        private void Set(string field, Object value)
            => typeof(CarFactory).GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(_factory, value);
    }
}
