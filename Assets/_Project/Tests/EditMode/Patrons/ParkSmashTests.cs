using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Core.Config;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Economy;
using UncannyValet.Patrons;
using UnityEngine;

namespace UncannyValet.Tests.Patrons
{
    public class ParkSmashTests
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
        private ShiftConfig _shift;
        private TipConfig _tipConfig;
        private ShiftScore _score;
        private TicketRegistry _registry;
        private readonly List<ParkSmashCompleted> _completed = new List<ParkSmashCompleted>();
        private readonly List<TipExplained> _explained = new List<TipExplained>();
        private readonly List<int> _closed = new List<int>();
        private readonly List<ParkSmashCarDelivered> _deliveredEvents = new List<ParkSmashCarDelivered>();
        private bool _carDelivered;
        private bool _perfectPark;

        [SetUp]
        public void SetUp()
        {
            _bus = new EventBus();
            _clock = new FakeClock();
            _shift = ScriptableObject.CreateInstance<ShiftConfig>();
            _tipConfig = ScriptableObject.CreateInstance<TipConfig>();
            _score = new ShiftScore(_bus, _shift);
            _registry = new TicketRegistry(_bus, _clock, new TipCalculator(_tipConfig), _score);
            _completed.Clear(); _explained.Clear(); _closed.Clear(); _deliveredEvents.Clear();
            _carDelivered = false; _perfectPark = false;
            _bus.Subscribe<ParkSmashCompleted>(e => _completed.Add(e));
            _bus.Subscribe<TipExplained>(e => _explained.Add(e));
            _bus.Subscribe<TicketClosed>(e => _closed.Add(e.Ticket.Value));
            _bus.Subscribe<ParkSmashCarDelivered>(e => _deliveredEvents.Add(e));
            _bus.Subscribe<CarDelivered>(e => _carDelivered = true);
            _bus.Subscribe<PerfectPark>(e => _perfectPark = true);
            _bus.Publish(new ShiftStarted());
        }

        [TearDown]
        public void TearDown()
        {
            _registry.Dispose();
            _score.Dispose();
            Object.DestroyImmediate(_shift);
            Object.DestroyImmediate(_tipConfig);
        }

        private void Tick(float minute)
        {
            _clock.ElapsedMinutes = minute;
            _bus.Publish(new ShiftTick(minute));
        }

        // Opens a ticket, parks the car, and ticks past the stay so the patron waits for the car.
        private Ticket WaitingPatron(int carId, int baseTip, bool ev = false, bool dirty = false)
        {
            var car = new CarId(carId);
            var t = _registry.Open(car, new PatronRequest(10f, baseTip, false, isEv: ev, isDirty: dirty));
            _bus.Publish(new CarClearedQueue(car));
            _bus.Publish(new CarParked(car, carId));
            Tick(_clock.ElapsedMinutes + 11f);
            Assert.AreEqual(TicketState.PickupRequested, t.State);
            return t;
        }

        [Test]
        public void ParkSmash_ClosesEveryWaitingTicket_WithoutAHandOver()
        {
            WaitingPatron(1, 20);
            WaitingPatron(2, 30);

            _bus.Publish(new PowerActivated(PowerKind.ParkSmash));

            Assert.AreEqual(0, _registry.OpenCount);
            Assert.AreEqual(2, _closed.Count);
            Assert.AreEqual(2, _deliveredEvents.Count);
            Assert.IsFalse(_carDelivered, "a smash is not a hand-over, so the patron vanishes and the car just leaves");
        }

        [Test]
        public void ParkSmash_PaysTheSpeedBonusEvenAfterALongWait()
        {
            WaitingPatron(1, 20);
            Tick(_clock.ElapsedMinutes + _tipConfig.SpeedBonusWindowMinutes + 5f); // late: a normal delivery would be penalised

            _bus.Publish(new PowerActivated(PowerKind.ParkSmash));

            Assert.AreEqual(1, _explained.Count);
            Assert.AreEqual(_tipConfig.SpeedBonus, _explained[0].Breakdown.SpeedAdjust);
            Assert.AreEqual(1, _closed.Count);
        }

        [Test]
        public void ParkSmash_TipsGetTheComboMultiplier_AndTheComboIsNotReset()
        {
            for (var i = 0; i < 3; i++) _bus.Publish(new TipEarned(new TicketId(100 + i), 10));
            var combo = _score.ComboMultiplier;
            Assert.Greater(combo, 1);
            WaitingPatron(1, 20);
            WaitingPatron(2, 20);

            _bus.Publish(new PowerActivated(PowerKind.ParkSmash));

            foreach (var e in _explained) Assert.AreEqual(combo, e.Breakdown.ComboMultiplier, "one multiplier for the whole smash");
            Assert.GreaterOrEqual(_score.ComboMultiplier, combo);
            Assert.AreEqual(30 + _deliveredEvents[0].Tip + _deliveredEvents[1].Tip, _score.Score);
        }

        [Test]
        public void ParkSmash_LeavesTicketsOfParkedCarsAlone()
        {
            var parked = _registry.Open(new CarId(5), new PatronRequest(500f, 20, false));
            _bus.Publish(new CarParked(new CarId(5), 2));
            _bus.Publish(new PowerActivated(PowerKind.ParkSmash));
            Assert.AreEqual(TicketState.Parked, parked.State);
            Assert.AreEqual(1, _registry.OpenCount);
        }

        [Test]
        public void ParkSmash_LeavesACarTheValetIsDrivingAlone()
        {
            var t = WaitingPatron(1, 20);
            _bus.Publish(new CarEntered(new CarId(1)));
            _bus.Publish(new PowerActivated(PowerKind.ParkSmash));
            Assert.AreEqual(TicketState.PickupRequested, t.State);
            Assert.AreEqual(0, _deliveredEvents.Count);
        }

        [Test]
        public void Completed_WaitsForBothHalves_AndCarriesTheTotals()
        {
            WaitingPatron(1, 20);
            WaitingPatron(2, 30);
            _bus.Publish(new PowerActivated(PowerKind.ParkSmash));
            Assert.AreEqual(0, _completed.Count, "the Lot half has not reported yet");

            _bus.Publish(new ParkSmashParked(3, 1, 2));

            Assert.AreEqual(1, _completed.Count);
            var done = _completed[0];
            Assert.AreEqual(3, done.Parked);
            Assert.AreEqual(1, done.Charged);
            Assert.AreEqual(2, done.Washed);
            Assert.AreEqual(2, done.Delivered);
            Assert.AreEqual(_deliveredEvents[0].Tip + _deliveredEvents[1].Tip, done.TotalTips);
        }

        [Test]
        public void Completed_WorksWhenTheLotReportsFirst_AndWithNothingToDo()
        {
            _bus.Publish(new ParkSmashParked(0, 0, 0));
            Assert.AreEqual(0, _completed.Count);
            _bus.Publish(new PowerActivated(PowerKind.ParkSmash));
            Assert.AreEqual(1, _completed.Count);
            Assert.AreEqual(0, _completed[0].Delivered);
            Assert.AreEqual(0, _completed[0].TotalTips);
        }

        [Test]
        public void OtherPowers_DoNothing()
        {
            WaitingPatron(1, 20);
            _bus.Publish(new PowerActivated(PowerKind.FreezeTime));
            Assert.AreEqual(1, _registry.OpenCount);
        }

        [Test]
        public void ParkedCars_GetChargedAndWashed_SoTheirTipsEarnTheBonuses()
        {
            var t = _registry.Open(new CarId(7), new PatronRequest(10f, 20, false, isEv: true, isDirty: true));
            _bus.Publish(new ParkSmashCarParked(new CarId(7), 7, true, true));
            Assert.IsTrue(t.ChargedEv);
            Assert.IsTrue(t.CleanedDirty);
        }

        [Test]
        public void ParkSmashParks_DoNotCountAsPerfectParks()
        {
            _registry.Open(new CarId(8), new PatronRequest(10f, 20, false));
            _bus.Publish(new ParkSmashCarParked(new CarId(8), 1, false, false));
            _bus.Publish(new ParkSmashParked(1, 0, 0));
            Assert.IsFalse(_perfectPark);
        }

        [Test]
        public void SmashStartedMidShift_ResetsOnANewShift()
        {
            _bus.Publish(new ParkSmashParked(2, 0, 0));
            _bus.Publish(new ShiftStarted());
            _bus.Publish(new PowerActivated(PowerKind.ParkSmash));
            Assert.AreEqual(0, _completed.Count, "the stale Lot half from before the new shift is gone");
        }
    }
}
