using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Director;

namespace UncannyValet.Tests.Director
{
    /// <summary>Spec section 5: return window, pickup spacing, forward and hold nudges; and the issue 73 late phase.</summary>
    public class ReturnSchedulerTests
    {
        private const float Now = 100f;
        private const float Stay = 40f;
        private const float Room = 10f;

        private ReturnSettings _settings;
        private ReturnScheduler _scheduler;
        private List<ReturnShift> _out;

        [SetUp]
        public void SetUp()
        {
            _settings = new ReturnSettings();
            _scheduler = new ReturnScheduler(_settings);
            _out = new List<ReturnShift>();
        }

        private static LotSnapshot Lot(params (CarLotState state, float toPickup, float shift)[] cars)
        {
            var b = new SnapshotBuilderForTests { NowSeconds = Now };
            foreach (var c in cars) b.Car(c.state, toPickup: c.toPickup, plannedStay: Stay, shift: c.shift);
            return b.Build();
        }

        private static (CarLotState, float, float) Parked(float toPickup, float shift = 0f)
            => (CarLotState.Parked, toPickup, shift);

        private static ReturnInput In(float gap = 0f, float left = 200f, bool relief = false, bool tight = false,
            bool wave = false, float lastSummoned = float.NegativeInfinity)
            => new ReturnInput(Now, left, gap, 0.15f, relief, tight, wave, lastSummoned);

        private int Plan(LotSnapshot lot, ReturnInput input)
        {
            _out.Clear();
            return _scheduler.Plan(lot, input, _out);
        }

        private static float TimeOf(LotSnapshot lot, int index, List<ReturnShift> shifts)
        {
            var car = lot.Cars[index];
            foreach (var s in shifts) if (s.Car.Value == car.Car.Value) return Now + car.SecondsToPickup + s.DeltaSeconds;
            return Now + car.SecondsToPickup;
        }

        [Test]
        public void UnderTargetBringsAReturnForwardNoFurtherThanTheWindow()
        {
            Assert.AreEqual(1, Plan(Lot(Parked(30f)), In(gap: -0.5f)));
            Assert.AreEqual(-Room, _out[0].DeltaSeconds, 0.001f);
        }

        [Test]
        public void OverTargetHoldsAReturnBackNoFurtherThanTheWindow()
        {
            Assert.AreEqual(1, Plan(Lot(Parked(30f)), In(gap: 0.5f)));
            Assert.AreEqual(Room, _out[0].DeltaSeconds, 0.001f);
        }

        [Test]
        public void ReliefHoldsBackAndNeverBringsForward()
        {
            Plan(Lot(Parked(30f)), In(gap: -0.5f, relief: true, tight: true));
            Assert.AreEqual(Room, _out[0].DeltaSeconds, 0.001f);
        }

        [Test]
        public void ATightLotBringsForwardEvenOnTarget()
        {
            Plan(Lot(Parked(30f)), In(tight: true));
            Assert.AreEqual(-Room, _out[0].DeltaSeconds, 0.001f);
        }

        [Test]
        public void OnTargetMovesNothing()
        {
            Assert.AreEqual(0, Plan(Lot(Parked(30f), Parked(50f)), In(gap: 0.05f)));
        }

        [Test]
        public void ARemainingWindowAccountsForWhatWasAlreadyMoved()
        {
            // Already 8 s earlier: only 2 s of the 10 s window are left.
            Plan(Lot(Parked(30f, shift: -8f)), In(gap: -0.5f));
            Assert.AreEqual(-2f, _out[0].DeltaSeconds, 0.001f);
            _scheduler = new ReturnScheduler(_settings);
            Assert.AreEqual(0, Plan(Lot(Parked(30f, shift: -10f)), In(gap: -0.5f)));
        }

        [Test]
        public void NeverMovesAPatronWhoAlreadyLeftTheDiner()
        {
            Assert.AreEqual(0, Plan(Lot(Parked(0f), Parked(1.5f)), In(gap: -0.5f, tight: true)));
            Assert.AreEqual(0, Plan(Lot(Parked(0f), Parked(1.5f)), In(left: 30f)));
        }

        [Test]
        public void ACarCountsWhereverItIsButARequestedOrNotYetArrivedOneDoesNot()
        {
            foreach (var state in new[] { CarLotState.InStation, CarLotState.Driven, CarLotState.WaitingToPark, CarLotState.Parked })
            {
                _scheduler = new ReturnScheduler(_settings);
                Assert.AreEqual(1, Plan(Lot((state, 30f, 0f)), In(gap: -0.5f)), state.ToString());
            }
            var gone = Lot((CarLotState.PickupRequested, float.PositiveInfinity, 0f),
                (CarLotState.Arriving, float.PositiveInfinity, 0f));
            Assert.AreEqual(0, Plan(gone, In(gap: -0.5f, tight: true)));
            Assert.AreEqual(0, Plan(gone, In(left: 30f)));
        }

        [Test]
        public void ANudgeNeverMovesAReturnCloserThanTheMinimumLeadToNow()
        {
            Plan(Lot(Parked(8f)), In(gap: -0.5f));
            Assert.GreaterOrEqual(Now + 8f + _out[0].DeltaSeconds, Now + _settings.MinLeadSeconds - 0.001f);
        }

        [Test]
        public void OnlyOneReturnIsNudgedPerCooldown()
        {
            Assert.AreEqual(1, Plan(Lot(Parked(30f), Parked(60f)), In(gap: -0.5f)));
            Assert.AreEqual(0, Plan(Lot(Parked(30f), Parked(60f)), In(gap: -0.5f)), "same instant, cooldown running");
        }

        [Test]
        public void TwoReturnsCloserThanTheSpacingAreSpread()
        {
            var lot = Lot(Parked(30f), Parked(31f));
            Assert.AreEqual(1, Plan(lot, In()));
            Assert.AreEqual(3f, _out[0].DeltaSeconds, 0.001f);
            Assert.AreEqual(Now + 30f + _settings.SpacingSeconds, TimeOf(lot, 1, _out), 0.001f);
        }

        [Test]
        public void ASpacingFixNeverLeavesTheWindow()
        {
            // The earlier one is already at the early end of its window and the later at the late end, so nothing can move.
            Assert.AreEqual(0, Plan(Lot(Parked(30f, shift: -Room), Parked(31f, shift: Room)), In()));
        }

        [Test]
        public void WhenTheLaterReturnCannotMoveLaterTheEarlierOneMovesEarlier()
        {
            var lot = Lot(Parked(30f), Parked(31f, shift: Room));
            Assert.AreEqual(1, Plan(lot, In()));
            Assert.AreEqual(-3f, _out[0].DeltaSeconds, 0.001f);
            Assert.AreEqual(_settings.SpacingSeconds, TimeOf(lot, 1, _out) - TimeOf(lot, 0, _out), 0.001f);
        }

        [Test]
        public void TheSpacingIsWaivedInAWavePeak()
        {
            Assert.AreEqual(0, Plan(Lot(Parked(30f), Parked(31f)), In(wave: true)));
        }

        [Test]
        public void ThePatronSentLastCountsForTheSpacing()
        {
            Plan(Lot(Parked(30f)), In(lastSummoned: Now + 28f));
            Assert.AreEqual(2f, _out[0].DeltaSeconds, 0.001f);
        }

    }
}
