using NUnit.Framework;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Lot;
using UnityEngine;

namespace UncannyValet.Tests.Lot
{
    public class PerfectJudgeTests
    {
        private PerfectTuning _tuning;

        [SetUp]
        public void SetUp() => _tuning = new PerfectTuning(new PerfectSettings());

        [TestCase(180f, true, 0f, true)]
        [TestCase(195f, true, 0.49f, true)]
        [TestCase(165f, true, 0.2f, true)]
        [TestCase(-180f, true, 0f, true)]
        [TestCase(200f, true, 0.2f, false)]
        [TestCase(180f, false, 0.2f, false)]
        [TestCase(180f, true, 0.5f, false)]
        [TestCase(180f, true, 1f, false)]
        [TestCase(110f, true, 0f, false)]
        public void DropOffNeedsAngleZoneAndEarlyArrival(float rotation, bool inside, float elapsed, bool expected) =>
            Assert.AreEqual(expected, new DropOffJudge(_tuning).IsPerfect(rotation, inside, elapsed));

        [Test]
        public void DropOffLimitsFollowTheTuning()
        {
            _tuning.TrySet(TuningKey.PerfectDropOffAngle, 40f);
            _tuning.TrySet(TuningKey.PerfectDropOffTimerShare, 0.8f);
            Assert.IsTrue(new DropOffJudge(_tuning).IsPerfect(210f, true, 0.7f));
        }

        [TestCase(0f, 0f, 0f, false, ParkVerdict.Perfect)]
        [TestCase(180f, 0.1f, -0.2f, false, ParkVerdict.Perfect)]
        [TestCase(4f, 0f, 0f, false, ParkVerdict.Perfect)]
        [TestCase(176f, 0f, 0f, false, ParkVerdict.Perfect)]
        [TestCase(-4f, 0f, 0f, false, ParkVerdict.Perfect)]
        [TestCase(8f, 0f, 0f, false, ParkVerdict.Crooked)]
        [TestCase(90f, 0f, 0f, false, ParkVerdict.Crooked)]
        [TestCase(0f, 0.2f, 0f, false, ParkVerdict.OffCentreSide)]
        [TestCase(0f, -0.2f, 0f, false, ParkVerdict.OffCentreSide)]
        [TestCase(0f, 0f, 0.4f, false, ParkVerdict.OffCentreLength)]
        [TestCase(0f, 0f, 0f, true, ParkVerdict.Touching)]
        public void ParkJudgeChecksAngleOffsetsAndContact(float angle, float side, float length, bool touching, ParkVerdict expected) =>
            Assert.AreEqual(expected, new ParkJudge(_tuning).Judge(angle, side, length, touching));

        [Test]
        public void AxisDeviationTreatsNoseInAndTailInAlike()
        {
            Assert.AreEqual(3f, ParkJudge.AxisDeviation(3f), 0.001f);
            Assert.AreEqual(3f, ParkJudge.AxisDeviation(183f), 0.001f);
            Assert.AreEqual(3f, ParkJudge.AxisDeviation(-177f), 0.001f);
            Assert.AreEqual(90f, ParkJudge.AxisDeviation(90f), 0.001f);
        }

        [Test]
        public void GateAwardsTheFirstAlignedRestOnly()
        {
            var gate = new PerfectParkGate(_tuning);
            var car = new CarId(1);
            Assert.IsFalse(gate.Rest(car, 0f, Vector2.zero, false));
            Assert.IsTrue(gate.Rest(car, 1f, new Vector2(5f, 0f), true));
        }

        [Test]
        public void GateRejectsANudgeUnderTheMinimumTravel()
        {
            var gate = new PerfectParkGate(_tuning);
            var car = new CarId(1);
            Assert.IsTrue(gate.Rest(car, 0f, Vector2.zero, true));
            Assert.IsFalse(gate.Rest(car, 60f, new Vector2(1f, 0f), true));
        }

        [Test]
        public void GateRejectsARepeatInsideTheCooldownThenAllowsIt()
        {
            var gate = new PerfectParkGate(_tuning);
            var car = new CarId(1);
            Assert.IsTrue(gate.Rest(car, 0f, Vector2.zero, true));
            Assert.IsFalse(gate.Rest(car, 5f, new Vector2(8f, 0f), true));
            Assert.IsTrue(gate.Rest(car, 15f, new Vector2(16f, 0f), true));
        }

        [Test]
        public void GateTracksEachCarSeparatelyAndForgets()
        {
            var gate = new PerfectParkGate(_tuning);
            Assert.IsTrue(gate.Rest(new CarId(1), 0f, Vector2.zero, true));
            Assert.IsTrue(gate.Rest(new CarId(2), 0f, Vector2.zero, true));
            gate.Forget(new CarId(1));
            Assert.IsTrue(gate.Rest(new CarId(1), 1f, Vector2.zero, true));
        }

        [Test]
        public void TicketMapTracksTicketAndPickupProgress()
        {
            var map = new CarTicketMap();
            var car = new CarId(3);
            Assert.AreEqual(TicketId.None, map.TicketOf(car));
            Assert.AreEqual(0f, map.PickupElapsed(car));
            map.Opened(car, new TicketId(9));
            map.PickupProgress(car, 0.4f);
            Assert.AreEqual(new TicketId(9), map.TicketOf(car));
            Assert.AreEqual(0.4f, map.PickupElapsed(car));
            map.Closed(car);
            Assert.AreEqual(TicketId.None, map.TicketOf(car));
            Assert.AreEqual(0f, map.PickupElapsed(car));
        }

        [Test]
        public void TuningTakesChangesOnlyForItsOwnKeys()
        {
            Assert.IsTrue(_tuning.TrySet(TuningKey.PerfectParkAngle, 9f));
            Assert.AreEqual(9f, _tuning.ParkAngle);
            Assert.IsFalse(_tuning.TrySet(TuningKey.HandlingForwardGrip, 1f));
        }
    }
}
