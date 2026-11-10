using NUnit.Framework;
using UncannyValet.Traffic;

namespace UncannyValet.Tests.Traffic
{
    public class MergeGapRuleTests
    {
        private readonly MergeGapRule _rule = new MergeGapRule(2f, 2f, 0.5f);
        private const float Infinity = float.PositiveInfinity;

        [Test]
        public void EmptyLane_CanMerge() => Assert.IsTrue(_rule.CanMerge(Infinity, Infinity, 0f));

        [Test]
        public void SmallGapAhead_Waits() => Assert.IsFalse(_rule.CanMerge(1f, Infinity, 0f));

        [Test]
        public void SmallGapBehind_Waits() => Assert.IsFalse(_rule.CanMerge(Infinity, 1f, 0f));

        [Test]
        public void FastFollower_NeedsABiggerGap()
        {
            Assert.IsTrue(_rule.CanMerge(Infinity, 3f, 1f));
            Assert.IsFalse(_rule.CanMerge(Infinity, 3f, 4f));
        }
    }
}
