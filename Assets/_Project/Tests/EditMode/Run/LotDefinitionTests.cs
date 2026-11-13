using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Run;

namespace UncannyValet.Tests.Run
{
    public sealed class LotDefinitionTests
    {
        private RunFixtures _fx;
        private LotDefinition _lot;

        [SetUp]
        public void SetUp()
        {
            _fx = new RunFixtures();
            _lot = _fx.Lot(World.Midgard, LotKind.Crazy, "Crazy", "Fair Night at Grandma's");
        }

        [TearDown] public void TearDown() { _fx.Dispose(); }

        [Test]
        public void Defaults_AreValid()
        {
            Assert.IsTrue(_lot.TryValidate(out var problem), problem);
        }

        [TestCase(0f)]
        [TestCase(-5f)]
        public void ShiftLengthMustBeAboveZero(float seconds)
        {
            RunFixtures.Set(_lot, "_pacing._shiftLengthSeconds", seconds);
            Assert.IsFalse(_lot.TryValidate(out var problem));
            StringAssert.Contains("shift length", problem);
        }

        [Test]
        public void ArrivalGaps_MustBePositiveAndOrdered()
        {
            RunFixtures.Set(_lot, "_pacing._arrivalGapMinSeconds", 20f);
            RunFixtures.Set(_lot, "_pacing._arrivalGapMaxSeconds", 10f);
            Assert.IsFalse(_lot.TryValidate(out var problem));
            StringAssert.Contains("gap", problem);
        }

        [Test]
        public void AtLeastOneCarMustBeAllowedToWait()
        {
            RunFixtures.Set(_lot, "_pacing._maxWaitingCars", 0f);
            Assert.IsFalse(_lot.TryValidate(out _));
        }

        [Test]
        public void IdMustBeARealLot()
        {
            RunFixtures.Set(_lot, "_id._kind", (float)LotKind.Finale); // Midgard Finale
            Assert.IsFalse(_lot.TryValidate(out var problem));
            StringAssert.Contains("LotId", problem);
        }

        [Test]
        public void ThresholdSceneAndNameAreRequired()
        {
            RunFixtures.Set(_lot, "_blessingThreshold", 0f);
            Assert.IsFalse(_lot.TryValidate(out _));
            RunFixtures.Set(_lot, "_blessingThreshold", 3f);
            RunFixtures.SetString(_lot, "_sceneName", "");
            Assert.IsFalse(_lot.TryValidate(out _));
        }

        [Test]
        public void TrainingLot_CannotGrantAPower()
        {
            var training = _fx.Lot(World.Midgard, LotKind.Training);
            RunFixtures.Set(training, "_grant._power", (float)StoryPower.ThunderStride);
            RunFixtures.Set(training, "_grant._level", 1f);
            Assert.IsFalse(training.TryValidate(out _));
            RunFixtures.Set(_lot, "_grant._power", (float)StoryPower.ThunderStride);
            RunFixtures.Set(_lot, "_grant._level", 1f);
            Assert.IsTrue(_lot.TryValidate(out var problem), problem);
        }

        [Test]
        public void LotId_RulesForAsgardAndNone()
        {
            Assert.IsTrue(new LotId(World.Asgard, LotKind.Finale).IsValid);
            Assert.IsFalse(new LotId(World.Asgard, LotKind.Crazy).IsValid);
            Assert.IsFalse(new LotId(World.Helheim, LotKind.Finale).IsValid);
            Assert.IsFalse(default(LotId).IsValid);
            Assert.AreEqual(new LotId(World.Midgard, LotKind.Training), new LotId(World.Midgard, LotKind.Training));
            Assert.AreNotEqual(new LotId(World.Midgard, LotKind.Training), new LotId(World.Midgard, LotKind.Crazy));
        }

        [Test]
        public void SeedPacing_WritesShiftLengthGapsAndWaitingCap()
        {
            RunFixtures.Set(_lot, "_pacing._shiftLengthSeconds", 150f);
            RunFixtures.Set(_lot, "_pacing._arrivalGapMinSeconds", 15f);
            RunFixtures.Set(_lot, "_pacing._arrivalGapMaxSeconds", 25f);
            RunFixtures.Set(_lot, "_pacing._maxWaitingCars", 2f);
            var tuning = new TuningStore(new EventBus());
            _lot.SeedPacing(tuning, 1f);
            Assert.AreEqual(150f, tuning.Get(TuningKey.ShiftLengthSeconds));
            Assert.AreEqual(15f, tuning.Get(TuningKey.ArrivalGapMinMinutes));
            Assert.AreEqual(25f, tuning.Get(TuningKey.ArrivalGapMaxMinutes));
            Assert.AreEqual(2f, tuning.Get(TuningKey.PlanMaxWaitingCars));
        }

        [Test]
        public void SeedPacing_ScalesGapsToGameMinutes_AndCapsWaitingAtTheQueueSlots()
        {
            RunFixtures.Set(_lot, "_pacing._maxWaitingCars", 4f);
            var tuning = new TuningStore(new EventBus());
            _lot.SeedPacing(tuning, 2f);
            Assert.AreEqual(3f, tuning.Get(TuningKey.PlanMaxWaitingCars));
            Assert.AreEqual(_lot.Pacing.ArrivalGapMinSeconds * 2f, tuning.Get(TuningKey.ArrivalGapMinMinutes), 0.0001f);
            Assert.AreEqual(1, _lot.Pacing.EffectiveMaxWaiting(0));
        }

        [Test]
        public void DialPositions_ValidateAndSeed()
        {
            var tuning = new TuningStore(new EventBus());
            tuning.Seed(TuningKey.DialIntensity, 8f);
            Assert.IsTrue(new DialPosition(TuningKey.DialIntensity, 3f).IsValid);
            Assert.IsFalse(new DialPosition(TuningKey.PlanEvChance, 3f).IsValid);
            Assert.IsFalse(new DialPosition(TuningKey.DialMercy, 11f).IsValid);
            var so = new UnityEditor.SerializedObject(_lot);
            var dials = so.FindProperty("_dials");
            dials.arraySize = 1;
            dials.GetArrayElementAtIndex(0).FindPropertyRelative("_dial").intValue = (int)TuningKey.DialIntensity;
            dials.GetArrayElementAtIndex(0).FindPropertyRelative("_position").floatValue = 3f;
            so.ApplyModifiedPropertiesWithoutUndo();
            Assert.IsTrue(_lot.TryValidate(out var problem), problem);
            _lot.SeedDialPositions(tuning);
            Assert.AreEqual(3f, tuning.Get(TuningKey.DialIntensity));
        }
    }
}
