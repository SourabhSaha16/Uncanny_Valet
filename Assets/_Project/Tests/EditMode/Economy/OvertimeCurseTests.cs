using NUnit.Framework;
using UnityEngine;
using UncannyValet.Core.Config;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Economy;

namespace UncannyValet.Tests.Economy
{
    /// <summary>An angry patron (pickup timer ran out) costs a curse on delivery that grows with the overtime, up to a cap. Issue 166.</summary>
    public class OvertimeCurseTests
    {
        private static readonly TicketId T = new TicketId(1);

        [Test]
        public void OnTime_CostsNothing()
        {
            var rule = new OvertimeCurseRule();
            Assert.AreEqual(0, rule.Take(T, 50f, 10f, 3));
            Assert.AreEqual(0, OvertimeCurseRule.Amount(0f, 10f, 3));
            Assert.AreEqual(0, OvertimeCurseRule.Amount(-4f, 10f, 3));
        }

        [TestCase(1f, 1)]
        [TestCase(4f, 1)]
        [TestCase(14f, 1)]
        [TestCase(15f, 2)]
        [TestCase(20f, 2)]
        [TestCase(25f, 3)]
        [TestCase(30f, 3)]
        public void Scales_RoundsAndNeverBelowOne(float overtime, int expected)
            => Assert.AreEqual(expected, OvertimeCurseRule.Amount(overtime, 10f, 3));

        [Test]
        public void IsCapped() => Assert.AreEqual(3, OvertimeCurseRule.Amount(500f, 10f, 3));

        [Test]
        public void Take_MeasuresFromExpiry_AndForgetsTheTicket()
        {
            var rule = new OvertimeCurseRule();
            rule.MarkExpired(T, 10f);
            Assert.AreEqual(2, rule.Take(T, 30f, 10f, 3));
            Assert.AreEqual(0, rule.Take(T, 60f, 10f, 3));
        }

        [Test]
        public void Take_InTheFrameTheTimerRanOut_StillCostsTheMinimum()
        {
            var rule = new OvertimeCurseRule();
            rule.MarkExpired(T, 10f);
            Assert.AreEqual(1, rule.Take(T, 10f, 10f, 3));
        }

        [Test]
        public void ForgetAndReset_DropPendingTickets()
        {
            var rule = new OvertimeCurseRule();
            rule.MarkExpired(T, 0f);
            rule.Forget(T);
            Assert.AreEqual(0, rule.Take(T, 90f, 10f, 3));
            rule.MarkExpired(T, 0f);
            rule.Reset();
            Assert.AreEqual(0, rule.Take(T, 90f, 10f, 3));
        }

        [Test]
        public void ShiftBlessings_ExpiryCostsNothing_TheCurseLandsOnDelivery()
        {
            var bus = new EventBus();
            var config = ScriptableObject.CreateInstance<ShiftConfig>();
            var now = 0f;
            var blessings = new ShiftBlessings(bus, config) { Clock = () => now };
            var curses = new System.Collections.Generic.List<int>();
            bus.Subscribe<CurseApplied>(c => curses.Add(c.Amount));
            bus.Publish(new ShiftStarted());
            bus.Publish(new TicketTimerExpired(T, new CarId(1), TimerKind.Pickup));
            Assert.AreEqual(0, curses.Count);
            now = 21f;
            bus.Publish(new CarDelivered(T, new CarId(1)));
            CollectionAssert.AreEqual(new[] { 5 }, curses);
            bus.Publish(new CarDelivered(T, new CarId(1)));
            Assert.AreEqual(1, curses.Count);
            blessings.Dispose();
            Object.DestroyImmediate(config);
        }

        [Test]
        public void ShiftBlessings_OnTimeDelivery_AddsNoCurse()
        {
            var bus = new EventBus();
            var config = ScriptableObject.CreateInstance<ShiftConfig>();
            var blessings = new ShiftBlessings(bus, config);
            bus.Publish(new ShiftStarted());
            bus.Publish(new CarDelivered(T, new CarId(1)));
            Assert.AreEqual(0, blessings.Curses);
            blessings.Dispose();
            Object.DestroyImmediate(config);
        }
    }
}
