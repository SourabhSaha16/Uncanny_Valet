using NUnit.Framework;
using UnityEngine;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Economy;
using UncannyValet.Patrons;

namespace UncannyValet.Tests.Patrons
{
    /// <summary>CarDamaged events (the real bus) lower the tip of that car's ticket at payout, in the calculator's damage stages.</summary>
    public class DamageTipTests
    {
        private sealed class FakeClock : IGameClock
        {
            public float SecondsRemaining => 0f;
            public float ShiftLengthSeconds => 0f;
            public float ElapsedMinutes { get; set; }
            public bool IsRunning => true;
        }

        private EventBus _bus;
        private FakeClock _clock;
        private TipConfig _config;
        private TicketRegistry _registry;
        private readonly System.Collections.Generic.Dictionary<int, TipBreakdown> _tips = new System.Collections.Generic.Dictionary<int, TipBreakdown>();
        private readonly CarId _damaged = new CarId(1);
        private readonly CarId _clean = new CarId(2);

        [SetUp]
        public void SetUp()
        {
            _bus = new EventBus(); _clock = new FakeClock();
            _config = ScriptableObject.CreateInstance<TipConfig>();
            _registry = new TicketRegistry(_bus, _clock, new TipCalculator(_config, null, ModifierTestKit.Catalog()));
            _bus.Subscribe<TipExplained>(e => _tips[e.Car.Value] = e.Breakdown);
        }

        [TearDown]
        public void TearDown()
        {
            _registry.Dispose();
            Object.DestroyImmediate(_config);
        }

        private void OpenParked(CarId car)
        {
            _registry.Open(car, new PatronRequest(20f, 10, false, 60f));
            _bus.Publish(new CarParked(car, car.Value));
        }

        private void DeliverBoth()
        {
            _clock.ElapsedMinutes = 20f;
            _bus.Publish(new ShiftTick(20f));
            _bus.Publish(new CarAtCurb(_damaged));
            _bus.Publish(new CarAtCurb(_clean));
        }

        [Test]
        public void HalfDamage_PaysHalfOfTheSameCarsUndamagedTip()
        {
            OpenParked(_damaged); OpenParked(_clean);
            _bus.Publish(new CarDamaged(_damaged, 0.5f, 0.5f));
            DeliverBoth();

            var damaged = _tips[_damaged.Value];
            var clean = _tips[_clean.Value];
            Assert.Greater(clean.Total, 1);
            Assert.AreEqual(0, clean.DamageAdjust);
            Assert.Less(damaged.DamageAdjust, 0);
            Assert.LessOrEqual(Mathf.Abs(damaged.Total * 2 - clean.Total), 1, "50% damage takes two stages of 25% off");
        }

        [Test]
        public void ATapBelowFirstStage_CostsNothing_AndLaterHitsAddUp()
        {
            OpenParked(_damaged);
            _bus.Publish(new CarDamaged(_damaged, 0.1f, 0.1f));
            _bus.Publish(new CarDamaged(_damaged, 0.3f, 0.2f));
            _clock.ElapsedMinutes = 20f;
            _bus.Publish(new ShiftTick(20f));
            _bus.Publish(new CarAtCurb(_damaged));
            Assert.Less(_tips[_damaged.Value].DamageAdjust, 0, "30% damage is past the first 25% stage");
        }

        [Test]
        public void AWreckedCar_PaysNothing()
        {
            OpenParked(_damaged);
            _bus.Publish(new CarDamaged(_damaged, 1f, 0.4f));
            _clock.ElapsedMinutes = 20f;
            _bus.Publish(new ShiftTick(20f));
            _bus.Publish(new CarAtCurb(_damaged));
            Assert.AreEqual(0, _tips[_damaged.Value].Total);
            Assert.AreEqual(TipZeroReason.Wrecked, _tips[_damaged.Value].ZeroReason);
        }

        [Test]
        public void TheLedgerForgetsAPaidOrVoidedCar_AndEveryCarWhenAShiftStarts()
        {
            var ledger = new DamageLedger(_bus);
            _bus.Publish(new CarDamaged(_damaged, 0.4f, 0.4f));
            _bus.Publish(new CarDamaged(_clean, 0.2f, 0.2f));
            Assert.AreEqual(0.4f, ledger.Take(_damaged), 1e-5f);
            Assert.AreEqual(0f, ledger.Damage01(_damaged));
            ledger.Clear(_clean);
            Assert.AreEqual(0f, ledger.Damage01(_clean));
            _bus.Publish(new CarDamaged(_clean, 0.6f, 0.6f));
            _bus.Publish(new ShiftStarted());
            Assert.AreEqual(0f, ledger.Damage01(_clean));
            ledger.Dispose();
        }
    }
}
