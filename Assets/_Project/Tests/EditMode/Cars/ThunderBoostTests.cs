using NUnit.Framework;
using UncannyValet.Cars;
using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UnityEngine;

namespace UncannyValet.Tests.Cars
{
    public class ThunderBoostTests
    {
        private EventBus _bus;
        private CarDriver _car;
        private ThunderBoostLink _link;
        private FakeFactory _factory;
        private FakePowers _powers;

        private sealed class FakePowers : IPowerView
        {
            public bool TryGetState(StoryPower power, out PowerState state) { state = default; return false; }

            public bool TryGetValues(StoryPower power, int level, out PowerLevelValues values)
            {
                values = new PowerLevelValues(4f, level == 2 ? 2.2f : 1.5f, 5f, 1.5f, 2f, 0.2f);
                return power == StoryPower.ThunderStride;
            }
        }

        private sealed class FakeFactory : ICarFactory
        {
            public CarDriver Car;
            public IDrivable Spawn(CarId id, Vector2 position, float rotationDegrees) => null;
            public IDrivable SpawnArriving(CarId id, Vector2 stopPosition, float rotationDegrees, int arrivalSlot) => null;
            public void DriveAwayAndDespawn(CarId id) { }
            public bool TryDriveTo(CarId id, Vector2 target, float speed) => false;
            public bool TryGet(CarId id, out IDrivable car) { car = Car; return Car != null; }
            public void Despawn(CarId id) { }
        }

        [SetUp]
        public void SetUp()
        {
            Services.Reset();
            _bus = new EventBus();
            Services.Register<IEventBus>(_bus);
            _car = new GameObject("car", typeof(Rigidbody2D), typeof(CarDriver)).GetComponent<CarDriver>();
            _car.Initialize(new CarId(3), null);
            _factory = new FakeFactory { Car = _car };
            _powers = new FakePowers();
            _link = new ThunderBoostLink(_bus, _factory, () => _powers);
        }

        [TearDown]
        public void TearDown()
        {
            _link.Dispose();
            Object.DestroyImmediate(_car.gameObject);
            Services.Reset();
        }

        [Test]
        public void APlainInputHasNoBoostScales()
        {
            var input = new DriveInput(1f, 0f, false);
            Assert.AreEqual(1f, input.TopSpeedScale);
            Assert.AreEqual(1f, input.AccelerationScale);
            Assert.AreEqual(1f, default(DriveInput).TopSpeedScale, "a parked car's default input is not boosted");
        }

        [Test]
        public void TheModifierMultipliesTopSpeedAndAccelerationAndKeepsTheSteering()
        {
            var boosted = new ThunderBoostModifier(1.5f, 2f).Modify(new DriveInput(0.5f, -1f, true));
            Assert.AreEqual(1.5f, boosted.TopSpeedScale, 0.0001f);
            Assert.AreEqual(2f, boosted.AccelerationScale, 0.0001f);
            Assert.AreEqual((0.5f, -1f, true), (boosted.Throttle, boosted.Steer, boosted.Drift));
        }

        [Test]
        public void TheBoostSitsOnTheDrivenCarFromActivationUntilThePowerEnds()
        {
            _bus.Publish(new CarEntered(new CarId(3)));
            _bus.Publish(new PowerActivated(StoryPower.ThunderStride, 1));
            Assert.IsTrue(_link.IsBoosting);
            Assert.AreEqual(1, _car.Modifiers.Count);
            var input = _car.Modifiers.Apply(new DriveInput(1f, 0f, false));
            Assert.AreEqual(1.5f, input.TopSpeedScale, 0.0001f);
            Assert.AreEqual(2f, input.AccelerationScale, 0.0001f);
            _bus.Publish(new PowerEnded(StoryPower.ThunderStride));
            Assert.IsFalse(_link.IsBoosting);
            Assert.AreEqual(0, _car.Modifiers.Count);
        }

        [Test]
        public void OnFootNothingIsBoosted_AndGettingOutEndsTheBoost()
        {
            _bus.Publish(new PowerActivated(StoryPower.ThunderStride, 1));
            Assert.AreEqual(0, _car.Modifiers.Count);
            _bus.Publish(new CarEntered(new CarId(3)));
            _bus.Publish(new PowerActivated(StoryPower.FreezeTime, 1));
            Assert.AreEqual(0, _car.Modifiers.Count, "another power is not the boost");
            _bus.Publish(new PowerActivated(StoryPower.ThunderStride, 2));
            Assert.AreEqual(1, _car.Modifiers.Count);
            _bus.Publish(new CarExited(new CarId(3)));
            Assert.AreEqual(0, _car.Modifiers.Count);
        }

        [Test]
        public void TheModifierSwitchesOffTheFalloffAndScalesGripLoss()
        {
            var boosted = new ThunderBoostModifier(1.5f, 2f, 0.4f, 0f).Modify(new DriveInput(1f, 0f, false));
            Assert.AreEqual(0.4f, boosted.GripLossScale, 0.0001f);
            Assert.AreEqual(0f, boosted.TurnFalloffScale, 0.0001f);
            var plain = new DriveInput(1f, 0f, false);
            Assert.AreEqual(1f, plain.TurnFalloffScale);
            Assert.AreEqual(1f, default(DriveInput).TurnFalloffScale);
        }

        [Test]
        public void ScaledFalloffTurnsLikeLowSpeedWhenOffAndNormallyWhenOne()
        {
            const float rate = 2f, full = 6f, fall = 0.12f, floor = 0.55f, speed = 12f;
            var slow = SteeringModel.TurnRate(1f, full, rate, full, fall, floor, 1f);
            Assert.AreEqual(slow, SteeringModel.TurnRate(1f, speed, rate, full, fall, floor, 0f), 0.0001f, "no fall-off: full rate at speed");
            Assert.AreEqual(SteeringModel.TurnRate(1f, speed, rate, full, fall, floor),
                SteeringModel.TurnRate(1f, speed, rate, full, fall, floor, 1f), 0.0001f);
            Assert.Less(Mathf.Abs(SteeringModel.TurnRate(1f, speed, rate, full, fall, floor, 1f)), Mathf.Abs(slow));
        }

        [Test]
        public void TheGripScaleReachesTheDriftModelAndNormalReturnsAfterTheBoost()
        {
            var s = new DriftSettings(8f, 0.6f, 3f, 6f, 0.75f, 0.2f, 1f);
            Assert.AreEqual(0.75f, DriftModel.Grip(10f, false, s), 0.0001f);
            Assert.AreEqual(1f - 0.25f * 0.4f, DriftModel.Grip(10f, false, s, 0.4f), 0.0001f);
            _bus.Publish(new CarEntered(new CarId(3)));
            _bus.Publish(new PowerActivated(StoryPower.ThunderStride, 1));
            Assert.AreEqual(1f, _car.Modifiers.Apply(new DriveInput(1f, 0f, false)).TurnFalloffScale, 0.0001f, "no live handling on this car: unchanged");
            _bus.Publish(new PowerEnded(StoryPower.ThunderStride));
            var after = _car.Modifiers.Apply(new DriveInput(1f, 0f, false));
            Assert.AreEqual((1f, 1f, 1f, 1f), (after.TopSpeedScale, after.AccelerationScale, after.GripLossScale, after.TurnFalloffScale));
        }

        [Test]
        public void LiveHandlingCarriesTheConfiguredBoostScalesAndTheLinkUsesThem()
        {
            var config = ScriptableObject.CreateInstance<CarHandlingConfig>();
            _car.Initialize(new CarId(3), null, new CarHandlingLive(config));
            _bus.Publish(new CarEntered(new CarId(3)));
            _bus.Publish(new PowerActivated(StoryPower.ThunderStride, 1));
            var boosted = _car.Modifiers.Apply(new DriveInput(1f, 0f, false));
            Assert.AreEqual(config.ThunderGripLossKept, boosted.GripLossScale, 0.0001f);
            Assert.AreEqual(config.ThunderTurnFalloffKept, boosted.TurnFalloffScale, 0.0001f);
            Assert.Less(boosted.GripLossScale, 1f);
            _bus.Publish(new PowerEnded(StoryPower.ThunderStride));
            Assert.AreEqual(1f, _car.Modifiers.Apply(new DriveInput(1f, 0f, false)).GripLossScale, 0.0001f);
            Object.DestroyImmediate(config);
        }
    }
}
