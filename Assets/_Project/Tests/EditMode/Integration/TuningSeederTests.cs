using NUnit.Framework;
using UncannyValet.Bootstrap;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Economy;
using UncannyValet.Orders;
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
            var run = ScriptableObject.CreateInstance<RunConfig>();
            var patrons = ScriptableObject.CreateInstance<PatronConfig>();
            var tips = ScriptableObject.CreateInstance<TipConfig>();
            var orders = ScriptableObject.CreateInstance<OrdersConfig>();
            try
            {
                TuningSeeder.Seed(tuning, run, patrons, tips, orders, new NightPlan(), 12);
                foreach (var info in TuningCatalog.All)
                    Assert.IsTrue(tuning.TryGet(info.Key, out _), info.Key + " was not seeded");
            }
            finally
            {
                Object.DestroyImmediate(run);
                Object.DestroyImmediate(patrons);
                Object.DestroyImmediate(tips);
                Object.DestroyImmediate(orders);
            }
        }

        [Test]
        public void OrdersConfig_SeedsOrderKeysWithItsValues()
        {
            var tuning = new TuningStore(new EventBus());
            var orders = ScriptableObject.CreateInstance<OrdersConfig>();
            try
            {
                orders.SeedInto(tuning);
                Assert.AreEqual(orders.MaxStrikes, tuning.Get(TuningKey.MaxStrikes));
                Assert.AreEqual(orders.MaxOpenOrders, tuning.Get(TuningKey.MaxOpenOrders));
            }
            finally { Object.DestroyImmediate(orders); }
        }
    }
}
