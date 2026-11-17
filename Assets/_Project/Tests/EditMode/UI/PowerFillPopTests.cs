using NUnit.Framework;
using UncannyValet.Core.Events;
using UncannyValet.UI.Hud;

namespace UncannyValet.Tests.UI
{
    public class PowerFillPopTests
    {
        private PowerFillPopSettings _settings;
        private PowerFillPops _pops;

        [SetUp]
        public void SetUp()
        {
            _settings = new PowerFillPopSettings();
            _pops = new PowerFillPops(_settings);
        }

        [TestCase(18f, "+18")]
        [TestCase(4.4f, "+4")]
        [TestCase(4.5f, "+5")]
        [TestCase(21.6f, "+22")]
        public void Amount_IsRoundedToAWholeNumber(float amount, string expected)
        {
            Assert.IsTrue(PowerFillPopRule.TryFormat(amount, out var text));
            Assert.AreEqual(expected, text);
        }

        [TestCase(0f)]
        [TestCase(0.4f)]
        [TestCase(-3f)]
        public void AmountThatRoundsToNothing_ShowsNoPop(float amount)
        {
            Assert.IsFalse(_pops.Start(amount, PowerFillSource.Drift, 0f));
            Assert.AreEqual(0, _pops.ActiveCount);
        }

        [Test]
        public void PopsArrivingTogether_TakeDifferentSlots()
        {
            _pops.Start(18f, PowerFillSource.PerfectPark, 1f);
            _pops.Start(4f, PowerFillSource.ParkedInside, 1f);
            Assert.AreEqual(2, _pops.ActiveCount);
            Assert.AreEqual(0, _pops[0].Slot);
            Assert.AreEqual(1, _pops[1].Slot);
        }

        [Test]
        public void SlotIsFreeAgainAfterTheHold()
        {
            _pops.Start(18f, PowerFillSource.PerfectPark, 1f);
            _pops.Start(5f, PowerFillSource.PerfectDropOff, 1f + _settings.SlotHoldSeconds + 0.01f);
            Assert.AreEqual(0, _pops[1].Slot);
        }

        [Test]
        public void FullPool_ReplacesTheOldest()
        {
            for (var i = 0; i < _pops.Capacity; i++) _pops.Start(10f + i, PowerFillSource.Drift, i * 0.01f);
            _pops.Start(99f, PowerFillSource.PerfectPark, 1f);
            Assert.AreEqual(_pops.Capacity, _pops.ActiveCount);
            Assert.AreEqual("+99", _pops[0].Text);
        }

        [Test]
        public void Expire_RetiresOldPops_AndClearDropsAll()
        {
            _pops.Start(18f, PowerFillSource.PerfectPark, 0f);
            _pops.Expire(_settings.DurationSeconds + 0.1f);
            Assert.AreEqual(0, _pops.ActiveCount);
            _pops.Start(18f, PowerFillSource.PerfectPark, 0f);
            _pops.Clear();
            Assert.AreEqual(0, _pops.ActiveCount);
        }

        [Test]
        public void Tracker_PopsOnPowerFilled_AndClearsOnShiftEdges()
        {
            var bus = new EventBus();
            using (new PowerFillPopTracker(bus, _pops, () => 0f))
            {
                bus.Publish(new PowerFilled(18f, PowerFillSource.PerfectPark));
                bus.Publish(new PowerFilled(4f, PowerFillSource.ParkedInside));
                Assert.AreEqual(2, _pops.ActiveCount);
                bus.Publish(new ShiftEnded());
                Assert.AreEqual(0, _pops.ActiveCount);
                bus.Publish(new PowerFilled(7f, PowerFillSource.Drift));
                bus.Publish(new ShiftStarted());
                Assert.AreEqual(0, _pops.ActiveCount);
            }
            bus.Publish(new PowerFilled(7f, PowerFillSource.Drift));
            Assert.AreEqual(0, _pops.ActiveCount);
        }
    }
}
