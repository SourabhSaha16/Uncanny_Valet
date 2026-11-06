using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Economy;

namespace UncannyValet.Tests.Economy
{
    public class NightTallyTests
    {
        private sealed class FakeDebt : IDebtView
        {
            public int DebtRemaining { get; set; }
            public int TipsTonight { get; set; }
        }

        private EventBus _bus;
        private FakeDebt _debt;
        private NightTallyRecorder _recorder;
        private int _summaries;
        private NightSummary _summary;

        [SetUp]
        public void SetUp()
        {
            _bus = new EventBus();
            _debt = new FakeDebt { DebtRemaining = 100 };
            _recorder = new NightTallyRecorder(_bus, _debt, 7);
            _summaries = 0;
            _bus.Subscribe<NightSummaryReady>(e => { _summaries++; _summary = e.Summary; });
        }

        [TearDown]
        public void TearDown() => _recorder.Dispose();

        private static TicketId T(int v) => new TicketId(v);
        private static CarId C(int v) => new CarId(v);

        private void Open(int n) => _bus.Publish(new TicketOpened(T(n), C(n), 10f));
        private void Park(int n) => _bus.Publish(new CarParked(C(n), 1));
        private void Close(int n, int tip) => _bus.Publish(new TicketClosed(T(n), C(n), 0f, tip));

        [Test]
        public void CountsTipsParkedAndNotParked()
        {
            _bus.Publish(new NightStarted(2));
            Open(1); Open(2); Open(3);
            Park(1); Park(1);
            Close(1, 30);
            Close(2, 0);
            _bus.Publish(new NightEnded(2));

            Assert.AreEqual(1, _summaries);
            Assert.AreEqual(2, _summary.Night);
            Assert.AreEqual(7, _summary.TotalNights);
            Assert.AreEqual(30, _summary.Tips);
            Assert.AreEqual(1, _summary.CarsParked);
            Assert.AreEqual(2, _summary.CarsNotParked);
            Assert.AreEqual(70, _summary.DebtLeft);
        }

        [Test]
        public void DebtLeftNeverBelowZero()
        {
            _bus.Publish(new NightStarted(1));
            Open(1); Park(1); Close(1, 500);
            _bus.Publish(new NightEnded(1));
            Assert.AreEqual(0, _summary.DebtLeft);
            Assert.IsTrue(_summary.PaidOff);
        }

        [Test]
        public void DebtLeftIgnoresLedgerOrder()
        {
            _bus.Publish(new NightStarted(1));
            Open(1); Park(1); Close(1, 40);
            _debt.DebtRemaining = 60;
            _bus.Publish(new NightEnded(1));
            Assert.AreEqual(60, _summary.DebtLeft);
        }

        [Test]
        public void PatronsClosingBeforeOurHandler_CountsOnce()
        {
            _bus.Publish(new NightStarted(1));
            Open(1); Open(2); Park(2);
            Close(1, 0);
            Close(2, 0);
            _bus.Publish(new NightEnded(1));
            Assert.AreEqual(1, _summary.CarsParked);
            Assert.AreEqual(1, _summary.CarsNotParked);
        }

        [Test]
        public void PatronsClosingAfterSummary_IsIgnored()
        {
            _bus.Publish(new NightStarted(1));
            Open(1); Open(2); Park(2);
            _bus.Publish(new NightEnded(1));
            Close(1, 0);
            Close(2, 0);
            Assert.AreEqual(1, _summaries);
            Assert.AreEqual(1, _summary.CarsParked);
            Assert.AreEqual(1, _summary.CarsNotParked);
        }

        [Test]
        public void NewNightResetsCounters()
        {
            _bus.Publish(new NightStarted(1));
            Open(1); Park(1); Close(1, 20);
            _bus.Publish(new NightEnded(1));
            _bus.Publish(new NightStarted(2));
            _bus.Publish(new NightEnded(2));
            Assert.AreEqual(2, _summaries);
            Assert.AreEqual(0, _summary.Tips);
            Assert.AreEqual(0, _summary.CarsParked);
            Assert.AreEqual(0, _summary.CarsNotParked);
        }

        [Test]
        public void DisposedRecorderPublishesNothing()
        {
            _recorder.Dispose();
            _bus.Publish(new NightStarted(1));
            _bus.Publish(new NightEnded(1));
            Assert.AreEqual(0, _summaries);
        }
    }
}
