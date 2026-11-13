using NUnit.Framework;
using UncannyValet.Bootstrap;
using UncannyValet.Core.Config;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Director;
using UncannyValet.Economy;
using UncannyValet.Patrons;
using UncannyValet.Run;
using UnityEngine;

namespace UncannyValet.Tests.Run
{
    /// <summary>The per-lot queue slot count and the per-lot Director arrival budget (issue 80 follow-up).</summary>
    public sealed class QueueSlotsAndBudgetTests
    {
        private RunFixtures _fx;
        private ShiftConfig _shift;
        private PatronConfig _patrons;
        private BlessingConfig _blessings;

        [SetUp]
        public void SetUp()
        {
            _fx = new RunFixtures();
            _shift = ScriptableObject.CreateInstance<ShiftConfig>();
            _patrons = ScriptableObject.CreateInstance<PatronConfig>();
            _blessings = ScriptableObject.CreateInstance<BlessingConfig>();
        }

        [TearDown]
        public void TearDown()
        {
            _fx.Dispose();
            Object.DestroyImmediate(_shift);
            Object.DestroyImmediate(_patrons);
            Object.DestroyImmediate(_blessings);
        }

        private LotDefinition Lot(float shift, float gapMin, float gapMax, int slots, int waiting)
        {
            var lot = _fx.Lot(World.Midgard, LotKind.Crazy);
            RunFixtures.Set(lot, "_pacing._shiftLengthSeconds", shift);
            RunFixtures.Set(lot, "_pacing._arrivalGapMinSeconds", gapMin);
            RunFixtures.Set(lot, "_pacing._arrivalGapMaxSeconds", gapMax);
            RunFixtures.Set(lot, "_pacing._maxWaitingCars", waiting);
            RunFixtures.Set(lot, "_queueSlots", slots);
            return lot;
        }

        [Test]
        public void QueueSlots_DefaultToThree_AndValidateBetweenOneAndTheMaximum()
        {
            var lot = _fx.Lot(World.Midgard, LotKind.Training);
            Assert.AreEqual(3, lot.QueueSlots);
            RunFixtures.Set(lot, "_queueSlots", 0);
            Assert.IsFalse(lot.TryValidate(out _));
            RunFixtures.Set(lot, "_queueSlots", LotDefinition.MaxQueueSlots + 1);
            Assert.IsFalse(lot.TryValidate(out _));
            RunFixtures.Set(lot, "_queueSlots", LotDefinition.MaxQueueSlots);
            Assert.IsTrue(lot.TryValidate(out var problem), problem);
        }

        [Test]
        public void EffectiveWaitingCap_IsTheSmallerOfTheCapAndTheSlots()
        {
            var four = Lot(240f, 10f, 15f, 4, 4);
            var three = Lot(240f, 10f, 15f, 3, 4);
            Assert.AreEqual(4, four.Pacing.EffectiveMaxWaiting(four.QueueSlots));
            Assert.AreEqual(3, three.Pacing.EffectiveMaxWaiting(three.QueueSlots));
            var tuning = new TuningStore(new EventBus());
            four.SeedPacing(tuning, 1f);
            Assert.AreEqual(4f, tuning.Get(TuningKey.PlanMaxWaitingCars));
            three.SeedPacing(tuning, 1f);
            Assert.AreEqual(3f, tuning.Get(TuningKey.PlanMaxWaitingCars));
        }

        [Test]
        public void ArrivalBudget_IsDerivedFromShiftLengthAndMeanGap_RoundedUp()
        {
            Assert.AreEqual(8, Lot(150f, 15f, 25f, 3, 2).Pacing.ArrivalBudget);
            Assert.AreEqual(20, Lot(240f, 10f, 15f, 4, 4).Pacing.ArrivalBudget);
            Assert.AreEqual(LotPacing.MaxArrivalBudget, Lot(240f, 0.5f, 0.5f, 4, 4).Pacing.ArrivalBudget);
        }

        [Test]
        public void ArrivalBudget_AnAuthoredValueWinsAndNegativeIsRejected()
        {
            var lot = Lot(240f, 10f, 15f, 4, 4);
            RunFixtures.Set(lot, "_pacing._arrivalBudget", 9);
            Assert.AreEqual(9, lot.Pacing.ArrivalBudget);
            RunFixtures.Set(lot, "_pacing._arrivalBudget", -1);
            Assert.IsFalse(lot.TryValidate(out _));
        }

        [Test]
        public void SeedPacing_WritesTheLotsBudgetIntoTheDirectorKey()
        {
            var tuning = new TuningStore(new EventBus());
            Lot(150f, 15f, 25f, 3, 2).SeedPacing(tuning, 1f);
            Assert.AreEqual(8f, tuning.Get(TuningKey.DirArrivalBudget));
            Lot(240f, 10f, 15f, 4, 4).SeedPacing(tuning, 1f);
            Assert.AreEqual(20f, tuning.Get(TuningKey.DirArrivalBudget));
        }

        [Test]
        public void Seed_GivesEachLotItsOwnDirectorBudget_BeforeTheDialsCaptureTheirBaselines()
        {
            var director = ScriptableObject.CreateInstance<DirectorConfig>();
            var profile = ScriptableObject.CreateInstance<DirectorProfile>();
            try
            {
                var none = new TuningStore(new EventBus());
                var a = new TuningStore(new EventBus());
                var b = new TuningStore(new EventBus());
                TuningSeeder.Seed(none, _shift, _patrons, _blessings, 6, director, profile, bus: new EventBus());
                TuningSeeder.Seed(a, _shift, _patrons, _blessings, 6, director, profile, bus: new EventBus(), lot: Lot(150f, 15f, 25f, 3, 2));
                TuningSeeder.Seed(b, _shift, _patrons, _blessings, 6, director, profile, bus: new EventBus(), lot: Lot(240f, 10f, 15f, 4, 4));
                var shared = none.Get(TuningKey.DirArrivalBudget);
                Assert.Less(a.Get(TuningKey.DirArrivalBudget), shared, "the training lot's own budget replaces the shared one");
                Assert.Greater(b.Get(TuningKey.DirArrivalBudget), shared, "the crazy lot is no longer capped by the shared one");
                Assert.AreEqual(none.Get(TuningKey.DirBudgetSlack), a.Get(TuningKey.DirBudgetSlack), "slack stays the profile's");
            }
            finally
            {
                Object.DestroyImmediate(director);
                Object.DestroyImmediate(profile);
            }
        }
    }
}
