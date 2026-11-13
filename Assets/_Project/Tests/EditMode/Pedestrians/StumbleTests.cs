using NUnit.Framework;
using UncannyValet.Pedestrians;
using UnityEngine;

namespace UncannyValet.Tests.Pedestrians
{
    /// <summary>The stumble of a pedestrian who is knocked over (reused by the pedestrian knockback power).</summary>
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
        public void Stumbler_StartsOnRequestUsesTheConfigLengthAndSettles()
        {
            var config = ScriptableObject.CreateInstance<PedestrianConfig>();
            var go = new GameObject("walker");
            try
            {
                var stumbler = new WalkerStumbler(config, new WalkerSprites(go.transform, config));
                var started = 0;
                stumbler.Started += () => started++;
                Assert.IsFalse(stumbler.TryStart(Vector2.zero));
                Assert.IsTrue(stumbler.TryStart(Vector2.right));
                Assert.IsFalse(stumbler.TryStart(Vector2.left), "already stumbling");
                Assert.AreEqual(1, started);
                Assert.IsTrue(stumbler.Active);
                stumbler.Step(config.StumbleSeconds + 0.01f);
                Assert.IsFalse(stumbler.Active);
            }
            finally
            {
                Object.DestroyImmediate(go);
                Object.DestroyImmediate(config);
            }
        }
    }
}
