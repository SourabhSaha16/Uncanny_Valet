using NUnit.Framework;
using UncannyValet.Bootstrap;
using UncannyValet.Core.Config;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
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
            try
            {
                TuningSeeder.Seed(tuning, shift, patrons, tips, 12);
                foreach (var info in TuningCatalog.All)
                    Assert.IsTrue(tuning.TryGet(info.Key, out _), info.Key + " was not seeded");
            }
            finally
            {
                Object.DestroyImmediate(shift);
                Object.DestroyImmediate(patrons);
                Object.DestroyImmediate(tips);
            }
        }

        [Test]
        public void Seed_UsesTheShiftLengthAndSpotCountFallback()
        {
            var tuning = new TuningStore(new EventBus());
            var shift = ScriptableObject.CreateInstance<ShiftConfig>();
            var patrons = ScriptableObject.CreateInstance<PatronConfig>();
            var tips = ScriptableObject.CreateInstance<TipConfig>();
            try
            {
                TuningSeeder.Seed(tuning, shift, patrons, tips, 12);
                Assert.AreEqual(240f, tuning.Get(TuningKey.ShiftLengthSeconds));
                Assert.AreEqual(12f, tuning.Get(TuningKey.PlanActiveSpots), "a plan of 0 opens every spot");
                Assert.AreEqual(15f, patrons.ArrivalIntervalRange.x);
                Assert.AreEqual(25f, patrons.ArrivalIntervalRange.y);
            }
            finally
            {
                Object.DestroyImmediate(shift);
                Object.DestroyImmediate(patrons);
                Object.DestroyImmediate(tips);
            }
        }
    }
}
