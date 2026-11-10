using NUnit.Framework;
using UncannyValet.Core.Events;
using UncannyValet.Director;

namespace UncannyValet.Tests.Director
{
    /// <summary>
    /// Regression: a Spawn decision the spawner could not execute (no free slot, request lost) used to leave the pacer
    /// waiting for a CarSpawned that never came, so the lot stayed empty for the rest of the shift.
    /// </summary>
    public class ArrivalPacerStuckTests
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

        // Late in the shift, an empty lot: the even spread of the budget is nearly spent, so idle spawns are allowed.
        private static PacerInput EmptyLate(int used = 6, int queue = 0)
            => new PacerInput(0.27f, 0f, 0.5f, 0f, 0, queue, 3, used, 20f, 240f, 15f, 25f);

        private int CountSpawns(PacerInput input, float seconds)
        {
            var spawns = 0;
            for (var t = 0f; t < seconds; t += Dt) if (_pacer.Tick(input, Dt).IsSpawn) spawns++;
            return spawns;
        }

        [Test]
        public void ASpawnNobodyAcknowledgesIsSentAgain()
        {
            var spawns = CountSpawns(EmptyLate(), 120f);
            Assert.GreaterOrEqual(spawns, 2, "the first send was lost; the director must try again");
        }

        [Test]
        public void WaitingForAnAcknowledgementNeverShowsAStuckCountdownForever()
        {
            var sawWait = false;
            for (var t = 0f; t < 120f; t += Dt)
            {
                var d = _pacer.Tick(EmptyLate(), Dt);
                if (d.Action == PacerAction.Wait && d.SecondsToNext <= 0f) sawWait = true;
                if (d.IsSpawn && sawWait) return;
            }
            Assert.Fail("a lost spawn must end in a new Spawn decision");
        }

        [Test]
        public void ReliefNeverHoldsAnEmptyLotForever()
        {
            _pacer.Tick(new PacerInput(1.5f, 0f, 1f, 0f, 0, 0, 3, 6, 20f, 240f, 15f, 25f), Dt);
            Assert.IsTrue(_pacer.InRelief);
            var seen = ForcePlainSpawn(EmptyLate(), 60f);
            Assert.IsTrue(seen, "once the hold is over an idle empty lot gets a plain relief car");
        }

        [Test]
        public void ReliefCarThatNeverArrivesIsRetriedUntilItDoes()
        {
            _pacer.Tick(new PacerInput(1.5f, 0f, 1f, 0f, 0, 0, 3, 6, 20f, 240f, 15f, 25f), Dt);
            var plainSends = 0;
            for (var t = 0f; t < 240f; t += Dt)
            {
                var d = _pacer.Tick(EmptyLate(), Dt);
                if (d.IsSpawn && d.ForcePlain) plainSends++;
            }
            Assert.GreaterOrEqual(plainSends, 3, "a blocked relief car keeps being asked for");
        }

        [Test]
        public void ArrivalAcknowledgedInTimeIsNotSentAgainAtOnce()
        {
            while (!_pacer.Tick(EmptyLate(), Dt).IsSpawn) { }
            _pacer.NotifySpawned();
            Assert.IsFalse(_pacer.Tick(EmptyLate(), Dt).IsSpawn);
        }

        private bool ForcePlainSpawn(PacerInput input, float seconds)
        {
            for (var t = 0f; t < seconds; t += Dt)
            {
                var d = _pacer.Tick(input, Dt);
                if (d.IsSpawn) return d.ForcePlain && d.Reason == ScheduleReason.ReliefCar;
            }
            return false;
        }
    }
}
