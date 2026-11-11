using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Director;
using UnityEngine;

namespace UncannyValet.Tests.Director
{
    public class TunedIntensityProfileTests
    {
        private const float Tolerance = 0.0001f;
        private const float RisingLookahead = 0.03f;

        private DirectorProfile _lot;
        private TuningStore _tuning;
        private ShapeSettings _shape;
        private TunedIntensityProfile _tuned;

        [SetUp]
        public void SetUp()
        {
            _lot = ScriptableObject.CreateInstance<DirectorProfile>();
            _tuning = new TuningStore(new EventBus());
            ShapeSettings.Seed(_tuning, _lot.Weights);
            _shape = new ShapeSettings();
            _shape.Read(_tuning);
            _tuned = new TunedIntensityProfile(_lot, _shape, _lot.RisingLookahead);
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_lot);

        private void Set(TuningKey key, float value)
        {
            _tuning.Set(key, value);
            _shape.Read(_tuning);
        }

        [Test]
        public void NeutralShape_AnswersExactlyLikeTheLotProfile()
        {
            Assert.IsTrue(_shape.IsNeutral);
            for (var f = 0f; f <= 1f; f += 0.05f)
            {
                Assert.AreEqual(_lot.Sample(f), _tuned.Sample(f));
                Assert.AreEqual(_lot.IsRising(f), _tuned.IsRising(f));
                Assert.AreEqual(_lot.AreaFrom(f, 240f), _tuned.AreaFrom(f, 240f));
            }
            Assert.AreEqual(_lot.Weights.Dirty, _tuned.Weights.Dirty);
            Assert.AreEqual(_lot.PhaseAt(0.5f), _tuned.PhaseAt(0.5f));
        }

        [Test]
        public void TargetScale_ScalesTheWholeCurve()
        {
            Set(TuningKey.DirTargetScale, 2f);
            Assert.AreEqual(_lot.Sample(0.3f) * 2f, _tuned.Sample(0.3f), Tolerance);
            Assert.AreEqual(_lot.AreaFrom(0f, 240f) * 2f, _tuned.AreaFrom(0f, 240f), 0.01f);
        }

        [Test]
        public void ZeroRamp_FlattensTheCurveToItsAverage()
        {
            Set(TuningKey.DirRampShape, 0f);
            var average = _lot.AreaFrom(0f, 1f);
            Assert.AreEqual(average, _tuned.Sample(0f), Tolerance);
            Assert.AreEqual(average, _tuned.Sample(0.85f), Tolerance);
            Assert.IsFalse(_tuned.IsRising(0.1f), "a flat curve never rises");
        }

        [Test]
        public void SteeperRamp_WidensTheSpreadAroundTheAverage()
        {
            var spread = Mathf.Abs(_lot.Sample(0f) - _lot.Sample(0.3f));
            Set(TuningKey.DirRampShape, 1.5f);
            Assert.AreEqual(spread * 1.5f, Mathf.Abs(_tuned.Sample(0f) - _tuned.Sample(0.3f)), Tolerance);
        }

        [Test]
        public void LateBoost_LiftsOnlyTheEndOfTheShiftAndNeverGoesNegative()
        {
            Set(TuningKey.DirLateTargetBoost, 0.4f);
            Assert.AreEqual(_lot.Sample(0.3f), _tuned.Sample(0.3f), Tolerance, "before the surge starts");
            Assert.AreEqual(_lot.Sample(1f) * 1.4f, _tuned.Sample(1f), Tolerance);
            Set(TuningKey.DirLateTargetBoost, -5f);
            Assert.AreEqual(0f, _tuned.Sample(1f));
        }

        [Test]
        public void SeededWeights_ReplaceTheLotWeights()
        {
            Set(TuningKey.DirWeightDirty, 40f);
            Assert.AreEqual(40f, _tuned.Weights.Dirty);
            Assert.AreEqual(_lot.Weights.Plain, _tuned.Weights.Plain);
            Assert.AreEqual(RisingLookahead, _lot.RisingLookahead, Tolerance);
        }
    }
}
