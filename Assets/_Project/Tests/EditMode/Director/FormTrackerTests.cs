using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Director;

namespace UncannyValet.Tests.Director
{
    public class FormTrackerTests
    {
        private const int MaxCombo = 4;

        private readonly FormSettings _settings = new FormSettings();
        private EventBus _bus;
        private FakeClock _clock;
        private FormTracker _form;

        [SetUp]
        public void SetUp()
        {
            _bus = new EventBus();
            _clock = new FakeClock();
            _form = new FormTracker(_bus, _settings, _clock, null, MaxCombo);
        }

        [TearDown]
        public void TearDown() => _form.Dispose();

        private LotSnapshot At(float now, params float[] angryTimes)
            => new LotSnapshot(null, null, 0, 0, float.PositiveInfinity, now, angryTimes);

        [Test]
        public void StepNeverMovesMoreThanTheLimit()
        {
            Assert.AreEqual(0.25f, FormMath.Step(0f, 1f, 0.25f), 0.0001f);
            Assert.AreEqual(-0.25f, FormMath.Step(0f, -1f, 0.25f), 0.0001f);
            Assert.AreEqual(0.1f, FormMath.Step(0f, 0.1f, 0.25f), 0.0001f);
        }

        [Test]
        public void StepStaysInsideMinusOneToOne()
        {
            Assert.AreEqual(1f, FormMath.Step(0.9f, 1f, 0.25f), 0.0001f);
            Assert.AreEqual(-1f, FormMath.Step(-0.9f, -1f, 0.25f), 0.0001f);
            Assert.AreEqual(-1f, FormMath.Step(-0.95f, -5f, 0.25f), 0.0001f);
        }

        [Test]
        public void TargetIsClampedAndCombinesTheThreeTerms()
        {
            Assert.AreEqual(-1f, FormMath.Target(0f, 2f, 5, 1, MaxCombo, _settings), 0.0001f);
            Assert.AreEqual(1f, FormMath.Target(10f, 2f, 0, MaxCombo, MaxCombo, _settings), 0.0001f);
            Assert.AreEqual(0f, FormMath.Target(2f, 2f, 0, 1, MaxCombo, _settings), 0.0001f);
        }

        [Test]
        public void FormStaysZeroDuringTheWarmup()
        {
            _form.Tick(At(15f, 10f, 11f, 12f));
            Assert.AreEqual(0f, _form.Value);
        }

        [Test]
        public void FormUpdatesOnlyEveryUpdateInterval()
        {
            _form.Tick(At(45f, 40f, 41f));
            var first = _form.Value;
            _form.Tick(At(50f, 40f, 41f));
            Assert.AreEqual(first, _form.Value);
            Assert.AreEqual(-0.25f, first, 0.0001f);
        }

        [Test]
        public void AngryPatronsDragFormDownOneStepAtATime()
        {
            for (var t = 45f; t <= 45f + 15f * 6f; t += 15f) _form.Tick(At(t, t - 5f, t - 4f));
            Assert.AreEqual(-1f, _form.Value, 0.0001f);
        }

        [Test]
        public void TipsAheadOfPaceAndAComboPushFormUp()
        {
            var score = new FakeScore();
            _form.Dispose();
            _form = new FormTracker(_bus, _settings, _clock, score, MaxCombo);
            _bus.Publish(new ScoreChanged(0, MaxCombo, 0, 0));
            _clock.SetElapsed(40f);
            for (var i = 0; i < 6; i++) _bus.Publish(new TipEarned(new TicketId(i + 1), 100));
            _form.Tick(At(45f));
            Assert.Greater(_form.Value, 0f);
        }

        [Test]
        public void NewShiftResetsFormAndCombo()
        {
            _form.Tick(At(45f, 40f, 41f));
            _bus.Publish(new ShiftStarted());
            Assert.AreEqual(0f, _form.Value);
        }

        private sealed class FakeScore : IShiftScoreView
        {
            public int Score => 0;
            public int ComboMultiplier => 1;
            public int Complaints => 0;
            public int Stars => 0;
            public int OneStarScore => 150;
            public int TwoStarScore => 300;
            public int ThreeStarScore => 450;
        }
    }
}
