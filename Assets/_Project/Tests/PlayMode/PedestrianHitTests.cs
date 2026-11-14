using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UncannyValet.Cars;
using UncannyValet.Core;
using UncannyValet.Core.Config;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Economy;
using UncannyValet.Pedestrians;
using UnityEngine;
using UnityEngine.TestTools;

namespace UncannyValet.Tests.PlayMode
{
    /// <summary>A driven car hits a patron walker and an ambient walker: knockback, no bounce, 15% slower, one curse for the patron only (crazy lot).</summary>
    public class PedestrianHitTests
    {
        private const float ApproachSeconds = 3f;
        private GameObject _factoryObject;
        private CarFactory _factory;
        private CarDefinition _definition;
        private EventBus _bus;
        private TuningStore _tuning;
        private ShiftBlessings _blessings;
        private ShiftConfig _shift;
        private readonly List<PedestrianHit> _hits = new List<PedestrianHit>();
        private readonly List<GameObject> _walkers = new List<GameObject>();
        private PedestrianConfig _pedestrians;

        private void Setup(bool cursesOn)
        {
            Services.Reset();
            _bus = new EventBus();
            _tuning = new TuningStore(_bus);
            PedestrianHitRows.Seed(_tuning, cursesOn);
            Services.Register<IEventBus>(_bus);
            Services.Register<ITuning>(_tuning);
            _bus.Subscribe<PedestrianHit>(_hits.Add);
            _shift = ScriptableObject.CreateInstance<ShiftConfig>();
            _blessings = new ShiftBlessings(_bus, _shift);
            _bus.Publish(new ShiftStarted());
            _bus.Publish(new BlessingEarned(new TicketId(1), 40));
            _pedestrians = ScriptableObject.CreateInstance<PedestrianConfig>();
            foreach (var layer in new[] { "_model", "_outfit", "_hair" })
                typeof(PedestrianConfig).GetField(layer, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(_pedestrians, new Sprite[24]);
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
            _blessings?.Dispose();
            _hits.Clear();
            foreach (var car in Object.FindObjectsByType<CarDriver>(FindObjectsSortMode.None)) Object.DestroyImmediate(car.gameObject);
            foreach (var walker in _walkers) Object.DestroyImmediate(walker);
            _walkers.Clear();
            Object.DestroyImmediate(_factoryObject);
            Object.DestroyImmediate(_shift);
            Object.DestroyImmediate(_pedestrians);
            Services.Reset();
        }

        [UnityTest]
        public IEnumerator CrazyLot_PatronIsKnockedAwayCarKeepsGoingSlowerAndOneCurseLands()
        {
            Setup(true);
            var walker = Walker(new Vector2(0f, 0f), patron: true);
            var car = Spawn(new Vector2(0f, -6f));
            yield return Drive(car, walker);
            Assert.AreEqual(1, _hits.Count, "one knockback per hit");
            Assert.IsTrue(_hits[0].IsPatron);
            Assert.IsTrue(_hits[0].ByPlayerCar);
            Assert.Greater(_hits[0].Speed, 1f);
            Assert.Greater(walker.transform.position.y, 0.3f, "the walker was thrown ahead of the car");
            Assert.GreaterOrEqual(car.GetComponent<Rigidbody2D>().linearVelocity.y, -0.05f, "the car is not bounced back");
            Assert.Less(car.GetComponent<Rigidbody2D>().linearVelocity.magnitude, _hits[0].Speed, "the car lost speed");
            Assert.AreEqual(1, _blessings.Curses);
            Assert.AreEqual(35, _blessings.Blessings);
        }

        [UnityTest]
        public IEnumerator CrazyLot_AmbientWalkerIsKnockedAwayWithoutACurse()
        {
            Setup(true);
            var walker = Walker(Vector2.zero, patron: false);
            var car = Spawn(new Vector2(0f, -6f));
            yield return Drive(car, walker);
            Assert.AreEqual(1, _hits.Count);
            Assert.IsFalse(_hits[0].IsPatron);
            Assert.Greater(walker.transform.position.y, 0.3f);
            Assert.AreEqual(0, _blessings.Curses);
            Assert.AreEqual(40, _blessings.Blessings);
        }

        [UnityTest]
        public IEnumerator TrainingLot_PatronIsKnockedAwayButNeverCursed()
        {
            Setup(false);
            var walker = Walker(Vector2.zero, patron: true);
            var car = Spawn(new Vector2(0f, -6f));
            yield return Drive(car, walker);
            Assert.AreEqual(1, _hits.Count);
            Assert.Greater(walker.transform.position.y, 0.3f);
            Assert.AreEqual(0, _blessings.Curses);
            Assert.AreEqual(40, _blessings.Blessings);
        }

        private IEnumerator Drive(CarDriver car, PedestrianWalker walker)
        {
            walker.CarHit += (w, c, p, s) => _bus.Publish(WalkerKnockback.ToEvent(w, c, s));
            car.SetDriver(true);
            for (var t = 0f; t < ApproachSeconds && _hits.Count == 0; t += Time.fixedDeltaTime)
            {
                yield return new WaitForFixedUpdate();
                car.SetInput(1f, 0f);
            }
            car.SetInput(0f, 0f);
            yield return new WaitForSeconds(0.3f);
        }

        private PedestrianWalker Walker(Vector2 position, bool patron)
        {
            var go = new GameObject("Walker");
            _walkers.Add(go);
            var walker = go.AddComponent<PedestrianWalker>();
            walker.Create(_pedestrians);
            if (patron) walker.Brain.BeginPatron(); else walker.Brain.BeginAmbient(5f);
            walker.Walk(new List<Vector2> { position, position + Vector2.up * 0.01f }); // solid, and standing still (speed 0)
            return walker;
        }

        private CarDriver Spawn(Vector2 position) => (CarDriver)_factory.Spawn(_definition, new CarId(2), position, 0f, default, false);

        private void Set(string field, Object value)
            => typeof(CarFactory).GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(_factory, value);
    }
}
