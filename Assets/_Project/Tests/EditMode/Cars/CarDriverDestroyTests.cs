using NUnit.Framework;
using UncannyValet.Cars;
using UncannyValet.Core;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UnityEngine;

namespace UncannyValet.Tests.Cars
{
    public class CarDriverDestroyTests
    {
        private int _exited;
        private CarId _lastId;

        [SetUp]
        public void SetUp()
        {
            Services.Reset();
            var bus = new EventBus();
            Services.Register<IEventBus>(bus);
            bus.Subscribe<CarExited>(e => { _exited++; _lastId = e.Car; });
        }

        [TearDown]
        public void TearDown() => Services.Reset();

        private static CarDriver Make(CarId id)
        {
            var go = new GameObject("car", typeof(Rigidbody2D), typeof(CarDriver));
            var driver = go.GetComponent<CarDriver>();
            driver.Initialize(id, null);
            return driver;
        }

        [Test]
        public void DestroyWhileDriven_PublishesCarExitedOnce()
        {
            var car = Make(new CarId(7));
            car.SetDriver(true);
            car.ReleaseDriverOnDestroy();
            Assert.AreEqual(1, _exited);
            Assert.AreEqual(7, _lastId.Value);
            Object.DestroyImmediate(car.gameObject);
        }

        [Test]
        public void ReleaseTwice_PublishesOnce()
        {
            var car = Make(new CarId(9));
            car.SetDriver(true);
            _exited = 0;
            car.ReleaseDriverOnDestroy();
            car.ReleaseDriverOnDestroy();
            Assert.AreEqual(1, _exited);
            Object.DestroyImmediate(car.gameObject);
        }

        [Test]
        public void DestroyWithoutDriver_PublishesNothing()
        {
            var car = Make(new CarId(8));
            car.ReleaseDriverOnDestroy();
            Assert.AreEqual(0, _exited);
            Object.DestroyImmediate(car.gameObject);
        }
    }
}
