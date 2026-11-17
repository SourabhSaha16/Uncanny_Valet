using NUnit.Framework;
using UncannyValet.Pedestrians;
using UnityEngine;

namespace UncannyValet.Tests.Pedestrians
{
    /// <summary>The gag line trigger rule: distance, the global and per-walker lockouts, one pop at a time, and the deck of lines.</summary>
    public sealed class GagLineRuleTests
    {
        private const float Distance = 3f;
        private const float Global = 6f;
        private const float PerWalker = 20f;

        private GagLineRule _rule;
        private readonly Vector2[] _positions = new Vector2[3];
        private readonly bool[] _active = new bool[3];

        [SetUp]
        public void SetUp()
        {
            _rule = new GagLineRule(Distance, Global, PerWalker, 3);
            _positions[0] = new Vector2(2f, 0f);
            _positions[1] = new Vector2(0f, 2.5f);
            _positions[2] = new Vector2(10f, 0f);
            for (var i = 0; i < _active.Length; i++) _active[i] = true;
        }

        private int Pick(float now) => _rule.Pick(now, Vector2.zero, _positions, _active, 3);

        [Test]
        public void NobodyPops_WhenEveryoneIsFarAway()
        {
            _positions[0] = _positions[1] = new Vector2(5f, 0f);
            Assert.AreEqual(GagLineRule.None, Pick(0f));
        }

        [Test]
        public void TheNearestWalkerWithinTheDistance_Pops()
        {
            Assert.AreEqual(0, Pick(0f), "2 units beats 2.5 units");
        }

        [Test]
        public void ExactlyAtTheDistance_Counts_JustBeyondDoesNot()
        {
            _positions[0] = new Vector2(Distance, 0f);
            _positions[1] = new Vector2(Distance + 0.01f, 0f);
            Assert.AreEqual(0, Pick(0f));
        }

        [Test]
        public void AfterAPop_NobodyPops_UntilTheGlobalLockoutEnds()
        {
            Assert.AreEqual(0, Pick(0f));
            Assert.AreEqual(GagLineRule.None, Pick(Global - 0.1f), "walker 1 is close but the global lockout runs");
            Assert.AreEqual(1, Pick(Global), "the lockout is over: the other walker pops");
        }

        [Test]
        public void TheSameWalker_WaitsItsOwnLockout()
        {
            Assert.AreEqual(0, Pick(0f));
            Assert.AreEqual(1, Pick(Global));
            Assert.AreEqual(GagLineRule.None, Pick(2f * Global + 0.1f), "walker 0 popped at 0 s and waits until 20 s");
            Assert.AreEqual(0, Pick(PerWalker));
        }

        [Test]
        public void AWalkerOutOfSight_NeverPops()
        {
            _active[0] = false;
            Assert.AreEqual(1, Pick(0f));
        }

        [Test]
        public void Reset_ForgetsTheLockouts()
        {
            Assert.AreEqual(0, Pick(0f));
            _rule.Reset();
            Assert.AreEqual(0, Pick(0.5f));
        }

        [Test]
        public void TheDeck_PlaysEveryLineOnce_ThenAgain_NeverTwiceInARow()
        {
            var lines = new[] { "A", "B", "C", "D" };
            var deck = new GagLineDeck(lines, 7);
            var previous = "";
            for (var round = 0; round < 6; round++)
            {
                var seen = new System.Collections.Generic.HashSet<string>();
                for (var i = 0; i < lines.Length; i++)
                {
                    var line = deck.Next();
                    Assert.IsTrue(seen.Add(line), "a line repeated inside a round");
                    Assert.AreNotEqual(previous, line);
                    previous = line;
                }
            }
        }

        [Test]
        public void AnEmptyDeck_DealsNothing() => Assert.IsNull(new GagLineDeck(new string[0], 1).Next());
    }
}
