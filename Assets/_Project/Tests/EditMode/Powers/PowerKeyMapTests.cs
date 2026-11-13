using NUnit.Framework;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Powers;

namespace UncannyValet.Tests.Powers
{
    public class PowerKeyMapTests
    {
        [Test]
        public void KeyOne_IsTheFirstPowerEarned_WhenItHasAnEffect()
        {
            var earned = new[] { StoryPower.FreezeTime };
            Assert.IsTrue(PowerKeyMap.TryGetKind(earned, 0, out var kind));
            Assert.AreEqual(PowerKind.FreezeTime, kind);
        }

        [Test]
        public void EmptySlotsOutOfRangeAndPowersWithoutAnEffectDoNothing()
        {
            var earned = new[] { StoryPower.ThunderStride, StoryPower.FreezeTime };
            Assert.IsFalse(PowerKeyMap.TryGetKind(earned, 0, out _), "no effect yet");
            Assert.IsTrue(PowerKeyMap.TryGetKind(earned, 1, out _), "key 2 holds the second power earned");
            Assert.IsFalse(PowerKeyMap.TryGetKind(earned, 2, out _));
            Assert.IsFalse(PowerKeyMap.TryGetKind(earned, -1, out _));
            Assert.IsFalse(PowerKeyMap.TryGetKind(null, 0, out _));
        }
    }
}
