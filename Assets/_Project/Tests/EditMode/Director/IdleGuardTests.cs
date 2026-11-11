using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Director;

namespace UncannyValet.Tests.Director
{
    /// <summary>Issue 70: an empty arrival queue never waits longer than the limit; a queued car never triggers the guard.</summary>
    public class IdleGuardTests
    {
        private const float Dt = 0.5f;

        private DirectorSettings _settings;
        private IdleGuard _guard;
        private ArrivalPacer _pacer;

        [SetUp]
        public void SetUp()
        {
            _settings = new DirectorSettings();
            _guard = new IdleGuard(_settings.Pacing);
            _pacer = new ArrivalPacer(new MidRandom(), _settings.Pacing, new ReliefGate(_settings.Relief));
        }

        private static PacerInput In(int queue = 0, int used = 0, float left = 200f, float w = 1f, float p = 0f)
            => new PacerInput(w, p, 1f, 0f, 0, queue, 3, used, 240f, 240f, 15f, 25f, left);

        private float SecondsToForce(PacerInput input, float limit = 60f)
        {
            for (var t = Dt; t <= limit; t += Dt) if (_guard.Update(input, Dt)) return t;
            return -1f;
        }

        [Test]
        public void EmptyQueueForcesASendAfterTheLimit()
        {
            Assert.AreEqual(_settings.Pacing.MaxEmptyQueueSeconds, SecondsToForce(In()), Dt);
        }

        [Test]
        public void TheFirstCarOfAShiftIsDueWithinTheLimit()
        {
            _guard.Reset();
            Assert.LessOrEqual(SecondsToForce(In(used: 0, left: 240f)), _settings.Pacing.MaxEmptyQueueSeconds + Dt);
        }

        [Test]
        public void ACarInTheQueueNeverForcesAndResetsTheCount()
        {
            _guard.Update(In(), 1.5f);
            Assert.IsFalse(_guard.Update(In(queue: 1), 100f));
            Assert.IsFalse(_guard.Armed);
            Assert.IsFalse(_guard.Update(In(), 1f), "the empty count restarted");
        }

        [Test]
        public void ASpentBudgetNeverForces()
        {
            var used = _settings.Pacing.MaxArrivals;
            Assert.AreEqual(-1f, SecondsToForce(In(used: used)));
            Assert.AreEqual(IdleGuard.NotArmed, _guard.SecondsUntilForced);
        }

        [Test]
        public void TooLateInTheShiftNeverForces()
        {
            Assert.AreEqual(-1f, SecondsToForce(In(left: _settings.Pacing.IdleCutoffSeconds - 1f)));
        }

        [Test]
        public void TheCountdownShowsTheSecondsLeft()
        {
            _guard.Update(In(), 0.5f);
            Assert.AreEqual(_settings.Pacing.MaxEmptyQueueSeconds - 0.5f, _guard.SecondsUntilForced, 0.001f);
        }

        [Test]
        public void PacerSendsToAnEmptyQueueWithinTheLimitEvenWhenItWouldWaitLonger()
        {
            var sent = -1f;
            for (var t = Dt; t < 30f && sent < 0f; t += Dt) if (_pacer.Tick(In(w: 1f), Dt).IsSpawn) sent = t;
            Assert.That(sent, Is.InRange(0f, _settings.Pacing.MaxEmptyQueueSeconds + Dt));
        }

        [Test]
        public void PacerWithQueuedCarKeepsItsOwnPacing()
        {
            for (var t = Dt; t < 10f; t += Dt) Assert.IsFalse(_pacer.Tick(In(queue: 1, w: 1f), Dt).IsSpawn, "at " + t);
        }

        [Test]
        public void PacerStillHoldsTheBudgetAndTheQueueCap()
        {
            var spent = _pacer.Tick(In(used: _settings.Pacing.MaxArrivals), 5f);
            Assert.AreEqual(HoldReason.BudgetSpent, spent.Hold);
            _pacer.Reset();
            Assert.IsFalse(_pacer.Tick(In(queue: 3), 5f).IsSpawn);
        }

        [Test]
        public void ReliefHoldYieldsAPlainCarWhenTheQueueIsEmpty()
        {
            var d = _pacer.Tick(In(w: 1.5f), 5f);
            Assert.IsTrue(d.ReliefStarted);
            Assert.IsTrue(d.IsSpawn);
            Assert.IsTrue(d.ForcePlain);
        }

        [Test]
        public void ALostSendIsRetriedWithinTheLimitWhileTheQueueIsEmpty()
        {
            Assert.IsTrue(_pacer.Tick(In(), 5f).IsSpawn);
            var retried = -1f;
            for (var t = Dt; t < 8f && retried < 0f; t += Dt) if (_pacer.Tick(In(), Dt).IsSpawn) retried = t;
            Assert.That(retried, Is.InRange(0f, _settings.Pacing.MaxEmptyQueueSeconds + Dt));
        }

        [Test]
        public void TheLimitIsASeededTuningKeyIndependentOfTheDials()
        {
            var bus = new EventBus();
            var tuning = new TuningStore(bus);
            _settings.Pacing.Seed(tuning, 14, 2);
            tuning.TryGet(TuningKey.ArrivalIdleLimit, out var limit);
            Assert.AreEqual(2f, limit);
        }
    }
}
