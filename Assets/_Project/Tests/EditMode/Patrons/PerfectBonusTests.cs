using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Economy;
using UncannyValet.Patrons;
using UncannyValet.Tests.Integration;

namespace UncannyValet.Tests.Patrons
{
    public class PerfectBonusTests
    {
        private sealed class FakeClock : IGameClock
        {
            public float SecondsRemaining => 0f;
            public float ShiftLengthSeconds => 0f;
            public float ElapsedMinutes { get; set; }
            public bool IsRunning => true;
        }

        private sealed class FakeScore : IShiftScoreView
        {
            public int ComboMultiplier { get; set; } = 1;
            public int Score => 0;
            public int Complaints => 0;
            public int Stars => 0;
            public int OneStarScore => 0;
            public int TwoStarScore => 0;
            public int ThreeStarScore => 0;
        }

        private EventBus _bus;
        private FakeClock _clock;
        private FakeScore _score;
        private TipConfig _config;
        private TicketRegistry _registry;
        private TipExplained _explained;
        private readonly CarId _car = new CarId(1);
        private readonly CarId _other = new CarId(2);

        [SetUp]
        public void SetUp()
        {
            _bus = new EventBus(); _clock = new FakeClock(); _score = new FakeScore();
            _config = ScriptableObject.CreateInstance<TipConfig>();
            _registry = new TicketRegistry(_bus, _clock, new TipCalculator(_config, null, ModifierTestKit.Catalog()), _score);
            _bus.Subscribe<TipExplained>(e => _explained = e);
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_config);

        private Ticket OpenParked(CarId car)
        {
            var t = _registry.Open(car, new PatronRequest(20f, 10, false, 60f));
            _bus.Publish(new CarParked(car, 1));
            return t;
        }

        private void Deliver(CarId car)
        {
            _clock.ElapsedMinutes = 20f;
            _bus.Publish(new ShiftTick(20f));
            _bus.Publish(new CarAtCurb(car));
        }

        [Test]
        public void PerfectPark_AddsDefaultBonusToThatTicketsTip()
        {
            var t = OpenParked(_car);
            _bus.Publish(new PerfectPark(_car, t.TicketId));
            Deliver(_car);

            Assert.AreEqual(_config.PerfectParkBonus, _explained.Breakdown.PerfectBonus);
            Assert.AreEqual(2, _config.PerfectParkBonus);
            Assert.AreEqual(_explained.Breakdown.Total, _explained.Breakdown.BaseTip + _explained.Breakdown.SpeedAdjust
                + _explained.Breakdown.PatienceBonus + _explained.Breakdown.PerfectBonus);
        }

        [Test]
        public void PerfectPark_IsMultipliedByTheCombo()
        {
            _score.ComboMultiplier = 3;
            var t = OpenParked(_car);
            _bus.Publish(new PerfectPark(_car, t.TicketId));
            Deliver(_car);
            var b = _explained.Breakdown;

            Assert.AreEqual(3, b.ComboMultiplier);
            Assert.AreEqual((b.BaseTip + b.SpeedAdjust + b.PatienceBonus + b.PerfectBonus) * 3, b.Total);
        }

        [Test]
        public void BonusSurvivesUntilPickup_AndOnlyBelongsToItsTicket()
        {
            var a = OpenParked(_car);
            OpenParked(_other);
            _bus.Publish(new PerfectPark(_car, a.TicketId));
            Deliver(_other);
            Assert.AreEqual(0, _explained.Breakdown.PerfectBonus);
            Deliver(_car);
            Assert.AreEqual(2, _explained.Breakdown.PerfectBonus);
        }

        [Test]
        public void EachPerfectParkEventPaysOnce_NoEventNoBonus()
        {
            var t = OpenParked(_car);
            _bus.Publish(new PerfectPark(_car, t.TicketId));
            _bus.Publish(new CarParked(_car, 1));
            Deliver(_car);
            Assert.AreEqual(2, _explained.Breakdown.PerfectBonus);

            var second = OpenParked(_other);
            _bus.Publish(new PerfectPark(_other, second.TicketId));
            _bus.Publish(new PerfectPark(_other, second.TicketId));
            _clock.ElapsedMinutes = 40f;
            _bus.Publish(new ShiftTick(40f));
            _bus.Publish(new CarAtCurb(_other));
            Assert.AreEqual(4, _explained.Breakdown.PerfectBonus);
        }

        [Test]
        public void TicketlessPerfectPark_IsIgnored_AndVoidedTicketForgetsItsBonus()
        {
            var ledger = new PerfectBonusLedger(_bus);
            _bus.Publish(new PerfectPark(_car, TicketId.None));
            Assert.AreEqual(0, ledger.Count(TicketId.None));

            var t = OpenParked(_car);
            _bus.Publish(new PerfectPark(_car, t.TicketId));
            _bus.Publish(new ShiftEnded());
            Assert.AreEqual(0, _explained.Breakdown.PerfectBonus);
            Assert.AreEqual(1, ledger.Count(t.TicketId));
            ledger.Clear(t.TicketId);
            Assert.AreEqual(0, ledger.Take(t.TicketId));
            ledger.Dispose();
        }

        [Test]
        public void ZeroTip_StaysZeroWithPerfectBonus()
        {
            var t = OpenParked(_car);
            _bus.Publish(new PerfectPark(_car, t.TicketId));
            _clock.ElapsedMinutes = 20f;
            _bus.Publish(new ShiftTick(20f));
            _bus.Publish(new ShiftTick(200f));
            _clock.ElapsedMinutes = 200f;
            _bus.Publish(new CarAtCurb(_car));
            Assert.AreEqual(0, _explained.Breakdown.Total);
        }
    }
}
