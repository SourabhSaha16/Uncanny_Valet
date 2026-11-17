using System.Collections;
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
    /// <summary>Refs #170: on ShiftEnded every car is despawned at once, including a car already driving away.</summary>
    public class ShiftEndDespawnTests
    {
        private GameObject _factoryObject;
        private CarFactory _factory;
        private IEventBus _bus;

        [SetUp]
        public void SetUp()
        {
            Services.Reset();
            _bus = new EventBus();
            Services.Register<IEventBus>(_bus);
            _factoryObject = new GameObject("Cars");
            _factoryObject.SetActive(false);
            _factory = _factoryObject.AddComponent<CarFactory>();
            Set("_defaultDefinition", ScriptableObject.CreateInstance<CarDefinition>());
            Set("_flash", ScriptableObject.CreateInstance<CarFlashConfig>());
            Set("_handling", ScriptableObject.CreateInstance<CarHandlingConfig>());
            _factoryObject.SetActive(true);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_factoryObject);
            Services.Reset();
        }

        [UnityTest]
        public IEnumerator ShiftEnded_DespawnsEveryCarAtOnce_EvenOneDrivingAway()
        {
            _factory.Spawn(new CarId(1), Vector2.zero, 0f);
            _factory.Spawn(new CarId(2), new Vector2(5f, 0f), 0f);
            _factory.Spawn(new CarId(3), new Vector2(10f, 0f), 0f);
            _factory.DriveAwayAndDespawn(new CarId(3));
            yield return null;
            Assert.IsTrue(_factory.TryGet(new CarId(1), out _));

            _bus.Publish(new ShiftEnded());
            yield return null;

            for (var id = 1; id <= 3; id++) Assert.IsFalse(_factory.TryGet(new CarId(id), out _), "car " + id);
            Assert.AreEqual(0, Object.FindObjectsByType<CarDriver>(FindObjectsSortMode.None).Length, "no car object left");
        }

        private void Set(string field, object value)
            => typeof(CarFactory).GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(_factory, value);
    }
}
