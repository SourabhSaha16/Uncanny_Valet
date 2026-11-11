using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UncannyValet.Cars;
using UncannyValet.Cars.Fx;
using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UnityEngine;
using UnityEngine.TestTools;

namespace UncannyValet.Tests.PlayMode
{
    /// <summary>A human-driven car that slides leaves skid marks and raises the slide events; a car with nobody in it never does.</summary>
    public class SkidMarkTests
    {
        private GameObject _factoryObject;
        private CarFactory _factory;
        private CarDefinition _definition;
        private IEventBus _bus;
        private readonly List<IGameEvent> _events = new List<IGameEvent>();

        [SetUp]
        public void SetUp()
        {
            Services.Reset();
            _bus = new EventBus();
            Services.Register<IEventBus>(_bus);
            _bus.Subscribe<CarSlideStarted>(OnStarted);
            _bus.Subscribe<CarSlideEnded>(OnEnded);
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
            _bus.Unsubscribe<CarSlideStarted>(OnStarted);
            _bus.Unsubscribe<CarSlideEnded>(OnEnded);
            _events.Clear();
            foreach (var car in Object.FindObjectsByType<CarDriver>(FindObjectsSortMode.None)) Object.DestroyImmediate(car.gameObject);
            Object.DestroyImmediate(_factoryObject);
            Services.Reset();
        }

        [UnityTest]
        public IEnumerator DrivenCarThatSlides_LeavesMarksAndReportsTheSlide()
        {
            var car = Spawn(1);
            car.SetDriver(true);
            yield return Drive(car, 1f, 0f, false, 1.5f);
            yield return Drive(car, 1f, 1f, true, 1.5f);
            var pool = Object.FindFirstObjectByType<DecalPool>();
            Assert.IsNotNull(pool);
            Assert.Greater(pool.ActiveCount, 2, "marks behind both tyres");
            Assert.LessOrEqual(pool.ActiveCount, pool.Capacity);
            Assert.IsTrue(_events.Exists(e => e is CarSlideStarted));
            Assert.IsTrue(Services.TryGet<ISlipSource>(out var slip));
            Assert.IsTrue(slip.IsSliding);
            Assert.Greater(slip.Intensity01, 0f);
        }

        [UnityTest]
        public IEnumerator SlideEnds_WhenTheDriverGetsOut()
        {
            var car = Spawn(1);
            car.SetDriver(true);
            yield return Drive(car, 1f, 1f, true, 3f);
            car.SetDriver(false);
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            Assert.IsTrue(_events.Exists(e => e is CarSlideEnded));
            Assert.IsFalse(Services.Get<ISlipSource>().IsSliding);
        }

        [UnityTest]
        public IEnumerator CarWithoutADriver_NeverLeavesMarksEvenWhenSlidingFast()
        {
            var car = Spawn(1);
            car.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(6f, 6f);
            for (var t = 0f; t < 0.5f; t += Time.fixedDeltaTime) yield return new WaitForFixedUpdate();
            var pool = Object.FindFirstObjectByType<DecalPool>();
            Assert.IsTrue(pool == null || pool.ActiveCount == 0);
            Assert.AreEqual(0, _events.Count);
        }

        private IEnumerator Drive(CarDriver car, float throttle, float steer, bool drift, float seconds)
        {
            for (var t = 0f; t < seconds; t += Time.fixedDeltaTime)
            {
                car.SetInput(throttle, steer);
                car.SetDrift(drift);
                yield return new WaitForFixedUpdate();
            }
        }

        private CarDriver Spawn(int id) => (CarDriver)_factory.Spawn(_definition, new CarId(id), Vector2.zero, 0f, default, false);

        private void Set(string field, Object value)
            => typeof(CarFactory).GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(_factory, value);

        private void OnStarted(CarSlideStarted e) => _events.Add(e);
        private void OnEnded(CarSlideEnded e) => _events.Add(e);
    }
}
