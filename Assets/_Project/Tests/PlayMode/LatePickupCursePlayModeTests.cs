using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Core;
using UncannyValet.Core.Config;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Core.Modifiers;
using UncannyValet.Economy;
using UncannyValet.Night;
using UncannyValet.Patrons;
using UncannyValet.UI.Hud;
using UnityEngine;
using UnityEngine.TestTools;

namespace UncannyValet.Tests.PlayMode
{
    /// <summary>
    /// Issue 173 through the real flow: park, stay ends, the pickup timer runs out, the car reaches the curb late. Real ShiftClock,
    /// TicketRegistry, ShiftBlessings on its production clock (Time.time), the seeded tuning and the HUD pop tracker.
    /// </summary>
    public class LatePickupCursePlayModeTests
    {
        private sealed class OneCar : ICarFactory
        {
            private readonly IDrivable _car;
            public OneCar(IDrivable car) { _car = car; }
            public IDrivable Spawn(CarId id, Vector2 position, float rotationDegrees) => null;
            public IDrivable SpawnArriving(CarId id, Vector2 stopPosition, float rotationDegrees, int arrivalSlot) => null;
            public void DriveAwayAndDespawn(CarId id) { }
            public bool TryDriveTo(CarId id, Vector2 target, float speed) => false;
            public bool TryGet(CarId id, out IDrivable car) { car = _car; return true; }
            public void Despawn(CarId id) { }
        }

        private sealed class Body : IDrivable
        {
            public Body(CarId id, Transform t) { Id = id; Transform = t; }
            public CarId Id { get; }
            public Transform Transform { get; }
            public float Speed => 0f;
            public bool HasDriver => false;
            public void SetInput(float throttle, float steer) { }
            public void SetDrift(bool drift) { }
            public void SetDriver(bool hasDriver) { }
        }

        private static readonly CarId Car = new CarId(1);
        private EventBus _bus;
        private GameObject _clockObject;
        private GameObject _carObject;
        private ShiftConfig _config;
        private BlessingConfig _blessingConfig;
        private ShiftBlessings _blessings;
        private TicketRegistry _registry;
        private ShiftClock _clock;
        private BlessingPops _pops;
        private BlessingPopTracker _tracker;
        private readonly List<CurseApplied> _curses = new List<CurseApplied>();
        private readonly List<TicketTimerExpired> _expired = new List<TicketTimerExpired>();

        [SetUp]
        public void SetUp()
        {
            Services.Reset();
            _curses.Clear();
            _expired.Clear();
            _bus = new EventBus();
            Services.Register<IEventBus>(_bus);
            var tuning = new TuningStore(_bus);
            OvertimeCurseRows.Seed(tuning);
            Services.Register<ITuning>(tuning);
            _config = ScriptableObject.CreateInstance<ShiftConfig>();
            _blessingConfig = ScriptableObject.CreateInstance<BlessingConfig>();
            _clockObject = new GameObject("late-pickup-clock");
            _clock = _clockObject.AddComponent<ShiftClock>();
            _clock.Initialize(_bus, _config);
            _blessings = new ShiftBlessings(_bus, _config);
            _registry = new TicketRegistry(_bus, _clock, new BlessingCalculator(_blessingConfig, null, ModifierDefaults.CreateCatalog()), _blessings);
            _carObject = new GameObject("late-car");
            _carObject.transform.position = new Vector3(3f, 4f, 0f);
            var settings = new BlessingPopSettings();
            _pops = new BlessingPops(settings);
            _tracker = new BlessingPopTracker(_bus, new OneCar(new Body(Car, _carObject.transform)), _pops, settings, () => Time.time);
            _bus.Subscribe<CurseApplied>(e => _curses.Add(e));
            _bus.Subscribe<TicketTimerExpired>(e => _expired.Add(e));
        }

        [TearDown]
        public void TearDown()
        {
            _tracker.Dispose();
            _registry.Dispose();
            _blessings.Dispose();
            Object.Destroy(_clockObject);
            Object.Destroy(_carObject);
            Object.DestroyImmediate(_config);
            Object.DestroyImmediate(_blessingConfig);
            Services.Reset();
        }

        private void RunToExpiry()
        {
            _clock.StartShift();
            _bus.Publish(new BlessingEarned(TicketId.None, 5, false));
            var ticket = _registry.Open(Car, new PatronRequest(5f, 20, false, 4f));
            _bus.Publish(new CarSpawned(Car, ticket.TicketId));
            _bus.Publish(new PatronEnteredDiner(Car));
            _bus.Publish(new CarLeftPickup(Car));
            _bus.Publish(new CarParked(Car, 1));
            for (var i = 0; i < 12 && _expired.Count == 0; i++) _clock.Advance(1f);
        }

        [UnityTest]
        public IEnumerator ALateCarAtTheCurb_CostsTheCurse_DropsTheTotalAndPopsRed()
        {
            RunToExpiry();
            Assert.AreEqual(1, _expired.Count, "the pickup timer ran out");
            yield return new WaitForSeconds(1.2f);
            _bus.Publish(new CarAtCurb(Car));
            Assert.AreEqual(1, _curses.Count, "delivery of the late car costs a curse");
            Assert.GreaterOrEqual(_curses[0].Amount, 1);
            Assert.AreEqual(_expired[0].Ticket, _curses[0].Ticket, "same ticket id end to end");
            Assert.AreEqual(5 - _curses[0].Amount, _blessings.Blessings);
            _bus.Publish(new PatronBoarded(Car));
            Assert.AreEqual(1, _pops.ActiveCount);
            Assert.AreEqual("-" + _curses[0].Amount, _pops[0].Text);
            Assert.AreEqual(BlessingPopKind.Curse, _pops[0].Kind);
        }

        [UnityTest]
        public IEnumerator ADeliveryInTheFrameTheTimerRanOut_StillCostsOne()
        {
            RunToExpiry();
            _bus.Publish(new CarAtCurb(Car));
            Assert.AreEqual(1, _curses.Count);
            Assert.AreEqual(1, _curses[0].Amount);
            yield return null;
        }

        [UnityTest]
        public IEnumerator ATicketVoidedByTheShiftEnding_CostsNothingAndPopsNothing()
        {
            RunToExpiry();
            yield return new WaitForSeconds(0.3f);
            _bus.Publish(new ShiftEnded());
            Assert.AreEqual(0, _curses.Count);
            Assert.AreEqual(0, _pops.ActiveCount);
        }
    }
}
