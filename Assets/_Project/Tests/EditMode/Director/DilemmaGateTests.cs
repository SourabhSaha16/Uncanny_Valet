using NUnit.Framework;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Director;

namespace UncannyValet.Tests.Director
{
    public class DilemmaGateTests
    {
        private const float Cooldown = 25f;
        private const float Blackout = 20f;

        private DilemmaGate _gate;
        private SnapshotBuilderForTests _lot;

        [SetUp]
        public void SetUp()
        {
            _gate = new DilemmaGate(new GateSettings());
            _lot = new SnapshotBuilderForTests();
        }

        private bool CanFire(float now, float gap = 0f, bool rising = false, bool relief = false)
        {
            _lot.NowSeconds = now;
            return _gate.CanFire(_lot.Build(), gap, rising, relief);
        }

        [Test]
        public void AFreshGateWithHeadroomAllowsADilemma() => Assert.IsTrue(CanFire(60f));

        [Test]
        public void OnlyOneDilemmaCanBeOpenAtATime()
        {
            _gate.Open(DilemmaKind.WashBlocked, new CarId(3), 60f);
            Assert.IsTrue(_gate.IsOpen);
            Assert.IsFalse(CanFire(61f));
        }

        [Test]
        public void ResolvingReportsTheRuleAndTheSecondsThenStartsTheCooldown()
        {
            _gate.Open(DilemmaKind.BaysBlocked, new CarId(3), 60f);
            Assert.IsTrue(_gate.TryResolve(75f, out var kind, out var seconds));
            Assert.AreEqual(DilemmaKind.BaysBlocked, kind);
            Assert.AreEqual(15f, seconds, 0.001f);
            Assert.IsFalse(_gate.IsOpen);
            Assert.IsFalse(CanFire(75f + Cooldown - 1f));
            Assert.AreEqual(1f, _gate.CooldownLeft(75f + Cooldown - 1f), 0.001f);
            Assert.IsTrue(CanFire(75f + Cooldown + 0.1f));
        }

        [Test]
        public void ResolvingWithNothingOpenDoesNothing()
        {
            Assert.IsFalse(_gate.TryResolve(10f, out _, out _));
            Assert.IsTrue(CanFire(10f), "no cooldown was started");
        }

        [Test]
        public void NoDilemmaDuringRelief() => Assert.IsFalse(CanFire(60f, relief: true));

        [Test]
        public void AnAngryPatronBlacksOutDilemmasForTwentySeconds()
        {
            _lot.Angry.Add(100f);
            Assert.IsFalse(CanFire(100f + Blackout - 1f));
            Assert.IsTrue(CanFire(100f + Blackout + 1f));
        }

        [Test]
        public void HeadroomMeansSmallGapOrARisingCurve()
        {
            Assert.IsTrue(CanFire(60f, 0.15f), "at the band edge");
            Assert.IsFalse(CanFire(60f, 0.4f), "player is over target");
            Assert.IsTrue(CanFire(60f, 0.4f, rising: true), "a wave is coming");
        }

        [Test]
        public void ResetForgetsTheOpenDilemmaAndTheCooldown()
        {
            _gate.Open(DilemmaKind.Chain, new CarId(2), 10f);
            _gate.TryResolve(20f, out _, out _);
            _gate.Open(DilemmaKind.Chain, new CarId(5), 30f);
            _gate.Reset();
            Assert.IsFalse(_gate.IsOpen);
            Assert.IsTrue(CanFire(0f));
        }
    }
}
