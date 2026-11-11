using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Director;

namespace UncannyValet.Tests.Director
{
    /// <summary>Issue 73: in the last stretch of the shift every parked car's patron is called back, spread and spaced.</summary>
    public class ReturnLatePhaseTests
    {
        private const float Now = 100f;
        private const float Left = 60f;
        private static readonly float[] ToPickup = { 20f, 35f, 50f, 60f, 70f };

        private ReturnSettings _settings;
        private ReturnScheduler _scheduler;
        private readonly List<ReturnShift> _out = new List<ReturnShift>();

        [SetUp]
        public void SetUp()
        {
            _settings = new ReturnSettings();
            _scheduler = new ReturnScheduler(_settings);
            _out.Clear();
        }

        private static LotSnapshot Lot(float[] toPickup, CarLotState state = CarLotState.Parked)
        {
            var b = new SnapshotBuilderForTests { NowSeconds = Now };
            foreach (var t in toPickup) b.Car(state, toPickup: t, plannedStay: 40f);
            return b.Build();
        }

        private static ReturnInput In(float left = Left, float lastSummoned = float.NegativeInfinity)
            => new ReturnInput(Now, left, 0f, 0.15f, false, false, false, lastSummoned);

        private float[] Times(float[] toPickup)
        {
            var times = (float[])toPickup.Clone();
            foreach (var s in _out) times[s.Car.Value - 1] += s.DeltaSeconds;
            for (var i = 0; i < times.Length; i++) times[i] += Now;
            System.Array.Sort(times);
            return times;
        }

        [Test]
        public void EveryParkedReturnIsPulledForwardOutsideTheWindow()
        {
            _scheduler.Plan(Lot(ToPickup), In(), _out);
            Assert.AreEqual(5, _out.Count);
            foreach (var s in _out) Assert.Less(s.DeltaSeconds, 0f);
            var latest = Times(ToPickup)[ToPickup.Length - 1];
            Assert.LessOrEqual(latest, Now + Left - _settings.LastLeadSeconds + 0.001f, "the last pickup must leave time to deliver it");
            Assert.Less(latest, Now + ToPickup[4] - 10f, "moved by more than the +/-25 percent window");
        }

        [Test]
        public void NoTwoReturnsAreCloserThanTheSpacingAndNoneIsTooClose()
        {
            _scheduler.Plan(Lot(ToPickup), In(), _out);
            var times = Times(ToPickup);
            Assert.GreaterOrEqual(times[0], Now + _settings.MinLeadSeconds - 0.001f);
            for (var i = 1; i < times.Length; i++)
                Assert.GreaterOrEqual(times[i] - times[i - 1], _settings.SpacingSeconds - 0.001f, "pair " + i);
        }

        [Test]
        public void ThePatronSentLastPushesTheFirstReturnBack()
        {
            _scheduler.Plan(Lot(ToPickup), In(lastSummoned: Now - 1f), _out);
            Assert.GreaterOrEqual(Times(ToPickup)[0], Now - 1f + _settings.SpacingSeconds - 0.001f);
        }

        [Test]
        public void ThePlanIsStableOnTheNextTick()
        {
            _scheduler.Plan(Lot(ToPickup), In(), _out);
            var moved = new float[ToPickup.Length];
            for (var i = 0; i < moved.Length; i++) moved[i] = ToPickup[i];
            foreach (var s in _out) moved[s.Car.Value - 1] += s.DeltaSeconds;
            _out.Clear();
            Assert.AreEqual(0, _scheduler.Plan(Lot(moved), In(), _out), "a second plan must not move anything again");
        }

        [Test]
        public void EveryOpenTicketIsCalledBackWhereverItsCarIs()
        {
            foreach (var state in new[] { CarLotState.InStation, CarLotState.Driven, CarLotState.WaitingToPark })
            {
                _scheduler = new ReturnScheduler(_settings);
                Assert.AreEqual(5, _scheduler.Plan(Lot(ToPickup, state), In(), _out), state.ToString());
                _out.Clear();
            }
        }

        [Test]
        public void ARequestedPickupOrACarThatHasNotArrivedIsNeverMoved()
        {
            var gone = new[] { float.PositiveInfinity, float.PositiveInfinity };
            Assert.AreEqual(0, _scheduler.Plan(Lot(gone, CarLotState.PickupRequested), In(), _out));
            Assert.AreEqual(0, _scheduler.Plan(Lot(gone, CarLotState.Arriving), In(), _out));
        }

        [Test]
        public void APatronWhoAlreadyLeftIsNeverMoved()
        {
            Assert.AreEqual(0, _scheduler.Plan(Lot(new[] { 0f, 2f }), In(), _out));
        }

        [Test]
        public void BeforeThePhaseStartsNothingMovesOutsideTheWindow()
        {
            _scheduler.Plan(Lot(new[] { 70f }), In(left: Left + 1f), _out);
            Assert.AreEqual(0, _out.Count);
        }

        [Test]
        public void ThePhaseCanBeSwitchedOff()
        {
            var tuning = new TuningStore(new EventBus());
            tuning.Seed(TuningKey.LatePickupStart, 0f);
            _settings.Read(tuning);
            Assert.AreEqual(0, _scheduler.Plan(Lot(ToPickup), In(left: 10f), _out));
        }

        [Test]
        public void ALateCarIsPlannedOnceWhenItArrivesLater()
        {
            _scheduler.Plan(Lot(new[] { 20f }), In(), _out);
            _out.Clear();
            var added = _scheduler.Plan(Lot(new[] { 20f, 90f }), In(), _out);
            Assert.LessOrEqual(added, 1, "only the new car may move");
        }
    }
}
