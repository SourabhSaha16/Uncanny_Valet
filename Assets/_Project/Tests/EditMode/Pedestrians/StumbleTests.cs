using NUnit.Framework;
using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Pedestrians;
using UnityEngine;

namespace UncannyValet.Tests.Pedestrians
{
    /// <summary>The stumble of a pedestrian the boosted valet runs through (car_handling_spec.md row 8).</summary>
    public class StumbleTests
    {
        [Test]
        public void Rule_PushesAwayAndSpinsThenSettlesAfterTheTime()
        {
            var rule = new PedestrianStumbleRule();
            rule.Configure(1f, 1f);
            Assert.IsTrue(rule.TryStart(Vector2.right * 5f, 0.4f));
            rule.Step(0.1f);
            Assert.IsTrue(rule.Active);
            Assert.Greater(rule.Offset.x, 0.1f);
            Assert.AreEqual(0f, rule.Offset.y, 0.0001f);
            Assert.AreEqual(90f, rule.SpinDegrees, 0.01f);
            rule.Step(0.31f);
            Assert.IsFalse(rule.Active);
            Assert.AreEqual(Vector2.zero, rule.Offset);
            Assert.AreEqual(0f, rule.SpinDegrees);
        }

        [Test]
        public void Rule_IgnoresAStartWhileRunningOrWithNothingToPushAlong()
        {
            var rule = new PedestrianStumbleRule();
            Assert.IsFalse(rule.TryStart(Vector2.zero, 0.4f));
            Assert.IsFalse(rule.TryStart(Vector2.up, 0f));
            Assert.IsTrue(rule.TryStart(Vector2.up, 0.4f));
            Assert.IsFalse(rule.TryStart(Vector2.down, 0.4f));
        }

        [Test]
        public void Stumbler_ReactsOnlyToItsOwnIdAndStopsAfterUnsubscribe()
        {
            var config = ScriptableObject.CreateInstance<PedestrianConfig>();
            var go = new GameObject("walker");
            try
            {
                var bus = new EventBus();
                var stumbler = new WalkerStumbler(bus, null, config, new WalkerSprites(go.transform, config), 5);
                var started = 0;
                stumbler.Started += () => started++;
                stumbler.Subscribe();
                bus.Publish(new ValetBargedPedestrian(new PedestrianId(6), 1f, 0f));
                Assert.IsFalse(stumbler.Active);
                bus.Publish(new ValetBargedPedestrian(new PedestrianId(5), 1f, 0f));
                Assert.IsTrue(stumbler.Active);
                Assert.AreEqual(1, started);
                stumbler.Step(config.StumbleSeconds + 0.01f);
                Assert.IsFalse(stumbler.Active);
                stumbler.Unsubscribe();
                bus.Publish(new ValetBargedPedestrian(new PedestrianId(5), 1f, 0f));
                Assert.IsFalse(stumbler.Active);
            }
            finally
            {
                Object.DestroyImmediate(go);
                Object.DestroyImmediate(config);
            }
        }

        [Test]
        public void Stumbler_TakesItsLengthFromTheTuningStore()
        {
            var config = ScriptableObject.CreateInstance<PedestrianConfig>();
            var go = new GameObject("walker");
            try
            {
                var bus = new EventBus();
                var tuning = new TuningStore(bus);
                tuning.Seed(TuningKey.PowerStumbleSeconds, 1.5f);
                var stumbler = new WalkerStumbler(bus, tuning, config, new WalkerSprites(go.transform, config), 5);
                stumbler.Subscribe();
                bus.Publish(new ValetBargedPedestrian(new PedestrianId(5), 0f, 1f));
                stumbler.Step(config.StumbleSeconds + 0.1f);
                Assert.IsTrue(stumbler.Active, "the F2 value (1.5 s) beats the config fallback (0.4 s)");
                stumbler.Unsubscribe();
            }
            finally
            {
                Object.DestroyImmediate(go);
                Object.DestroyImmediate(config);
            }
        }
    }
}
