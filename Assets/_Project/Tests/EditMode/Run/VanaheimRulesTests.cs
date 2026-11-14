using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Core.Modifiers;
using UncannyValet.Core.Watchers;
using UncannyValet.Lot;
using UncannyValet.Run;
using UnityEditor;
using UnityEngine;
using static UncannyValet.Tests.Run.VanaheimAssets;

namespace UncannyValet.Tests.Run
{
    /// <summary>The shipped Vanaheim lots (task 107): Muddy shares, pacing, crowd and traffic, the Storm wind grant, the watchers and the gag lines.</summary>
    public sealed class VanaheimRulesTests
    {
        [TestCase(TrainingPath, 0.3f)]
        [TestCase(CrazyPath, 0.6f)]
        public void TheMuddyShare_IsTheLots_NoCarNeedsCharge_AndTheSurgeIsOff(string path, float muddy)
        {
            var lot = LotAt(path);
            Assert.AreEqual(muddy, Share(lot, ModifierId.Muddy), 0.0001f);
            Assert.AreEqual(0f, Share(lot, ModifierId.NeedsCharge), "no EV cars in the story");
            var total = 0f;
            foreach (var entry in lot.ModifierChances) total += entry.Chance;
            Assert.LessOrEqual(total, 1f);
            var tuning = Seeded(lot);
            Assert.AreEqual(muddy, tuning.Get(ModifierDefaults.Muddy().ChanceKey), 0.0001f);
            Assert.AreEqual(0f, tuning.Get(TuningKey.PlanLateEvChance), "the end-of-shift charge surge is off too");
        }

        [Test]
        public void TheTrainingLot_Paces15To25Seconds_TwoWaiting_ThreeSlots_150Seconds()
        {
            var lot = LotAt(TrainingPath);
            var p = lot.Pacing;
            Assert.AreEqual(15f, p.ArrivalGapMinSeconds);
            Assert.AreEqual(25f, p.ArrivalGapMaxSeconds);
            Assert.AreEqual(2, p.MaxWaitingCars);
            Assert.AreEqual(3, lot.QueueSlots);
            Assert.AreEqual(150f, p.ShiftLengthSeconds);
            Assert.AreEqual(0, p.ArrivalBudgetOverride, "the arrival budget is derived");
            Assert.AreEqual(150, lot.BlessingThreshold);
            Assert.AreEqual(150f, Seeded(lot).Get(TuningKey.ShiftLengthSeconds));
        }

        [Test]
        public void TheCrazyLot_Paces10To15Seconds_FourWaiting_FourSlots_240Seconds_AndItsOwnBudget()
        {
            var lot = LotAt(CrazyPath);
            var p = lot.Pacing;
            Assert.AreEqual(10f, p.ArrivalGapMinSeconds);
            Assert.AreEqual(15f, p.ArrivalGapMaxSeconds);
            Assert.AreEqual(4, p.MaxWaitingCars);
            Assert.AreEqual(4, lot.QueueSlots);
            Assert.AreEqual(240f, p.ShiftLengthSeconds);
            Assert.AreEqual(0, p.ArrivalBudgetOverride, "the arrival budget is derived, not typed in");
            Assert.AreEqual(Mathf.CeilToInt(240f / 12.5f), p.ArrivalBudget);
            Assert.AreEqual(300, lot.BlessingThreshold);
            Assert.AreEqual(4f, Seeded(lot).Get(TuningKey.PlanMaxWaitingCars));
        }

        [TestCase(TrainingPath, 4)]
        [TestCase(CrazyPath, 8)]
        public void TheCrowd_AmbientTrafficIsOn_AndPedestrianCursesAreOff(string path, int crowd)
        {
            var lot = LotAt(path);
            var tuning = Seeded(lot);
            Assert.AreEqual(crowd, lot.AmbientPedestrians);
            Assert.AreEqual(crowd, tuning.Get(TuningKey.PedestrianCount));
            Assert.IsTrue(lot.AmbientTraffic);
            Assert.AreEqual(2f, tuning.Get(TuningKey.TrafficMinOnScreen), "today's traffic numbers are left alone");
            Assert.AreEqual(4f, tuning.Get(TuningKey.TrafficTargetOnScreen));
            Assert.IsFalse(lot.PedestrianCurses);
        }

        [Test]
        public void OnlyTheCrazyLotGrants_AndItIsStormWindLevelOne()
        {
            Assert.IsTrue(LotAt(TrainingPath).Grant.IsNone);
            Assert.AreEqual(StoryPower.StormWind, LotAt(CrazyPath).Grant.Power);
            Assert.AreEqual(1, LotAt(CrazyPath).Grant.Level);
            var progress = new RunProgress(AssetDatabase.LoadAssetAtPath<RunDefinition>(RunPath));
            progress.GrantPower(StoryPower.ThunderStride, 1);
            progress.GrantPower(LotAt(CrazyPath).Grant.Power, LotAt(CrazyPath).Grant.Level);
            Assert.AreEqual(1, progress.PowerLevel(StoryPower.StormWind));
            CollectionAssert.AreEqual(new[] { StoryPower.ThunderStride, StoryPower.StormWind }, progress.EarnedPowers);
        }

        [Test]
        public void TheWatchers_AreArniInTraining_AndFreyjaInTheCrazyLot()
        {
            Assert.AreEqual("FARMHAND ARNI", LotAt(TrainingPath).Watcher.SpeakerName);
            Assert.AreEqual(WatcherKind.Trainer, LotAt(TrainingPath).Watcher.Kind);
            Assert.AreEqual("FREYJA", LotAt(CrazyPath).Watcher.SpeakerName);
            Assert.AreEqual(WatcherKind.God, LotAt(CrazyPath).Watcher.Kind);
            Assert.AreEqual(World.Vanaheim, LotAt(CrazyPath).Watcher.World);
        }

        [TestCase(TrainingPath)]
        [TestCase(CrazyPath)]
        public void TheWatcherSpot_IsByTheBarnDoor_OutsideTheDrivableArea(string path)
        {
            var lot = LotAt(path);
            var g = Geometry(lot);
            var spot = lot.WatcherSpot;
            Assert.IsTrue(g.Bounds.Contains(spot), "inside the lot frame");
            Assert.Less(spot.x, g.Diner.xMax + 1f, "beside the barn");
            foreach (var r in Drivable(g)) Assert.IsFalse(Inflate(r, 0.5f).Contains(spot), "spot " + spot + " is inside " + r);
        }

        [Test]
        public void EachLot_HasEightGagLines_TheCrazyListHasTheVanirsThunderLine()
        {
            foreach (var path in new[] { TrainingPath, CrazyPath })
            {
                var gag = LotAt(path).GagLines;
                Assert.IsNotNull(gag);
                Assert.IsNull(gag.Check(), gag.Check());
                Assert.AreEqual(8, gag.Lines.Count);
                Assert.AreEqual(3f, gag.TriggerDistance);
            }
            CollectionAssert.Contains(new List<string>(LotAt(CrazyPath).GagLines.Lines), "SMELLS LIKE THUNDER IN HERE");
            Assert.AreEqual("VANIR", LotAt(CrazyPath).GagLines.Speaker);
        }

        private static IEnumerable<Rect> Drivable(LotGeometry g)
        {
            yield return g.Curb;
            yield return g.Corridor;
            foreach (var r in g.WashStations) yield return r;
            foreach (var r in g.Spots) yield return r;
            foreach (var r in g.QueueCars) yield return r;
            yield return Rect.MinMaxRect(g.ExitGap.x, g.Curb.yMax, g.ExitGap.y, g.Bounds.yMax);
            yield return Rect.MinMaxRect(g.Spots[0].xMin, g.Bounds.yMin, g.Bounds.xMax, g.Bounds.yMax);
        }

        private static Rect Inflate(Rect r, float by) => Rect.MinMaxRect(r.xMin - by, r.yMin - by, r.xMax + by, r.yMax + by);
    }
}
