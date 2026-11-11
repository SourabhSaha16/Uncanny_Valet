using NUnit.Framework;
using UncannyValet.Lot;
using UnityEngine;

namespace UncannyValet.Tests.Lot
{
    /// <summary>A car is clear of the queue after resting far enough from every slot for the settle time (issue 78).</summary>
    public class QueueClearRuleTests
    {
        private const float Settle = 0.5f;
        private const float Clear = 6f;
        private const float Far = 8f;
        private const float Near = 1f;

        [Test]
        public void RestingFarEnoughForTheSettleTime_FiresOnce()
        {
            var rule = new QueueClearRule(Settle);
            Assert.IsFalse(rule.Tick(0.3f, Far, Clear, true));
            Assert.IsTrue(rule.Tick(0.3f, Far, Clear, true));
            Assert.IsFalse(rule.Tick(1f, Far, Clear, true), "only the first rest counts");
            Assert.IsTrue(rule.IsDone);
        }

        [Test]
        public void RestingInsideTheQueue_NeverFires()
        {
            var rule = new QueueClearRule(Settle);
            for (var i = 0; i < 20; i++) Assert.IsFalse(rule.Tick(1f, Near, Clear, true));
        }

        [Test]
        public void MovingOrDrivenRestartsTheCount()
        {
            var rule = new QueueClearRule(Settle);
            Assert.IsFalse(rule.Tick(0.4f, Far, Clear, true));
            Assert.IsFalse(rule.Tick(0.1f, Far, Clear, false), "a driver or speed breaks the rest");
            Assert.IsFalse(rule.Tick(0.4f, Far, Clear, true));
            Assert.IsTrue(rule.Tick(0.2f, Far, Clear, true));
        }

        [Test]
        public void ExactlyTheClearDistanceCounts()
        {
            var rule = new QueueClearRule(0f);
            Assert.IsTrue(rule.Tick(0.1f, Clear, Clear, true));
        }

        [Test]
        public void DistanceToNearest_UsesTheClosestSlotCentre()
        {
            var slots = new[] { new Vector2(0f, 0f), new Vector2(0f, -4.5f), new Vector2(0f, -9f) };
            Assert.AreEqual(0f, QueueClearRule.DistanceToNearest(slots, new Vector2(0f, -4.5f)), 0.001f);
            Assert.AreEqual(3f, QueueClearRule.DistanceToNearest(slots, new Vector2(3f, -9f)), 0.001f);
            Assert.AreEqual(6f, QueueClearRule.DistanceToNearest(slots, new Vector2(0f, -15f)), 0.001f);
            Assert.IsTrue(float.IsPositiveInfinity(QueueClearRule.DistanceToNearest(System.Array.Empty<Vector2>(), Vector2.zero)));
        }
    }
}
