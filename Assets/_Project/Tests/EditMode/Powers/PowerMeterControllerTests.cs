using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UncannyValet.Core;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Powers;
using UnityEngine;

namespace UncannyValet.Tests.Powers
{
    public class PowerMeterControllerTests
    {
        private GameObject _go;
        private PowerMeterConfig _config;
        private PowerMeterController _controller;
        private EventBus _bus;
        private readonly List<PowerMeterChanged> _changes = new List<PowerMeterChanged>();

        [SetUp]
        public void SetUp()
        {
            _changes.Clear();
            Services.Reset();
            _bus = new EventBus();
            Services.Register<IEventBus>(_bus);
            _bus.Subscribe<PowerMeterChanged>(e => _changes.Add(e));
            _config = ScriptableObject.CreateInstance<PowerMeterConfig>();
            _go = new GameObject("powers");
            _go.SetActive(false);
            _controller = _go.AddComponent<PowerMeterController>();
            typeof(PowerMeterController).GetField("_config", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(_controller, _config);
            _go.SetActive(true);
            typeof(PowerMeterController).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(_controller, null);
            typeof(PowerMeterController).GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(_controller, null);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
            Object.DestroyImmediate(_config);
            Services.Reset();
        }

        [Test]
        public void PerfectPark_FillsTheMeterAndPublishes()
        {
            _bus.Publish(new PerfectPark(new CarId(1), TicketId.None));
            Assert.AreEqual(18f, _controller.Meter.Value, 0.0001f);
            Assert.AreEqual(1, _changes.Count);
        }

        [Test]
        public void PerfectDropOff_FillsTheMeter()
        {
            _bus.Publish(new PerfectDropOff(new CarId(1), TicketId.None));
            Assert.AreEqual(10f, _controller.Meter.Value, 0.0001f);
        }

        [Test]
        public void ShiftStarted_EmptiesTheMeter()
        {
            _bus.Publish(new PerfectPark(new CarId(1), TicketId.None));
            _bus.Publish(new ShiftStarted());
            Assert.AreEqual(0f, _controller.Meter.Value);
        }

        [Test]
        public void Disabled_StopsListening()
        {
            typeof(PowerMeterController).GetMethod("OnDisable", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(_controller, null);
            _bus.Publish(new PerfectPark(new CarId(1), TicketId.None));
            Assert.AreEqual(0f, _controller.Meter.Value);
        }
    }
}
