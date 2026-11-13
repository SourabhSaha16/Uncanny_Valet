using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Audio;
using UncannyValet.Bootstrap;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.UI;
using UncannyValet.UI.Hud;
using UncannyValet.UI.Screens;
using UnityEngine;

namespace UncannyValet.Tests.Integration
{
    public class PerfectFeedbackTests
    {
        private readonly CarId _car = new CarId(4);
        private EventBus _bus;
        private TransformCarFactory _cars;
        private GameObject _carObject;

        [SetUp]
        public void SetUp()
        {
            _bus = new EventBus();
            _cars = new TransformCarFactory();
            _carObject = new GameObject("Car");
            _carObject.transform.position = new Vector3(2f, 3f, 0f);
            _carObject.transform.rotation = Quaternion.Euler(0f, 0f, 90f); // nose points to -x
            _cars.Add(_car, _carObject.transform);
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_carObject);

        [Test]
        public void CameraKick_GoesOutAndBackAlongTheDirection()
        {
            var kick = new CameraKick();
            kick.Start(new Vector2(0f, 2f), 0.05f, 0.08f, 10f);
            Assert.AreEqual(Vector2.zero, kick.Offset(9.9f));
            Assert.AreEqual(0.05f, kick.Offset(10.04f).y, 0.0001f);
            Assert.AreEqual(0f, kick.Offset(10.04f).x, 0.0001f);
            Assert.AreEqual(Vector2.zero, kick.Offset(10.2f), "zero once the kick is over");
        }

        [Test]
        public void CameraKick_WithNoLength_DoesNothing()
        {
            var kick = new CameraKick();
            kick.Start(Vector2.up, 0f, 0.08f, 0f);
            Assert.AreEqual(Vector2.zero, kick.Offset(0.04f));
        }

        [Test]
        public void KickLink_FiresAlongTheCarNoseForBothPerfects()
        {
            var got = new List<Vector2>();
            using (new PerfectKickLink(_bus, _cars, got.Add))
            {
                _bus.Publish(new PerfectPark(_car, TicketId.None));
                _bus.Publish(new PerfectDropOff(_car, TicketId.None));
                _bus.Publish(new PerfectPark(new CarId(99), TicketId.None));
            }
            _bus.Publish(new PerfectPark(_car, TicketId.None));
            Assert.AreEqual(2, got.Count);
            Assert.AreEqual(-1f, got[0].x, 0.001f);
            Assert.AreEqual(0f, got[0].y, 0.001f);
        }

        [Test]
        public void PopTracker_StartsAPerfectPopAboveTheCar()
        {
            var settings = new BlessingPopSettings();
            var pops = new BlessingPops(settings);
            using (new BlessingPopTracker(_bus, _cars, pops, settings, () => 5f))
            {
                _bus.Publish(new PerfectDropOff(_car, TicketId.None));
                Assert.AreEqual(1, pops.ActiveCount);
                Assert.AreEqual(BlessingPopText.PerfectLabel, pops[0].Text);
                Assert.AreEqual(2f, pops[0].World.x, 0.001f);
                Assert.Greater(pops[0].World.y, 3f + settings.LiftWorldUnits);
                _bus.Publish(new PerfectPark(new CarId(99), TicketId.None));
                Assert.AreEqual(1, pops.ActiveCount, "an unknown car makes no pop");
            }
        }

        [Test]
        public void ShiftStats_CountsPerfectParksAndResetsEachShift()
        {
            using (var stats = new ShiftStats(_bus))
            {
                _bus.Publish(new PerfectPark(_car, TicketId.None));
                _bus.Publish(new PerfectPark(_car, TicketId.None));
                Assert.AreEqual(2, stats.PerfectParks);
                _bus.Publish(new ShiftStarted());
                Assert.AreEqual(0, stats.PerfectParks);
            }
        }

        [Test]
        public void Results_ShowPerfectParks()
        {
            var content = ResultsFormatter.Build(new ShiftResult(100, 50, true, 0, 2), 3, 5);
            var line = content.Stats[content.Stats.Length - 1];
            Assert.AreEqual("PERFECT PARKS", line.Label);
            Assert.AreEqual("5", line.Value);
        }

        [Test]
        public void Router_PlaysTheDingForBothPerfects()
        {
            var played = new List<SfxId>();
            var router = new SfxEventRouter(_bus, played.Add);
            router.Subscribe();
            _bus.Publish(new PerfectPark(_car, TicketId.None));
            _bus.Publish(new PerfectDropOff(_car, TicketId.None));
            router.Unsubscribe();
            _bus.Publish(new PerfectPark(_car, TicketId.None));
            CollectionAssert.AreEqual(new[] { SfxId.PerfectDing, SfxId.PerfectDing }, played);
        }
    }
}
