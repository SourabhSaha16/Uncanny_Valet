using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UncannyValet.Cars;
using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Lot;
using UncannyValet.Patrons;
using UnityEngine;
using UnityEngine.TestTools;

namespace UncannyValet.Tests.PlayMode
{
    /// <summary>Issue 127 in physics: the patron steps out only when the valet is at the pickup car, and the next car drives into the freed pickup spot.</summary>
    public class PickupQueuePlayModeTests
    {
        private sealed class Focus : IPlayerFocus
        {
            public Vector2 Position { get; set; } = new Vector2(40f, 40f);
        }

        private GameObject _lotObject;
        private GameObject _factoryObject;
        private CarFactory _factory;
        private LotLayoutBuilder _layout;
        private QueueAdvancer _queue;
        private Focus _focus;
        private EventBus _bus;
        private readonly List<PatronStepsOut> _out = new List<PatronStepsOut>();
        private readonly List<CarRepositioned> _moved = new List<CarRepositioned>();

        [SetUp]
        public void SetUp()
        {
            Services.Reset();
            _bus = new EventBus();
            Services.Register<IEventBus>(_bus);
            var config = ScriptableObject.CreateInstance<LotConfig>();
            _lotObject = Build<LotLayoutBuilder>("Lot", "_config", config, out _layout);
            typeof(LotLayoutBuilder).GetField("_queueSlots", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(_layout, 4);
            var definition = ScriptableObject.CreateInstance<CarDefinition>();
            _factoryObject = Build<CarFactory>("Cars", "_defaultDefinition", definition, out _factory);
            typeof(CarFactory).GetField("_flash", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(_factory, ScriptableObject.CreateInstance<CarFlashConfig>());
            typeof(CarFactory).GetField("_handling", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(_factory, ScriptableObject.CreateInstance<CarHandlingConfig>());
            _focus = new Focus();
            var settings = new PickupQueueSettings();
            _queue = new QueueAdvancer(_bus, _factory, _layout, new ArrivalSlotScanner(settings.AdvanceClearSize, _layout), settings) { Focus = _focus };
            _out.Clear();
            _moved.Clear();
            _bus.Subscribe<PatronStepsOut>(e => _out.Add(e));
            _bus.Subscribe<CarRepositioned>(e => _moved.Add(e));
        }

        [TearDown]
        public void TearDown()
        {
            _queue.Dispose();
            foreach (var car in Object.FindObjectsByType<CarDriver>(FindObjectsSortMode.None)) Object.DestroyImmediate(car.gameObject);
            Object.Destroy(_lotObject);
            Object.Destroy(_factoryObject);
            Services.Reset();
        }

        private CarDriver Arrive(int id, int slot)
        {
            var position = _layout.ArrivalSlots[slot];
            var car = (CarDriver)_factory.Spawn(new CarId(id), position, _layout.ArrivalRotationDegrees);
            _bus.Publish(new CarArrived(car.Id));
            return car;
        }

        private IEnumerator Run(float seconds)
        {
            for (var t = 0f; t < seconds; t += Time.fixedDeltaTime)
            {
                yield return new WaitForFixedUpdate();
                _queue.Tick();
            }
        }

        [UnityTest]
        public IEnumerator ThePatronStepsOutOnlyWhenTheValetComesUpToTheCarInThePickupSpot()
        {
            Arrive(1, 0);
            Arrive(2, 1);
            yield return Run(0.5f);
            Assert.AreEqual(0, _out.Count, "the valet is far away");
            _focus.Position = _layout.ArrivalSlots[0] + new Vector2(2f, 0f);
            yield return Run(0.2f);
            Assert.AreEqual(1, _out.Count);
            Assert.AreEqual(new CarId(1), _out[0].Car, "only the car in the pickup spot");
        }

        [UnityTest]
        public IEnumerator TheCarBehindDrivesIntoThePickupSpotOnceTheValetHasDrivenTheFirstAway()
        {
            var first = Arrive(1, 0);
            var second = Arrive(2, 1);
            yield return Run(0.5f);
            Assert.AreEqual(0, _moved.Count, "nobody moves while the pickup spot is taken");
            first.SetDriver(true);
            first.GetComponent<Rigidbody2D>().position = _layout.ArrivalSlots[0] + new Vector2(14f, 0f);
            yield return Run(4f);
            Assert.AreEqual(1, _moved.Count, "the second car reached the pickup spot");
            Assert.AreEqual(second.Id, _moved[0].Car);
            Assert.Less(Vector2.Distance(second.GetComponent<Rigidbody2D>().position, _layout.ArrivalSlots[0]), 0.1f);
        }

        [UnityTest]
        public IEnumerator AFullQueueMovesUpIntoTheirSlotsWhenThePickupCarIsDrivenAway_EvenOneThatStoppedShort()
        {
            var cars = new CarDriver[4];
            for (var i = 0; i < 4; i++) cars[i] = Arrive(i + 1, i);
            // The front queued car stopped a little short of its slot: its nose reaches into the pickup spot's box.
            cars[1].GetComponent<Rigidbody2D>().position = _layout.ArrivalSlots[1] + new Vector2(0f, 0.9f);
            cars[1].transform.position = cars[1].GetComponent<Rigidbody2D>().position;
            yield return Run(0.5f);
            cars[0].SetDriver(true);
            cars[0].GetComponent<Rigidbody2D>().position = _layout.ArrivalSlots[0] + new Vector2(14f, 0f);
            yield return Run(8f);
            for (var i = 1; i < 4; i++)
            {
                var at = cars[i].GetComponent<Rigidbody2D>().position;
                Assert.Less(Vector2.Distance(at, _layout.ArrivalSlots[i - 1]), 0.1f, "car " + (i + 1) + " ends in slot " + (i - 1));
                Assert.IsFalse(cars[i].GetComponent<CarAutoDrive>().IsBusy, "car " + (i + 1) + " stopped driving");
            }
        }

        private static GameObject Build<T>(string name, string field, Object value, out T component) where T : Component
        {
            var go = new GameObject(name);
            go.SetActive(false);
            component = go.AddComponent<T>();
            typeof(T).GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(component, value);
            go.SetActive(true);
            return go;
        }
    }
}
