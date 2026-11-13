using NUnit.Framework;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Run;

namespace UncannyValet.Tests.Run
{
    public sealed class RunProgressTests
    {
        private static readonly LotId MidgardTraining = new LotId(World.Midgard, LotKind.Training);
        private static readonly LotId MidgardCrazy = new LotId(World.Midgard, LotKind.Crazy);

        private RunFixtures _fx;
        private RunProgress _progress;

        [SetUp] public void SetUp() { _fx = new RunFixtures(); _progress = new RunProgress(_fx.MidgardRun()); }
        [TearDown] public void TearDown() { _fx.Dispose(); }

        [Test]
        public void New_HasNoCurrentLotAndNothingEarned()
        {
            Assert.IsFalse(_progress.HasCurrentLot);
            Assert.AreEqual(0, _progress.EarnedPowers.Count);
            Assert.AreEqual(0, _progress.PowerLevel(StoryPower.ThunderStride));
            Assert.IsFalse(_progress.IsCleared(MidgardTraining));
            Assert.AreEqual(0, _progress.BestBlessings(MidgardTraining));
        }

        [Test]
        public void SetCurrentLot_RecordsIt()
        {
            _progress.SetCurrentLot(MidgardCrazy);
            Assert.IsTrue(_progress.HasCurrentLot);
            Assert.AreEqual(MidgardCrazy, _progress.CurrentLot);
        }

        [Test]
        public void GrantPower_EarnsThenUpgrades_AndNeverLowers()
        {
            _progress.GrantPower(StoryPower.ThunderStride, 1);
            Assert.AreEqual(1, _progress.PowerLevel(StoryPower.ThunderStride));
            _progress.GrantPower(StoryPower.ThunderStride, 2);
            Assert.AreEqual(2, _progress.PowerLevel(StoryPower.ThunderStride));
            _progress.GrantPower(StoryPower.ThunderStride, 1);
            Assert.AreEqual(2, _progress.PowerLevel(StoryPower.ThunderStride));
        }

        [Test]
        public void GrantPower_KeepsEarnOrderAndListsAPowerOnce()
        {
            _progress.GrantPower(StoryPower.StormWind, 1);
            _progress.GrantPower(StoryPower.ThunderStride, 1);
            _progress.GrantPower(StoryPower.StormWind, 2);
            CollectionAssert.AreEqual(new[] { StoryPower.StormWind, StoryPower.ThunderStride }, _progress.EarnedPowers);
        }

        [Test]
        public void GrantPower_NoneIsIgnored()
        {
            _progress.GrantPower(StoryPower.None, 1);
            Assert.AreEqual(0, _progress.EarnedPowers.Count);
        }

        [Test]
        public void MarkCleared_TrackedPerLot_TrainingAndCrazySeparate()
        {
            _progress.MarkCleared(MidgardTraining);
            Assert.IsTrue(_progress.IsCleared(MidgardTraining));
            Assert.IsFalse(_progress.IsCleared(MidgardCrazy));
            Assert.IsFalse(_progress.IsWorldCleared(World.Midgard));
        }

        [Test]
        public void IsWorldCleared_WhenCrazyLotCleared_AndWhenAsgardFinaleCleared()
        {
            _progress.MarkCleared(MidgardCrazy);
            _progress.MarkCleared(new LotId(World.Asgard, LotKind.Finale));
            Assert.IsTrue(_progress.IsWorldCleared(World.Midgard));
            Assert.IsTrue(_progress.IsWorldCleared(World.Asgard));
            Assert.IsFalse(_progress.IsWorldCleared(World.Vanaheim));
        }

        [Test]
        public void RecordBest_KeepsTheMaximum()
        {
            _progress.RecordBest(MidgardCrazy, 4);
            _progress.RecordBest(MidgardCrazy, 2);
            Assert.AreEqual(4, _progress.BestBlessings(MidgardCrazy));
            _progress.RecordBest(MidgardCrazy, 6);
            Assert.AreEqual(6, _progress.BestBlessings(MidgardCrazy));
            Assert.AreEqual(0, _progress.BestBlessings(MidgardTraining));
        }

        [Test]
        public void TryGetNextLot_WalksTheRunDefinition()
        {
            Assert.IsTrue(_progress.TryGetNextLot(out var first));
            Assert.AreEqual(MidgardTraining, first.Id);
            _progress.SetCurrentLot(MidgardTraining);
            Assert.IsTrue(_progress.TryGetNextLot(out var second));
            Assert.AreEqual(MidgardCrazy, second.Id);
            _progress.SetCurrentLot(MidgardCrazy);
            Assert.IsFalse(_progress.TryGetNextLot(out _));
        }

        [Test]
        public void StartNewRun_ForgetsEverything()
        {
            _progress.SetCurrentLot(MidgardCrazy);
            _progress.GrantPower(StoryPower.ThunderStride, 1);
            _progress.MarkCleared(MidgardCrazy);
            _progress.RecordBest(MidgardCrazy, 5);
            _progress.StartNewRun();
            Assert.IsFalse(_progress.HasCurrentLot);
            Assert.AreEqual(0, _progress.EarnedPowers.Count);
            Assert.AreEqual(0, _progress.PowerLevel(StoryPower.ThunderStride));
            Assert.IsFalse(_progress.IsCleared(MidgardCrazy));
            Assert.AreEqual(0, _progress.BestBlessings(MidgardCrazy));
        }

        [Test]
        public void Link_AppliesLotEventsToTheProgress()
        {
            var bus = new EventBus();
            using (new RunProgressLink(bus, _progress))
            {
                bus.Publish(new LotStarted(MidgardCrazy));
                bus.Publish(new PowerGranted(StoryPower.FreezeTime, 1));
                bus.Publish(new LotCleared(MidgardCrazy));
            }
            Assert.AreEqual(MidgardCrazy, _progress.CurrentLot);
            Assert.AreEqual(1, _progress.PowerLevel(StoryPower.FreezeTime));
            Assert.IsTrue(_progress.IsCleared(MidgardCrazy));
        }

        [Test]
        public void Link_AfterDispose_IgnoresEvents()
        {
            var bus = new EventBus();
            new RunProgressLink(bus, _progress).Dispose();
            bus.Publish(new LotStarted(MidgardCrazy));
            Assert.IsFalse(_progress.HasCurrentLot);
        }
    }
}
