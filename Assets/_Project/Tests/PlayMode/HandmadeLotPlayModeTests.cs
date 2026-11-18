using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UncannyValet.Cars;
using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Lot;
using UncannyValet.Lot.Handmade;
using UnityEngine;
using UnityEngine.TestTools;

namespace UncannyValet.Tests.PlayMode
{
    /// <summary>A minimal hand-made lot plays: a car parks in a bay, is accepted at the curb, routes end where the markers say, and a failing rule blocks the build.</summary>
    public class HandmadeLotPlayModeTests
    {
        private const float SettleWait = 1.2f;

        private readonly List<CarParked> _parked = new List<CarParked>();
        private readonly List<CarAtCurb> _atCurb = new List<CarAtCurb>();
        private LotConfig _config;
        private GameObject _lot;
        private GameObject _factoryObject;
        private CarFactory _factory;

        [SetUp]
        public void SetUp()
        {
            Services.Reset();
            var bus = new EventBus();
            Services.Register<IEventBus>(bus);
            bus.Subscribe<CarParked>(e => _parked.Add(e));
            bus.Subscribe<CarAtCurb>(e => _atCurb.Add(e));
            _config = ScriptableObject.CreateInstance<LotConfig>();
            _factoryObject = new GameObject("Cars");
            _factoryObject.SetActive(false);
            _factory = _factoryObject.AddComponent<CarFactory>();
            Set(_factory, "_defaultDefinition", ScriptableObject.CreateInstance<CarDefinition>());
            Set(_factory, "_flash", ScriptableObject.CreateInstance<CarFlashConfig>());
            Set(_factory, "_handling", ScriptableObject.CreateInstance<CarHandlingConfig>());
            _factoryObject.SetActive(true);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var car in Object.FindObjectsByType<CarDriver>(FindObjectsSortMode.None)) Object.DestroyImmediate(car.gameObject);
            if (_lot != null) Object.Destroy(_lot);
            Object.Destroy(_factoryObject);
            Object.DestroyImmediate(_config);
            Services.Reset();
        }

        [UnityTest]
        public IEnumerator Lot_BuildsFromMarkers_AndAnswersAsTheLayout()
        {
            _lot = HandmadePlayKit.Build(_config, true);
            yield return null;
            var lot = _lot.GetComponent<HandmadeLot>();
            Assert.IsTrue(lot.IsBuilt);
            Assert.AreEqual(2, lot.RegularSpotCount);
            Assert.AreEqual(2, lot.ArrivalSlots.Count);
            Assert.IsTrue(lot.IsSpotFree(1));
            Assert.IsTrue(lot.TryGetSpot(2, out var bay));
            Assert.AreEqual(new Vector2(10f, 0f), bay.Center);
            var route = new List<Vector2>();
            lot.Road.QueueEntryRoute(1, route);
            Assert.AreEqual(HandmadePlayKit.Slot1, route[route.Count - 1]);
            Assert.AreEqual(2, lot.SpotIds.Count);
        }

        [UnityTest]
        public IEnumerator Car_RestingInABay_PublishesCarParkedOnce()
        {
            _lot = HandmadePlayKit.Build(_config, true);
            yield return null;
            _factory.Spawn(new CarId(1), HandmadePlayKit.Bay1, 0f);
            yield return new WaitForSeconds(SettleWait);
            Assert.AreEqual(1, _parked.Count);
            Assert.AreEqual(1, _parked[0].SpotId);
            Assert.IsFalse(_lot.GetComponent<HandmadeLot>().IsSpotFree(1));
        }

        [UnityTest]
        public IEnumerator Car_FacingUpAtTheCurb_IsAccepted_AndTheArrowsExist()
        {
            _lot = HandmadePlayKit.Build(_config, true);
            yield return null;
            _factory.Spawn(new CarId(2), HandmadePlayKit.CurbCentre, 0f);
            yield return new WaitForSeconds(SettleWait);
            Assert.AreEqual(1, _atCurb.Count);
            Assert.AreEqual(new CarId(2), _atCurb[0].Car);
            var curb = _lot.GetComponentInChildren<CurbZone>();
            Assert.IsTrue(curb.Span.IsMarked, "the pickup arrows are painted");
            Assert.IsNotNull(_lot.GetComponentInChildren<CurbArrow>());
        }

        [UnityTest]
        public IEnumerator FailingRule_BlocksAwake_NothingIsBuilt()
        {
            LogAssert.Expect(LogType.Error, new Regex("exactly one curb"));
            _lot = HandmadePlayKit.Build(_config, false);
            yield return null;
            var lot = _lot.GetComponent<HandmadeLot>();
            Assert.IsFalse(lot.IsBuilt);
            Assert.AreEqual(0, lot.RegularSpotCount);
            Assert.IsNull(lot.Road);
            Assert.IsNull(_lot.GetComponentInChildren<ParkingSpot>(true));
            Assert.IsTrue(HandmadeLotRules.HasErrors(lot.Issues));
        }

        private static void Set(object target, string field, object value)
            => target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    }
}
