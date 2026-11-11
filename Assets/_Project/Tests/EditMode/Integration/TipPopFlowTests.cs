using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Patrons;
using UncannyValet.UI.Hud;
using UnityEngine;

namespace UncannyValet.Tests.Integration
{
    /// <summary>Regression for #67: the tip pop never showed because PatronBoarded sends the car off before the HUD hears it.</summary>
    public class TipPopFlowTests
    {
        private sealed class Clock : IGameClock
        {
            public float SecondsRemaining => 0f;
            public float ShiftLengthSeconds => 0f;
            public float ElapsedMinutes => 0f;
            public bool IsRunning => true;
        }

        private readonly CarId _car = new CarId(7);
        private EventBus _bus;
        private TransformCarFactory _cars;
        private GameObject _carObject;
        private PatronDeparture _departure;

        [SetUp]
        public void SetUp()
        {
            _bus = new EventBus();
            _cars = new TransformCarFactory();
            _carObject = new GameObject("Car");
            _carObject.transform.position = new Vector3(5f, 6f, 0f);
            _cars.Add(_car, _carObject.transform);
            _departure = new PatronDeparture(_bus, _cars, new Clock(), 6f); // subscribes first, as in the real game
        }

        [TearDown]
        public void TearDown()
        {
            _departure.Dispose();
            Object.DestroyImmediate(_carObject);
        }

        [Test]
        public void DeliveredTip_PopsAtTheCarEvenThoughBoardingSendsTheCarOff()
        {
            var settings = new TipPopSettings();
            var pops = new TipPops(settings);
            using (new TipPopTracker(_bus, _cars, pops, settings, () => 1f))
            {
                _bus.Publish(new CarDelivered(TicketId.None, _car));
                _bus.Publish(new TicketClosed(TicketId.None, _car, 3f, 25));
                _bus.Publish(new PatronBoarded(_car));
                Assert.AreEqual(1, pops.ActiveCount);
                Assert.AreEqual("+$25", pops[0].Text);
                Assert.AreEqual(5f, pops[0].World.x, 0.001f);
            }
        }

        [Test]
        public void SettingsFromTheShippedAsset_AreUsable()
        {
            var config = UnityEditor.AssetDatabase.LoadAssetAtPath<UncannyValet.UI.Mood.HudEmotionConfig>(
                "Assets/_Project/Data/UI/HudEmotionConfig.asset");
            Assert.IsNotNull(config);
            Assert.Greater(config.TipPop.MaxPops, 0);
            Assert.Greater(config.TipPop.DurationSeconds, 0f);
        }
    }
}
