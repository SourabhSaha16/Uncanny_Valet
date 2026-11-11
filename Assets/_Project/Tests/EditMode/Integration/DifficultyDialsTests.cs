using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Director;
using UnityEngine;

namespace UncannyValet.Tests.Integration
{
    public class DifficultyDialsTests
    {
        private const float Mid = 5f;
        private const float DialMin = 0f;
        private const float DialMax = 10f;

        [Test]
        public void DefaultDialPositions_ReproduceEveryAuthoredValueExactly()
        {
            using (var authored = new DialRig(linked: false))
            using (var dialed = new DialRig())
            {
                foreach (var info in TuningCatalog.All)
                    Assert.AreEqual(authored[info.Key], dialed[info.Key], info.Key + " moved at the default dial position");
                Assert.AreEqual(14f, dialed[TuningKey.DirArrivalBudget]);
                Assert.AreEqual(1.4f, dialed[TuningKey.DirReliefWorkload]);
                Assert.AreEqual(0.6f, dialed[TuningKey.PlanLateEvStart]);
            }
        }

        [Test]
        public void EveryDialStartsAtTheDefaultPosition()
        {
            using (var rig = new DialRig())
                foreach (var key in DifficultyDials.DialKeys) Assert.AreEqual(Mid, rig[key], key.ToString());
        }

        [Test]
        public void MovingADialAndBack_RestoresEveryKey()
        {
            using (var rig = new DialRig())
            {
                var before = Snapshot(rig);
                foreach (var key in DifficultyDials.DialKeys)
                {
                    rig.Dial(key, DialMax);
                    CollectionAssert.AreNotEqual(before, Snapshot(rig), key + " drives nothing");
                    rig.Dial(key, Mid);
                    CollectionAssert.AreEqual(before, Snapshot(rig), key + " did not return to the default");
                }
            }
        }

        [Test]
        public void Interpolation_IsPiecewiseLinearThroughTheAnchors()
        {
            using (var rig = new DialRig())
            {
                rig.Dial(TuningKey.DialIntensity, 2.5f);
                Assert.AreEqual(0.75f, rig[TuningKey.DirTargetScale], 0.0001f, "halfway between 0.5 and 1");
                rig.Dial(TuningKey.DialIntensity, 7.5f);
                Assert.AreEqual(1.2f, rig[TuningKey.DirTargetScale], 0.0001f, "halfway between 1 and 1.4");
                rig.Dial(TuningKey.DialIntensity, DialMin);
                Assert.AreEqual(0.5f, rig[TuningKey.DirTargetScale], 0.0001f);
                rig.Dial(TuningKey.DialIntensity, DialMax);
                Assert.AreEqual(1.4f, rig[TuningKey.DirTargetScale], 0.0001f);
            }
        }

        [TestCase(TuningKey.DialIntensity, TuningKey.DirTargetScale, 1)]
        [TestCase(TuningKey.DialIntensity, TuningKey.DirArrivalBudget, 1)]
        [TestCase(TuningKey.DialIntensity, TuningKey.DirMinInterval, -1)]
        [TestCase(TuningKey.DialIntensity, TuningKey.DirUnderFactor, -1)]
        [TestCase(TuningKey.DialIntensity, TuningKey.DirOverFactor, -1)]
        [TestCase(TuningKey.DialRamp, TuningKey.DirRampShape, 1)]
        [TestCase(TuningKey.DialLateRush, TuningKey.DirLateTargetBoost, 1)]
        [TestCase(TuningKey.DialLateRush, TuningKey.PlanLateEvStart, -1)]
        [TestCase(TuningKey.DialDirtyCars, TuningKey.PlanDirtyChance, 1)]
        [TestCase(TuningKey.DialDirtyCars, TuningKey.DirWeightDirty, 1)]
        [TestCase(TuningKey.DialEvCars, TuningKey.PlanEvChance, 1)]
        [TestCase(TuningKey.DialEvCars, TuningKey.DirWeightEv, 1)]
        [TestCase(TuningKey.DialDilemmas, TuningKey.DirDilemmaWashBlocked, 1)]
        [TestCase(TuningKey.DialDilemmas, TuningKey.DirDilemmaCooldown, -1)]
        [TestCase(TuningKey.DialMercy, TuningKey.DirReliefWorkload, -1)]
        [TestCase(TuningKey.DialMercy, TuningKey.DirReliefHold, 1)]
        [TestCase(TuningKey.DialMercy, TuningKey.DirReliefCooldown, -1)]
        [TestCase(TuningKey.DialMercy, TuningKey.DirAssistDilemmaScale, -1)]
        [TestCase(TuningKey.DialSkillReward, TuningKey.DirFormInfluence, 1)]
        [TestCase(TuningKey.DialSkillReward, TuningKey.DirFormWarmup, -1)]
        public void EachDial_MovesItsKeysMonotonicallyInTheDocumentedDirection(TuningKey dial, TuningKey key, int direction)
        {
            using (var rig = new DialRig())
            {
                rig.Dial(dial, DialMin);
                var previous = rig[key];
                for (var position = DialMin; position <= DialMax; position += 0.5f)
                {
                    rig.Dial(dial, position);
                    var value = rig[key];
                    Assert.GreaterOrEqual(value * direction, previous * direction - 0.0001f, key + " at dial " + position);
                    previous = value;
                }
                rig.Dial(dial, DialMin);
                var low = rig[key];
                rig.Dial(dial, DialMax);
                Assert.Greater((rig[key] - low) * direction, 0f, key + " does not move with " + dial);
            }
        }

        [Test]
        public void EveryAnchorRowIsMonotoneAndNeutralAtTheDefault()
        {
            foreach (var link in DialAnchors.All())
            {
                var neutral = link.Effect == DialEffect.Scale ? 1f : 0f;
                Assert.AreEqual(neutral, link.AtDefault, link.Key + " changes at the default dial position");
                var rising = link.AtMin <= link.AtDefault && link.AtDefault <= link.AtMax;
                var falling = link.AtMin >= link.AtDefault && link.AtDefault >= link.AtMax;
                Assert.IsTrue(rising || falling, link.Dial + " -> " + link.Key + " is not monotone");
            }
        }

        [Test]
        public void ADialMove_WritesDerivedKeysOnceEach_WithoutFeedback()
        {
            using (var rig = new DialRig())
            {
                rig.Changes = 0;
                rig.Dial(TuningKey.DialMercy, DialMax);
                Assert.Greater(rig.Changes, 1);
                Assert.LessOrEqual(rig.Changes, 1 + DialAnchors.All().Count, "derived writes must not retrigger the mapper");
                var settled = rig.Changes;
                rig.Dial(TuningKey.DialMercy, DialMax);
                Assert.AreEqual(settled + 1, rig.Changes, "setting the same dial value changes only the dial itself");
            }
        }

        [Test]
        public void HandEditsToAnAdvancedRow_StickUntilTheNextDialMove()
        {
            using (var rig = new DialRig())
            {
                rig.Tuning.Set(TuningKey.DirBudgetSlack, 7f);
                Assert.AreEqual(7f, rig[TuningKey.DirBudgetSlack]);
                rig.Dial(TuningKey.DialEvCars, 6f);
                Assert.AreEqual(2f, rig[TuningKey.DirBudgetSlack], "any dial move re-derives every driven key");
                rig.Tuning.Set(TuningKey.DirBudgetSlack, 7f);
                rig.Dial(TuningKey.DialIntensity, DialMax);
                Assert.AreEqual(4f, rig[TuningKey.DirBudgetSlack], "and applies the moved dial");
            }
        }

        [Test]
        public void WholeNumberKeysAreRoundedAndStayInRange()
        {
            using (var rig = new DialRig())
            {
                rig.Dial(TuningKey.DialIntensity, 7.3f);
                Assert.AreEqual(Mathf.Round(rig[TuningKey.DirArrivalBudget]), rig[TuningKey.DirArrivalBudget]);
                rig.Dial(TuningKey.DialIntensity, DialMin);
                Assert.AreEqual(2f, rig[TuningKey.PlanMaxWaitingCars]);
                rig.Dial(TuningKey.DialIntensity, DialMax);
                Assert.AreEqual(3f, rig[TuningKey.PlanMaxWaitingCars]);
                rig.Dial(TuningKey.DialMercy, DialMax);
                Assert.AreEqual(1f, rig[TuningKey.DirReliefAngryCount]);
                Assert.AreEqual(1f, rig[TuningKey.DirAssist], "assist switches on at the top of Mercy");
            }
        }

        [Test]
        public void DirtyEvWeight_FollowsBothCarMixDials()
        {
            using (var rig = new DialRig())
            {
                rig.Dial(TuningKey.DialDirtyCars, DialMax);
                rig.Dial(TuningKey.DialEvCars, DialMax);
                Assert.AreEqual(10f * 2.5f * 2.5f, rig[TuningKey.DirWeightDirtyEv], 0.001f, "both anchors multiply");
                rig.Dial(TuningKey.DialEvCars, DialMin);
                Assert.AreEqual(0f, rig[TuningKey.DirWeightEv]);
                Assert.AreEqual(0f, rig[TuningKey.DirWeightDirtyEv]);
            }
        }

        [Test]
        public void TheCatalog_PutsTheDialsFirstAndHidesEverythingElseBehindAdvanced()
        {
            var visible = new List<TuningKey>();
            var seen = new HashSet<TuningKey>();
            var closed = new HashSet<string>();
            var group = string.Empty;
            for (var i = 0; i < TuningCatalog.All.Count; i++)
            {
                var info = TuningCatalog.All[i];
                Assert.IsTrue(seen.Add(info.Key), info.Key + " listed twice");
                if (info.Group != group) { Assert.IsTrue(closed.Add(info.Group), info.Group + " is split"); group = info.Group; }
                if (!info.Advanced) visible.Add(info.Key);
                if (i < 9) Assert.AreEqual(TuningInfo.DifficultyGroup, info.Group, "dials come first");
            }
            Assert.AreEqual(11, visible.Count, "nine dials plus game speed and shift length");
            Assert.AreEqual(TuningKey.DialIntensity, TuningCatalog.All[0].Key);
            foreach (var key in DifficultyDials.DialKeys) Assert.IsNotEmpty(Find(key).Tooltip, key + " needs a tooltip");
            Assert.AreEqual(TuningKey.DirSeed, TuningCatalog.All[8].Key);
        }

        private static TuningInfo Find(TuningKey key)
        {
            foreach (var info in TuningCatalog.All) if (info.Key == key) return info;
            throw new KeyNotFoundException(key.ToString());
        }

        private static List<float> Snapshot(DialRig rig)
        {
            var values = new List<float>();
            foreach (var info in TuningCatalog.All) values.Add(rig[info.Key]);
            return values;
        }
    }
}
