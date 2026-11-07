using NUnit.Framework;
using UnityEngine;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Economy;
using UncannyValet.Night;

namespace UncannyValet.Tests.Night
{
    public class RunLossTests
    {
        private const int Nights = 7;
        private const int Debt = 100;

        private sealed class FakeDebt : IDebtView
        {
            public int DebtRemaining => Debt;
            public int TipsTonight => 0;
        }

        private EventBus _bus;
        private GameObject _go;
        private NightClock _clock;
        private NightTallyRecorder _recorder;
        private RunLossGuard _guard;
        private int _ended;
        private int _summaries;
        private NightSummary _summary;

        [SetUp]
        public void SetUp()
        {
            _bus = new EventBus();
            _go = new GameObject("clock");
            _clock = _go.AddComponent<NightClock>();
            _clock.Initialize(_bus, 0, 1f);
            _recorder = new NightTallyRecorder(_bus, new FakeDebt(), Nights, n => "next");
            _guard = new RunLossGuard(_bus, _clock);
            _ended = 0;
            _summaries = 0;
            _bus.Subscribe<NightEnded>(_ => _ended++);
            _bus.Subscribe<NightSummaryReady>(e => { _summaries++; _summary = e.Summary; });
        }

        [TearDown]
        public void TearDown()
        {
            _guard.Dispose();
            _recorder.Dispose();
            Object.DestroyImmediate(_go);
        }

        private void Tip(int amount, int n = 1)
        {
            var ticket = new Core.Ids.TicketId(n);
            var car = new Core.Ids.CarId(n);
            _bus.Publish(new TicketOpened(ticket, car, 10f));
            _bus.Publish(new CarParked(car, 1));
            _bus.Publish(new TicketClosed(ticket, car, 1f, amount));
        }

        [Test]
        public void FinalStrikeMidNight_EndsOnceWithRunLost()
        {
            _clock.StartNight(2);
            _clock.Advance(10f);
            Tip(15);
            _bus.Publish(new StrikeGiven(3, 3));
            _clock.Advance(0f);
            _clock.Advance(1f);

            Assert.AreEqual(1, _ended);
            Assert.AreEqual(1, _summaries);
            Assert.IsFalse(_clock.IsRunning);
            Assert.IsTrue(_summary.RunLost);
            Assert.IsTrue(_summary.IsFinalNight);
            Assert.IsFalse(_summary.PaidOff);
            Assert.AreEqual(15, _summary.Tips);
            Assert.AreEqual("", _summary.NextNightNote);
        }

        [Test]
        public void NonFinalStrike_DoesNothing()
        {
            _clock.StartNight(1);
            _bus.Publish(new StrikeGiven(1, 3));
            _clock.Advance(5f);

            Assert.AreEqual(0, _ended);
            Assert.IsTrue(_clock.IsRunning);
        }

        [Test]
        public void FinalStrikeWhileNotRunning_IsIgnored()
        {
            _clock.StartNight(1);
            _clock.Advance(180f);
            Assert.AreEqual(1, _ended);

            _bus.Publish(new StrikeGiven(3, 3));
            _clock.Advance(1f);

            Assert.AreEqual(1, _ended);
            Assert.AreEqual(1, _summaries);
            Assert.IsFalse(_summary.RunLost);
        }

        [Test]
        public void SecondFinalStrike_DoesNotEndTwice()
        {
            _clock.StartNight(1);
            _bus.Publish(new StrikeGiven(3, 3));
            _bus.Publish(new StrikeGiven(4, 3));
            _clock.Advance(1f);
            _bus.Publish(new StrikeGiven(5, 3));
            _clock.Advance(1f);

            Assert.AreEqual(1, _ended);
            Assert.AreEqual(1, _summaries);
        }

        [Test]
        public void NewRunAfterLoss_IsNotLostAndRunsFullLength()
        {
            _clock.StartNight(3);
            _bus.Publish(new StrikeGiven(3, 3));
            _clock.Advance(1f);

            _clock.StartNight(1);
            _clock.Advance(10f);
            Assert.IsTrue(_clock.IsRunning);
            _clock.Advance(500f);

            Assert.AreEqual(2, _ended);
            Assert.IsFalse(_summary.RunLost);
        }

        [Test]
        public void RunLostSummary_NeverReadsAsPaidOffEvenWithBigTips()
        {
            _clock.StartNight(2);
            Tip(Debt + 50);
            _bus.Publish(new StrikeGiven(3, 3));
            _clock.Advance(1f);

            Assert.IsTrue(_summary.RunLost);
            Assert.IsFalse(_summary.PaidOff);
        }
    }
}
