using NUnit.Framework;
using UncannyValet.Core.Events;
using UncannyValet.Director;
using UnityEditor;
using UnityEngine;

namespace UncannyValet.Tests.Director
{
    public class DirectorProfileTests
    {
        private const float ShiftSeconds = 240f;

        private DirectorProfile _profile;

        [SetUp]
        public void SetUp() => _profile = ScriptableObject.CreateInstance<DirectorProfile>();

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_profile);

        private void SetCurve(AnimationCurve curve)
        {
            var so = new SerializedObject(_profile);
            so.FindProperty("_intensity").animationCurveValue = curve;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        [Test]
        public void DefaultCurveStartsGentleAndEndsAtItsHighest()
        {
            Assert.AreEqual(0.35f, _profile.Sample(0f), 0.001f);
            Assert.AreEqual(1.15f, _profile.Sample(1f), 0.001f);
            Assert.Greater(_profile.Sample(0.6f), _profile.Sample(0.42f), "harder peak after the breather");
            Assert.Less(_profile.Sample(0.42f), _profile.Sample(0.30f), "breather after the first peak");
        }

        [Test]
        public void SamplingClampsOutsideTheShift()
        {
            Assert.AreEqual(_profile.Sample(0f), _profile.Sample(-3f), 0.0001f);
            Assert.AreEqual(_profile.Sample(1f), _profile.Sample(7f), 0.0001f);
        }

        [Test]
        public void ConstantCurveSamplesItsValueAndIntegratesToIt()
        {
            SetCurve(AnimationCurve.Constant(0f, 1f, 0.8f));
            Assert.AreEqual(0.8f, _profile.Sample(0.37f), 0.0001f);
            Assert.AreEqual(0.8f * ShiftSeconds, _profile.AreaFrom(0f, ShiftSeconds), 0.01f);
            Assert.AreEqual(0.8f * ShiftSeconds * 0.5f, _profile.AreaFrom(0.5f, ShiftSeconds), 0.01f);
        }

        [Test]
        public void NegativeCurveValuesSampleAsZero()
        {
            SetCurve(AnimationCurve.Constant(0f, 1f, -2f));
            Assert.AreEqual(0f, _profile.Sample(0.5f));
        }

        [Test]
        public void AreaShrinksToZeroAtTheEnd()
        {
            Assert.Greater(_profile.AreaFrom(0f, ShiftSeconds), _profile.AreaFrom(0.5f, ShiftSeconds));
            Assert.AreEqual(0f, _profile.AreaFrom(1f, ShiftSeconds), 0.0001f);
        }

        [Test]
        public void PhasesFollowTheShiftInOrder()
        {
            Assert.AreEqual(DirectorPhase.BuildUp, _profile.PhaseAt(0f));
            Assert.AreEqual(DirectorPhase.Peak, _profile.PhaseAt(0.3f));
            Assert.AreEqual(DirectorPhase.Breather, _profile.PhaseAt(0.4f));
            Assert.AreEqual(DirectorPhase.HarderPeak, _profile.PhaseAt(0.55f));
            Assert.AreEqual(DirectorPhase.Breather, _profile.PhaseAt(0.7f));
            Assert.AreEqual(DirectorPhase.Finale, _profile.PhaseAt(0.95f));
            Assert.AreEqual(DirectorPhase.Finale, _profile.PhaseAt(1f));
        }

        [Test]
        public void RisingMeansTheCurveIsHigherJustAhead()
        {
            Assert.IsTrue(_profile.IsRising(0.05f), "build-up climbs");
            Assert.IsFalse(_profile.IsRising(0.34f), "falling into the breather");
            Assert.IsFalse(_profile.IsRising(1f), "nothing is ahead of the end");
        }

        [Test]
        public void WeightsCarryTheFourBaseWeightsInArchetypeOrder()
        {
            var w = _profile.Weights;
            Assert.AreEqual(50f, w.Of(CarArchetype.Plain));
            Assert.AreEqual(20f, w.Of(CarArchetype.Muddy));
            Assert.AreEqual(20f, w.Of(CarArchetype.Ev));
            Assert.AreEqual(10f, w.Of(CarArchetype.MuddyEv));
        }

        [Test]
        public void DinerDefaultsAreFourteenCarsPlusTwoSlackAndTheSpecWeights()
        {
            Assert.AreEqual(14, _profile.ArrivalBudget);
            Assert.AreEqual(2, _profile.BudgetSlack);
            Assert.AreEqual(50f, _profile.WeightOf(CarArchetype.Plain));
            Assert.AreEqual(20f, _profile.WeightOf(CarArchetype.Muddy));
            Assert.AreEqual(20f, _profile.WeightOf(CarArchetype.Ev));
            Assert.AreEqual(10f, _profile.WeightOf(CarArchetype.MuddyEv));
        }
    }
}
