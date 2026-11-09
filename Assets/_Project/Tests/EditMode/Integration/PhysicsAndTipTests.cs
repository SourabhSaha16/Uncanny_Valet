using NUnit.Framework;
using UncannyValet.Cars;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Economy;
using UncannyValet.Lot;
using UncannyValet.Patrons;
using UnityEngine;

namespace UncannyValet.Tests.Integration
{
    /// <summary>Driving maths into parking into tips: the numbers must line up across modules.</summary>
    public class PhysicsAndTipTests
    {
        private ShiftHarness _h;

        [SetUp] public void SetUp() { _h = new ShiftHarness(); _h.StartShift(); }
        [TearDown] public void TearDown() { _h.Dispose(); }

        [Test]
        public void ParkingByPhysics_CoastToStop_SettlesOnceAndParksTheTicket()
        {
            var t = _h.Spawn(new PatronRequest(10f, 20, false));
            const float dt = 0.02f;
            float speed = 0f;
            for (var i = 0; i < 500; i++) speed = CarMath(speed, 1f, dt);
            Assert.AreEqual(10f, speed, 0.001f);
            var timer = new StopTimer(0.5f);
            var settled = 0;
            for (var i = 0; i < 2000; i++)
            {
                speed = CarMath(speed, 0f, dt);
                if (timer.Tick(dt, speed < 0.2f)) settled++;
            }
            Assert.AreEqual(1, settled);
            if (settled == 1) _h.Park(t, 4);
            Assert.AreEqual(TicketState.Parked, t.State);
        }

        private static float CarMath(float speed, float throttle, float dt)
            => CarPhysicsMath.NextForwardSpeed(speed, throttle, 8f, 20f, 4f, 10f, 0.5f, dt);

        [Test]
        public void TipNeverBelowZero_ForAnyWaitAndBase()
        {
            var calc = new TipCalculator(ScriptableObject.CreateInstance<TipConfig>());
            for (var baseTip = -10; baseTip <= 60; baseTip += 5)
            for (var wait = 0f; wait <= 2000f; wait += 7.5f)
            {
                Assert.GreaterOrEqual(calc.Calculate(new TipInput(baseTip, wait, false)), 0);
                Assert.GreaterOrEqual(calc.Calculate(new TipInput(baseTip, wait, true)), 0);
            }
        }

        [Test]
        public void EveryTipOnTheBus_IsNonNegative_AndScoreMatchesTheirSum()
        {
            var sum = 0;
            _h.Bus.Subscribe<TipEarned>(e => { Assert.GreaterOrEqual(e.Amount, 0); sum += e.Amount; });
            for (var i = 0; i < 5; i++)
            {
                var t = _h.Spawn(new PatronRequest(5f, 10 + i * 5, i % 2 == 0));
                var spot = i % 4 + 1;
                _h.Park(t, spot);
                _h.Minutes(5 + i * 12);
                _h.LeaveSpot(t, spot);
                _h.AtCurb(t);
            }
            Assert.AreEqual(sum, _h.Score.Score);
            Assert.AreEqual(0, _h.Registry.OpenCount);
        }
    }
}
