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
using UnityEngine;
using UnityEngine.TestTools;

namespace UncannyValet.Tests.PlayMode
{
    /// <summary>
    /// Park Smash on the real lot: waiting cars are stood in free bays nose-in, an EV goes to the EV bay, CarParked is published
    /// once per car (the bay's own settle does not repeat it), no Perfect Park is judged, and cars that do not fit stay put.
    /// </summary>
    public class ParkSmashPlayModeTests
    {
        private const float PositionTolerance = 0.05f;
        private const float SettleWait = 1.5f;
        private const int EvBay = 7;
        private const int RegularBays = 6;

        private GameObject _lot;
        private GameObject _factoryObject;
        private CarFactory _factory;
        private CarDefinition _definition;
        private LotLayoutBuilder _layout;
        private IEventBus _bus;
        private readonly List<ParkSmashCarParked> _smashParked = new List<ParkSmashCarParked>();
        private readonly List<CarParked> _parked = new List<CarParked>();
        private readonly List<ParkSmashParked> _done = new List<ParkSmashParked>();
        private bool _perfect;

        [SetUp]
        public void SetUp()
        {
            Services.Reset();
            _bus = new EventBus();
            Services.Register<IEventBus>(_bus);
            _smashParked.Clear(); _parked.Clear(); _done.Clear(); _perfect = false;
            _bus.Subscribe<ParkSmashCarParked>(e => _smashParked.Add(e));
            _bus.Subscribe<CarParked>(e => _parked.Add(e));
            _bus.Subscribe<ParkSmashParked>(e => _done.Add(e));
            _bus.Subscribe<PerfectPark>(e => _perfect = true);
            _definition = ScriptableObject.CreateInstance<CarDefinition>();
            _lot = Build<LotLayoutBuilder>("Lot", "_config", ScriptableObject.CreateInstance<LotConfig>(), out _layout);
            _factoryObject = Build<CarFactory>("Cars", "_defaultDefinition", _definition, out _factory);
            Set(_factory, "_flash", ScriptableObject.CreateInstance<CarFlashConfig>());
            Set(_factory, "_handling", ScriptableObject.CreateInstance<CarHandlingConfig>());
            Services.Register<ICarFactory>(_factory);
            Services.Register<ILotLayout>(_layout);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var car in Object.FindObjectsByType<CarDriver>(FindObjectsSortMode.None)) Object.DestroyImmediate(car.gameObject);
            Object.Destroy(_lot);
            Object.Destroy(_factoryObject);
            Services.Reset();
        }

        [UnityTest]
        public IEnumerator WaitingCars_AreParkedNoseIn_EvToTheEvBay_WithoutAPerfectPark_OrADoubleCarParked()
        {
            yield return null; // components Start and subscribe
            _bus.Publish(new ShiftPlanChanged(new ShiftPlan(6, 1f, 0f, 0f, true, true)));
            var plain = Waiting(1, false, false, 0);
            var ev = Waiting(2, true, false, 1);
            var dirty = Waiting(3, false, true, 2);

            _bus.Publish(new PowerActivated(PowerKind.ParkSmash));

            Assert.AreEqual(1, _done.Count);
            Assert.AreEqual(3, _done[0].Parked);
            Assert.AreEqual(1, _done[0].Charged);
            Assert.AreEqual(1, _done[0].Washed);
            Assert.AreEqual(EvBay, SpotOf(2));
            Assert.AreEqual(1, SpotOf(1));
            Assert.AreEqual(2, SpotOf(3));
            AssertInBay(plain, 1, 0f);
            AssertInBay(ev, EvBay, 180f);
            AssertInBay(dirty, 2, 0f);
            Assert.AreEqual(3, _parked.Count, "CarParked once per car, published by the bay");

            yield return new WaitForSeconds(SettleWait); // the bays now settle the cars themselves
            Assert.AreEqual(3, _parked.Count, "the bay's own settle must not announce the car again");
            Assert.IsFalse(_perfect, "a smash park is not a Perfect Park");
        }

        [UnityTest]
        public IEnumerator CarsThatDoNotFit_StayWhereTheyAre()
        {
            yield return null;
            var cars = new List<IDrivable>();
            for (var i = 0; i < RegularBays + 1; i++) cars.Add(Waiting(10 + i, false, false, i % _layout.ArrivalSlots.Count));
            var lastStart = cars[RegularBays].Transform.position;

            _bus.Publish(new PowerActivated(PowerKind.ParkSmash));

            Assert.AreEqual(RegularBays, _done[0].Parked);
            Assert.AreEqual(lastStart, cars[RegularBays].Transform.position, "the seventh car has no bay and stays queued");
        }

        [UnityTest]
        public IEnumerator DrivenCars_AndCarsAlreadyRequested_AreLeftAlone()
        {
            yield return null;
            var driven = Waiting(20, false, false, 0);
            driven.SetDriver(true);
            var asked = Waiting(21, false, false, 1);
            _bus.Publish(new PickupRequested(new TicketId(21), new CarId(21)));

            _bus.Publish(new PowerActivated(PowerKind.ParkSmash));

            Assert.AreEqual(0, _done[0].Parked);
            Assert.AreEqual(0, _smashParked.Count);
            Assert.IsNotNull(asked);
        }

        private IDrivable Waiting(int id, bool ev, bool dirty, int slot)
        {
            var at = _layout.ArrivalSlots[slot];
            var car = _factory.Spawn(_definition, new CarId(id), at, 0f, default, false);
            _bus.Publish(new TicketTraitsAssigned(new TicketId(id), new CarId(id), ev, dirty));
            _bus.Publish(new CarArrived(new CarId(id)));
            return car;
        }

        private int SpotOf(int carId)
        {
            foreach (var e in _smashParked) if (e.Car.Value == carId) return e.SpotId;
            return -1;
        }

        private void AssertInBay(IDrivable car, int bay, float expectedRotation)
        {
            var spot = _lot.transform.Find((bay == EvBay ? "Charging Spot " : "Spot ") + bay);
            Assert.IsNotNull(spot, "bay " + bay);
            Assert.AreEqual(0f, Vector2.Distance(spot.position, car.Transform.position), PositionTolerance, "car stands centred in bay " + bay);
            Assert.AreEqual(0f, Mathf.Abs(Mathf.DeltaAngle(expectedRotation, car.Transform.eulerAngles.z)), 1f, "nose-in rotation");
        }

        private static GameObject Build<T>(string name, string field, Object value, out T component) where T : Component
        {
            var go = new GameObject(name);
            go.SetActive(false);
            component = go.AddComponent<T>();
            Set(component, field, value);
            go.SetActive(true);
            return go;
        }

        private static void Set(object target, string field, Object value)
            => target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    }
}
