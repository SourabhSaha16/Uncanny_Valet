using NUnit.Framework;
using UncannyValet.Core.Ids;
using UncannyValet.Powers;

namespace UncannyValet.Tests.Powers
{
    public class PowerKeyMapTests
    {
        [Test]
        public void KeysFillInTheOrderThePlayerEarnedThePowers()
        {
            var earned = new[] { StoryPower.StormWind, StoryPower.FreezeTime, StoryPower.ThunderStride };
            Assert.IsTrue(PowerKeyMap.TryGetPower(earned, 0, out var first));
            Assert.AreEqual(StoryPower.StormWind, first, "the first power earned is on key 1");
            Assert.IsTrue(PowerKeyMap.TryGetPower(earned, 1, out var second));
            Assert.AreEqual(StoryPower.FreezeTime, second);
            Assert.IsTrue(PowerKeyMap.TryGetPower(earned, 2, out var third));
            Assert.AreEqual(StoryPower.ThunderStride, third);
        }

        [Test]
        public void EmptyAndOutOfRangeKeysDoNothing()
        {
            var earned = new[] { StoryPower.ThunderStride, StoryPower.FreezeTime };
            Assert.IsFalse(PowerKeyMap.TryGetPower(earned, 2, out _), "key 3 has no power yet");
            Assert.IsFalse(PowerKeyMap.TryGetPower(earned, 3, out _));
            Assert.IsFalse(PowerKeyMap.TryGetPower(earned, 4, out _), "there are only four keys");
            Assert.IsFalse(PowerKeyMap.TryGetPower(earned, -1, out _));
            Assert.IsFalse(PowerKeyMap.TryGetPower(null, 0, out _));
        }

        [Test]
        public void TheFourthPowerIsOnKeyFour()
        {
            var earned = new[] { StoryPower.ThunderStride, StoryPower.StormWind, StoryPower.FreezeTime, StoryPower.MjolnirRecall };
            Assert.IsTrue(PowerKeyMap.TryGetPower(earned, 3, out var power));
            Assert.AreEqual(StoryPower.MjolnirRecall, power);
        }
    }
}
