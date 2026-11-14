using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Core;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Run;
using UnityEditor;

namespace UncannyValet.Tests.Run
{
    /// <summary>The run rules over a fake 17-lot order (the doc's full order, ending in the finale).</summary>
    public sealed class RunFlowFullOrderTests : RunFlowTestBase
    {
        private void Start17()
        {
            var lots = new List<LotDefinition>();
            var worlds = new[] { World.Midgard, World.Vanaheim, World.Niflheim, World.Svartalfheim, World.Alfheim,
                World.Muspelheim, World.Jotunheim, World.Helheim };
            var powers = new[] { StoryPower.ThunderStride, StoryPower.StormWind, StoryPower.FreezeTime, StoryPower.MjolnirRecall,
                StoryPower.ThunderStride, StoryPower.StormWind, StoryPower.FreezeTime, StoryPower.MjolnirRecall };
            var levels = new[] { 1, 1, 1, 1, 2, 2, 2, 2 };
            for (var i = 0; i < worlds.Length; i++)
            {
                lots.Add(_fx.Lot(worlds[i], LotKind.Training));
                var crazy = _fx.Lot(worlds[i], LotKind.Crazy);
                RunFixtures.Set(crazy, "_grant._power", (float)powers[i]);
                RunFixtures.Set(crazy, "_grant._level", levels[i]);
                lots.Add(crazy);
            }
            lots.Add(_fx.Lot(World.Asgard, LotKind.Finale));
            Start(_fx.Run(lots.ToArray()));
        }

        [Test]
        public void FullOrder_ClearsEveryLotInTurn_AndTheFinaleCompletesTheRun()
        {
            Start17();
            var worlds = new[] { World.Midgard, World.Vanaheim, World.Niflheim, World.Svartalfheim, World.Alfheim,
                World.Muspelheim, World.Jotunheim, World.Helheim };
            foreach (var w in worlds)
            {
                var training = new LotId(w, LotKind.Training);
                var crazy = new LotId(w, LotKind.Crazy);
                AssertPlay(Clear(training), crazy, RunOutcome.Advanced);
                var next = Clear(crazy);
                if (w == World.Helheim) AssertPlay(next, Asgard, RunOutcome.Advanced);
                else AssertPlay(next, new LotId(worlds[System.Array.IndexOf(worlds, w) + 1], LotKind.Training), RunOutcome.Advanced);
            }
            Assert.AreEqual(0, _completed.Count);
            var end = Clear(Asgard);
            Assert.AreEqual(RunOutcome.RunComplete, end.Reason);
            Assert.IsFalse(end.HasNext);
            Assert.AreEqual(1, _completed.Count);
            Assert.AreEqual(RunEnd.Finale, _completed[0].Reason);
        }
        [Test]
        public void FullOrder_PowersGrantOnceAndLevelsNeverExceedTheDoc()
        {
            Start17();
            foreach (var w in new[] { World.Midgard, World.Vanaheim, World.Niflheim, World.Svartalfheim, World.Alfheim,
                         World.Muspelheim, World.Jotunheim, World.Helheim })
            {
                Clear(new LotId(w, LotKind.Training));
                Clear(new LotId(w, LotKind.Crazy));
            }
            Assert.AreEqual(8, _granted.Count);
            Assert.AreEqual(4, _progress.EarnedPowers.Count);
            foreach (var p in _progress.EarnedPowers) Assert.AreEqual(2, _progress.PowerLevel(p));
            Fail(Asgard);
            Clear(MC);
            Assert.AreEqual(8, _granted.Count);
        }
        [Test]
        public void FailedFinale_GoesBackToMidgardCrazy_KeepingEverything()
        {
            Start17();
            Clear(MT);
            Clear(MC);
            Clear(NT);
            AssertPlay(Fail(Asgard), MC, RunOutcome.SentBack);
            Assert.IsTrue(_progress.IsCleared(NT));
            Assert.AreEqual(1, _progress.PowerLevel(StoryPower.ThunderStride));
            Assert.AreEqual(0, _completed.Count);
        }
        [Test]
        public void FullOrder_ClimbSkipsEveryClearedTraining_AndPlaysUnclearedOnes()
        {
            Start17();
            Clear(MT);
            Clear(MC);
            Clear(VT);
            Fail(new LotId(World.Vanaheim, LotKind.Crazy));
            AssertPlay(Clear(MC), VC, RunOutcome.Advanced);
            AssertPlay(Clear(VC), NT, RunOutcome.Advanced);
            Clear(NT);
            Fail(NC);
            AssertPlay(Clear(MC), VC, RunOutcome.Advanced);
            AssertPlay(Clear(VC), NC, RunOutcome.Advanced);
        }
    }
}
