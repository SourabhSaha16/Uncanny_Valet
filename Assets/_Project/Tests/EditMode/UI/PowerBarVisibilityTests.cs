using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.UI.Hud;

namespace UncannyValet.Tests.UI
{
    /// <summary>The lot's ShowPowerBar flag hides the whole bar (issue #137) without touching the earned powers.</summary>
    public class PowerBarVisibilityTests
    {
        private sealed class Owned : IRunProgress
        {
            private readonly List<StoryPower> _powers = new List<StoryPower> { StoryPower.FreezeTime };
            public bool HasCurrentLot => false;
            public LotId CurrentLot => default;
            public IReadOnlyList<StoryPower> EarnedPowers => _powers;
            public int PowerLevel(StoryPower power) => 1;
            public int FlashedLevel(StoryPower power) => 1;
            public bool IsCleared(LotId lot) => false;
            public bool IsWorldCleared(World world) => false;
            public int BestBlessings(LotId lot) => 0;
        }

        private sealed class Goal : ILotGoal
        {
            public LotId Lot => default;
            public int BlessingThreshold => 1;
            public bool ShowPowerBar { get; set; }
        }

        [TestCase(true, true)]
        [TestCase(false, false)]
        public void TheBar_FollowsTheLotFlag_WhenAPowerIsOwned(bool flag, bool visible)
        {
            var tracker = new PowerBarTracker(new EventBus(), new PowerBarSettings(), new Owned(), null, new Goal { ShowPowerBar = flag });
            Assert.AreEqual(visible, tracker.IsVisible);
        }
    }
}
