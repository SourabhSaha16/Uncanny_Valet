using NUnit.Framework;
using UncannyValet.Core.Events;
using UncannyValet.Director;

namespace UncannyValet.Tests.Director
{
    public class ArrivalPacerTests
    {
        private const float Dt = 0.5f;

        private DirectorSettings _settings;
        private ArrivalPacer _pacer;

        [SetUp]
        public void SetUp()
        {
            _settings = new DirectorSettings();
            _pacer = new ArrivalPacer(new MidRandom(), _settings.Pacing, new ReliefGate(_settings.Relief));
        }

        private static PacerInput In(float w = 1f, float t = 1f, float p = 0f, float form = 0f, int angry = 0,
            int queue = 1, int cap = 3, int used = 0, float left = 240f)
            => new PacerInput(w, p, t, form, angry, queue, cap, used, 240f, 240f, 15f, 25f, left);

        // Ticks until the pacer sends a car (or the guard runs out); returns the decision and the seconds waited.
        private PacerDecision RunUntilSpawn(PacerInput input, out float waited, float limit = 300f)
        {
            waited = 0f;
            PacerDecision d = default;
            while (waited < limit)
            {
                d = _pacer.Tick(input, Dt);
                waited += Dt;
                if (d.IsSpawn) break;
            }
            return d;
        }

        [Test]
        public void NoCarIsSentWhenTooLittleOfTheShiftIsLeftToServeIt()
        {
            var cutoff = _settings.Pacing.IdleCutoffSeconds;
            var d = RunUntilSpawn(In(w: 0f, queue: 0, left: cutoff - 1f), out _, limit: 120f);
            Assert.IsFalse(d.IsSpawn, "even an empty queue gets no car this late");
            Assert.AreEqual(HoldReason.ShiftEnding, d.Hold);
        }

        [Test]
        public void ACarIsStillSentJustBeforeTheCutoff()
        {
            Assert.IsTrue(RunUntilSpawn(In(w: 0f, queue: 0, left: _settings.Pacing.IdleCutoffSeconds + 1f), out _).IsSpawn);
        }

        [Test]
        public void IdleLotGetsACarAfterTheIdleSeconds()
        {
            var d = RunUntilSpawn(In(w: 0f), out var waited);
            Assert.IsTrue(d.IsSpawn);
            Assert.AreEqual(ScheduleReason.Idle, d.Reason);
            Assert.AreEqual(_settings.Pacing.IdleSeconds, waited, Dt);
        }

        [Test]
        public void IdleSpawnWaitsWhileTheShiftIsAheadOfItsBudgetSchedule()
        {
            var d = RunUntilSpawn(In(w: 0f, used: 5), out var waited, limit: 6f);
            Assert.IsFalse(d.IsSpawn, "5 cars already, none due by an even spread at the start");
            Assert.GreaterOrEqual(waited, 6f);
        }

        [Test]
        public void BusyLotWaitsForTheBaseInterval()
        {
            var d = RunUntilSpawn(In(w: 1f, t: 1f), out var waited);
            Assert.IsTrue(d.IsSpawn);
            Assert.AreEqual(ScheduleReason.OnTarget, d.Reason);
            Assert.Greater(waited, _settings.Pacing.IdleSeconds);
        }

        [Test]
        public void UnderTargetSendsSoonerThanOverTarget()
        {
            RunUntilSpawn(In(w: 0.6f, t: 1f), out var under);
            _pacer.Reset();
            RunUntilSpawn(In(w: 1.3f, t: 1f), out var over);
            Assert.Less(under, over);
        }

        [Test]
        public void DrowningHoldsAllArrivalsForTheReliefHold()
        {
            var first = _pacer.Tick(In(w: 1.5f), Dt);
            Assert.AreEqual(PacerAction.Hold, first.Action);
            Assert.AreEqual(HoldReason.Relief, first.Hold);
            Assert.IsTrue(first.ReliefStarted);
            Assert.AreEqual(ReliefTrigger.Workload, first.Trigger);
            for (var t = Dt; t < _settings.Relief.HoldSeconds - Dt; t += Dt)
                Assert.AreEqual(PacerAction.Hold, _pacer.Tick(In(w: 1.5f), Dt).Action, "at " + t);
            Assert.IsTrue(_pacer.InRelief);
        }

        [Test]
        public void AfterTheHoldOnlyTwoPlainCarsAreSentThenReliefEnds()
        {
            _pacer.Tick(In(w: 1.5f), Dt);
            var input = In(w: 1.2f);
            var first = RunUntilSpawn(input, out _);
            Assert.IsTrue(first.IsSpawn);
            Assert.IsTrue(first.ForcePlain);
            Assert.AreEqual(ScheduleReason.ReliefCar, first.Reason);
            Assert.IsFalse(_pacer.NotifySpawned());
            var second = RunUntilSpawn(input, out _);
            Assert.IsTrue(second.ForcePlain);
            Assert.IsTrue(_pacer.NotifySpawned(), "the second plain car ends relief");
            Assert.IsFalse(_pacer.InRelief);
            var third = RunUntilSpawn(In(w: 1f), out _);
            Assert.IsFalse(third.ForcePlain);
        }

        [Test]
        public void PressureAndAngryPatronsAlsoStartRelief()
        {
            var pressure = _pacer.Tick(In(w: 0.5f, p: 0.9f), Dt);
            Assert.IsTrue(pressure.ReliefStarted);
            Assert.AreEqual(ReliefTrigger.Pressure, pressure.Trigger);
            _pacer.Reset();
            var angry = _pacer.Tick(In(w: 0.5f, angry: 2), Dt);
            Assert.IsTrue(angry.ReliefStarted);
            Assert.AreEqual(ReliefTrigger.Angry, angry.Trigger);
        }

        [Test]
        public void ReliefCannotRestartRightAfterItEnds()
        {
            _pacer.Tick(In(w: 1.5f), Dt);
            for (var i = 0; i < 2; i++) { RunUntilSpawn(In(w: 1.2f), out _); _pacer.NotifySpawned(); }
            Assert.IsFalse(_pacer.Tick(In(w: 1.5f), Dt).ReliefStarted, "cooldown");
        }

        [Test]
        public void FullQueueHoldsEvenWhenIdle()
        {
            var d = RunUntilSpawn(In(w: 0f, queue: 3), out _, limit: 60f);
            Assert.IsFalse(d.IsSpawn);
            Assert.AreEqual(HoldReason.QueueFull, d.Hold);
            var freed = _pacer.Tick(In(w: 0f, queue: 2), Dt);
            Assert.IsTrue(freed.IsSpawn);
        }

        [Test]
        public void ArrivalsNeverExceedTheBudgetPlusSlack()
        {
            var used = 0;
            for (var i = 0; i < 4000; i++)
            {
                var d = _pacer.Tick(In(w: 0f, used: used), Dt);
                if (!d.IsSpawn) continue;
                used++;
                _pacer.NotifySpawned();
            }
            Assert.AreEqual(_settings.Pacing.MaxArrivals, used);
            Assert.AreEqual(HoldReason.BudgetSpent, _pacer.Tick(In(w: 0f, used: used), Dt).Hold);
        }

        [Test]
        public void CountdownRescalesSmoothlyWhenTheWorkloadJumps()
        {
            for (var i = 0; i < 6; i++) _pacer.Tick(In(w: 1f, t: 1f), Dt);
            var before = _pacer.Tick(In(w: 1f, t: 1f), Dt).SecondsToNext;
            var after = _pacer.Tick(In(w: 1.35f, t: 1f), Dt).SecondsToNext;
            Assert.Greater(before, 0f);
            Assert.Less(System.Math.Abs(after - before), 5f, "a reset would jump by a whole interval");
        }

        [Test]
        public void SameRandomAndInputsGiveTheSameSchedule()
        {
            RunUntilSpawn(In(w: 1f), out var a);
            _pacer.NotifySpawned();
            var other = new ArrivalPacer(new MidRandom(), _settings.Pacing, new ReliefGate(_settings.Relief));
            var waited = 0f;
            while (!other.Tick(In(w: 1f), Dt).IsSpawn && waited < 300f) waited += Dt;
            Assert.AreEqual(a, waited + Dt, 0.001f);
        }
    }
}
