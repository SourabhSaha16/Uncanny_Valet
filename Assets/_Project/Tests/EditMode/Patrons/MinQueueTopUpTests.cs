using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Patrons;

namespace UncannyValet.Tests.Patrons
{
    /// <summary>
    /// Issue 178: Vanaheim training keeps at least 2 cars queued. Issue 179: a shift sends its first car at once on every
    /// lot kind (the top-up rules are due at minute 0).
    /// </summary>
    public class MinQueueTopUpTests
    {
        private const float Spacing = 0.5f;
        private const float Step = 0.1f;
        private const int Minimum = 2;

        private TuningStore _tuning;

        [SetUp]
        public void SetUp() => _tuning = new TuningStore(new EventBus());

        private MinQueueTopUp For(int minimum)
        {
            _tuning.Seed(TuningKey.PlanMinQueuedCars, minimum);
            return new MinQueueTopUp(_tuning, Spacing);
        }

        [Test]
        public void NoMinimumNeverAsksForACar()
        {
            Assert.IsFalse(For(0).Due(0, 0f));
        }

        [Test]
        public void AnUnseededMinimumCountsAsNone()
        {
            Assert.IsFalse(new MinQueueTopUp(_tuning, Spacing).Due(0, 0f));
        }

        [Test]
        public void AnEmptyQueueIsDueAtShiftStart()
        {
            Assert.IsTrue(For(Minimum).Due(0, 0f), "the first car drives in at t=0");
        }

        [Test]
        public void AQueueAtTheMinimumIsNotDue()
        {
            Assert.IsFalse(For(Minimum).Due(Minimum, 0f));
        }

        [Test]
        public void SendsAreSpacedSoCarsDoNotArriveStacked()
        {
            var rule = For(Minimum);
            rule.Sent(0f);
            Assert.IsFalse(rule.Due(0, Spacing * 0.5f));
            Assert.IsTrue(rule.Due(0, Spacing));
        }

        [Test]
        public void ANewShiftForgetsTheLastSend()
        {
            var rule = For(Minimum);
            rule.Sent(10f);
            rule.Reset();
            Assert.IsTrue(rule.Due(0, 0f));
        }

        [Test]
        public void ARunKeepsTheQueueAtTheMinimumAtEveryStep()
        {
            var rule = For(Minimum);
            var queued = 0;
            var short_ = 0;
            for (var step = 0; step < 600; step++)
            {
                var t = step * Step;
                if (step > 0 && step % 70 == 0 && queued > 0) queued--;
                if (rule.Due(queued, t)) { queued++; rule.Sent(t); }
                if (t > Spacing * Minimum && queued < Minimum) short_++;
            }
            Assert.AreEqual(0, short_, "at the start and after every car leaves, the queue is back at the minimum at once");
        }

        [Test]
        public void EveryLotKindSendsItsFirstCarAtMinuteZero()
        {
            Assert.IsTrue(ArrivalPacings.For(LotKind.Crazy, Spacing).Due(0f), "crazy lots top up at once");
            Assert.IsTrue(For(Minimum).Due(0, 0f), "a lot with a queue minimum sends at once");
            Assert.IsFalse(ArrivalPacings.For(LotKind.Training, Spacing).TopsUpQueue, "other training lots: the Director sends the first car at once (IdleGuardTests)");
        }

    }
}
