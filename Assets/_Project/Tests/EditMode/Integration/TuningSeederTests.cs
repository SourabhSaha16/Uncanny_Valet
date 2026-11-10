using NUnit.Framework;
using UncannyValet.Bootstrap;
using UncannyValet.Core.Config;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Director;
using UncannyValet.Economy;
using UncannyValet.Patrons;
using UnityEngine;

namespace UncannyValet.Tests.Integration
{
    public class TuningSeederTests
    {
        [Test]
        public void Seed_SeedsEveryKeyInTheCatalog()
        {
            var tuning = new TuningStore(new EventBus());
            var shift = ScriptableObject.CreateInstance<ShiftConfig>();
            var patrons = ScriptableObject.CreateInstance<PatronConfig>();
            var tips = ScriptableObject.CreateInstance<TipConfig>();
            var director = ScriptableObject.CreateInstance<DirectorConfig>();
            var profile = ScriptableObject.CreateInstance<DirectorProfile>();
            try
            {
                TuningSeeder.Seed(tuning, shift, patrons, tips, 9, director, profile);
                foreach (var info in TuningCatalog.All)
                    Assert.IsTrue(tuning.TryGet(info.Key, out _), info.Key + " was not seeded");
            }
            finally
            {
                Object.DestroyImmediate(shift);
                Object.DestroyImmediate(patrons);
                Object.DestroyImmediate(tips);
                Object.DestroyImmediate(director);
                Object.DestroyImmediate(profile);
            }
        }

        [Test]
        public void Seed_SeedsTheDirectorFromItsConfigAndProfile()
        {
            var tuning = new TuningStore(new EventBus());
            var director = ScriptableObject.CreateInstance<DirectorConfig>();
            var profile = ScriptableObject.CreateInstance<DirectorProfile>();
            var shift = ScriptableObject.CreateInstance<ShiftConfig>();
            var patrons = ScriptableObject.CreateInstance<PatronConfig>();
            var tips = ScriptableObject.CreateInstance<TipConfig>();
            try
            {
                TuningSeeder.Seed(tuning, shift, patrons, tips, 9, director, profile);
                Assert.AreEqual(1f, tuning.Get(TuningKey.DirEnabled));
                Assert.AreEqual(14f, tuning.Get(TuningKey.DirArrivalBudget));
                Assert.AreEqual(2f, tuning.Get(TuningKey.DirBudgetSlack));
                Assert.AreEqual(1.4f, tuning.Get(TuningKey.DirReliefWorkload), 0.0001f);
                Assert.AreEqual(3f, tuning.Get(TuningKey.DirDilemmaWashBlocked), 0.0001f);
                Assert.AreEqual(25f, tuning.Get(TuningKey.DirDilemmaCooldown), 0.0001f);
                Assert.AreEqual(2f, tuning.Get(TuningKey.DirStreakLimit), 0.0001f);
                Assert.AreEqual(0f, tuning.Get(TuningKey.DirAssist), 0.0001f);
            }
            finally
            {
                Object.DestroyImmediate(shift);
                Object.DestroyImmediate(patrons);
                Object.DestroyImmediate(tips);
                Object.DestroyImmediate(director);
                Object.DestroyImmediate(profile);
            }
        }

        [Test]
        public void Seed_UsesTheShiftLengthAndSpotCountFallback()
        {
            var tuning = new TuningStore(new EventBus());
            var shift = ScriptableObject.CreateInstance<ShiftConfig>();
            var patrons = ScriptableObject.CreateInstance<PatronConfig>();
            var tips = ScriptableObject.CreateInstance<TipConfig>();
            var director = ScriptableObject.CreateInstance<DirectorConfig>();
            var profile = ScriptableObject.CreateInstance<DirectorProfile>();
            try
            {
                TuningSeeder.Seed(tuning, shift, patrons, tips, 9, director, profile);
                Assert.AreEqual(240f, tuning.Get(TuningKey.ShiftLengthSeconds));
                Assert.AreEqual(9f, tuning.Get(TuningKey.PlanActiveSpots), "a plan of 0 opens every spot");
                Assert.AreEqual(15f, patrons.ArrivalIntervalRange.x);
                Assert.AreEqual(25f, patrons.ArrivalIntervalRange.y);
            }
            finally
            {
                Object.DestroyImmediate(shift);
                Object.DestroyImmediate(patrons);
                Object.DestroyImmediate(tips);
                Object.DestroyImmediate(director);
                Object.DestroyImmediate(profile);
            }
        }
    }
}
