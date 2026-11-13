using NUnit.Framework;
using UncannyValet.Bootstrap;
using UncannyValet.Core.Config;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Director;
using UncannyValet.Economy;
using UncannyValet.Patrons;
using UnityEngine;

namespace UncannyValet.Tests.Run
{
    /// <summary>The lot's numbers win over the shared configs when Bootstrap seeds the tuning store.</summary>
    public sealed class LotSeedingTests
    {
        private RunFixtures _fx;
        private ShiftConfig _shift;
        private PatronConfig _patrons;
        private TipConfig _tips;

        [SetUp]
        public void SetUp()
        {
            _fx = new RunFixtures();
            _shift = ScriptableObject.CreateInstance<ShiftConfig>();
            _patrons = ScriptableObject.CreateInstance<PatronConfig>();
            _tips = ScriptableObject.CreateInstance<TipConfig>();
        }

        [TearDown]
        public void TearDown()
        {
            _fx.Dispose();
            Object.DestroyImmediate(_shift);
            Object.DestroyImmediate(_patrons);
            Object.DestroyImmediate(_tips);
        }

        [Test]
        public void Seed_WithALot_UsesItsShiftLengthAndPacing()
        {
            var lot = _fx.Lot(World.Midgard, LotKind.Training);
            var tuning = new TuningStore(new EventBus());
            TuningSeeder.Seed(tuning, _shift, _patrons, _tips, 6, lot: lot);
            Assert.AreEqual(lot.Pacing.ShiftLengthSeconds, tuning.Get(TuningKey.ShiftLengthSeconds));
            Assert.AreEqual(lot.Pacing.ArrivalGapMinSeconds, tuning.Get(TuningKey.ArrivalGapMinMinutes));
            Assert.AreEqual(lot.Pacing.MaxWaitingCars, tuning.Get(TuningKey.PlanMaxWaitingCars));
            Assert.AreNotEqual(_shift.ShiftLengthSeconds, tuning.Get(TuningKey.ShiftLengthSeconds));
        }

        [Test]
        public void Seed_WithoutALot_KeepsTheSharedDefaults()
        {
            var tuning = new TuningStore(new EventBus());
            TuningSeeder.Seed(tuning, _shift, _patrons, _tips, 6);
            Assert.AreEqual(_shift.ShiftLengthSeconds, tuning.Get(TuningKey.ShiftLengthSeconds));
            Assert.AreEqual(_patrons.ArrivalIntervalRange.x, tuning.Get(TuningKey.ArrivalGapMinMinutes));
        }

        [Test]
        public void Seed_WithALot_AppliesItsDialPositionBeforeTheDialsDeriveTheirKeys()
        {
            var lot = _fx.Lot(World.Midgard, LotKind.Crazy);
            var so = new UnityEditor.SerializedObject(lot);
            var dials = so.FindProperty("_dials");
            dials.arraySize = 1;
            dials.GetArrayElementAtIndex(0).FindPropertyRelative("_dial").intValue = (int)TuningKey.DialIntensity;
            dials.GetArrayElementAtIndex(0).FindPropertyRelative("_position").floatValue = 2f;
            so.ApplyModifiedPropertiesWithoutUndo();
            var tuning = new TuningStore(new EventBus());
            var director = ScriptableObject.CreateInstance<DirectorConfig>();
            var profile = ScriptableObject.CreateInstance<DirectorProfile>();
            try
            {
                TuningSeeder.Seed(tuning, _shift, _patrons, _tips, 6, director, profile, bus: new EventBus(), lot: lot);
                Assert.AreEqual(2f, tuning.Get(TuningKey.DialIntensity));
            }
            finally
            {
                Object.DestroyImmediate(director);
                Object.DestroyImmediate(profile);
            }
        }
    }
}
