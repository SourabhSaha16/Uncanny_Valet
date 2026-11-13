using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UncannyValet.Bootstrap;
using UncannyValet.Core.Config;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Economy;
using UncannyValet.Patrons;
using UncannyValet.Powers;
using UncannyValet.Run;
using UncannyValet.Tests.Run;
using UnityEngine;

namespace UncannyValet.Tests.Powers
{
    /// <summary>Levels, F2 rows and seeding, the read-only power view, the earned-power flash memory and the per-lot fill amounts.</summary>
    public class PowerFrameworkTests
    {
        private PowerBarConfig _config;
        private TuningStore _tuning;
        private PowerSettings _settings;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<PowerBarConfig>();
            _tuning = new TuningStore(new EventBus());
            _config.SeedInto(_tuning);
            _settings = new PowerSettings(_config);
        }

        [TearDown]
        public void TearDown() => UnityEngine.Object.DestroyImmediate(_config);

        [Test]
        public void EveryPowerLevelFieldHasAUniqueKey_AndItMapsBack()
        {
            var seen = new HashSet<TuningKey>();
            foreach (var power in PowerLevelRows.All)
                for (var level = 1; level <= PowerTuningKeys.Levels; level++)
                    for (var f = 0; f < PowerTuningKeys.FieldCount; f++)
                    {
                        Assert.IsTrue(PowerTuningKeys.TryGetKey(power, level, (PowerField)f, out var key));
                        Assert.IsTrue(seen.Add(key), key.ToString());
                        Assert.IsTrue(PowerTuningKeys.TryGetSlot(key, out var p, out var l, out var field));
                        Assert.AreEqual((power, level, (PowerField)f), (p, l, field));
                        StringAssert.Contains(power.ToString(), key.ToString());
                        StringAssert.Contains("L" + level, key.ToString());
                    }
            Assert.AreEqual(PowerTuningKeys.KeyCount, seen.Count);
            Assert.IsFalse(PowerTuningKeys.TryGetKey(StoryPower.None, 1, PowerField.Cooldown, out _));
            Assert.IsFalse(PowerTuningKeys.TryGetKey(StoryPower.FreezeTime, 3, PowerField.Cooldown, out _));
            Assert.IsFalse(PowerTuningKeys.TryGetSlot(TuningKey.PowerFillPerfectPark, out _, out _, out _));
        }

        [Test]
        public void EveryUsedLevelValueHasAnAdvancedRowSeededFromTheDefinitions()
        {
            var rows = TuningCatalog.All.ToList();
            foreach (var power in PowerLevelRows.All)
                for (var level = 1; level <= PowerTuningKeys.Levels; level++)
                    for (var f = 0; f < PowerTuningKeys.FieldCount; f++)
                    {
                        var field = (PowerField)f;
                        PowerTuningKeys.TryGetKey(power, level, field, out var key);
                        Assert.IsTrue(_tuning.TryGet(key, out var seeded), "seeded: " + key);
                        Assert.AreEqual(_config.ValuesFor(power, level).Get(field), seeded, key.ToString());
                        var used = PowerLevelRows.FieldName(power, field) != null;
                        var row = rows.Where(r => r.Key == key).ToList();
                        Assert.AreEqual(used ? 1 : 0, row.Count, "row of " + key);
                        if (!used) continue;
                        Assert.IsTrue(row[0].Advanced);
                        Assert.AreEqual(PowerLevelRows.GroupOf(power), row[0].Group);
                        StringAssert.StartsWith(PowerLevelRows.GroupOf(power) + " level " + level + ": ", row[0].Label);
                    }
        }

        [Test]
        public void TheRowsOfOnePowerStayTogetherInTheCatalog()
        {
            var groups = TuningCatalog.All.Select(r => r.Group).ToList();
            foreach (var power in PowerLevelRows.All)
            {
                var name = PowerLevelRows.GroupOf(power);
                var first = groups.IndexOf(name);
                var last = groups.LastIndexOf(name);
                Assert.GreaterOrEqual(first, 0, name);
                Assert.AreEqual(last - first + 1, groups.Count(g => g == name), name + " rows are contiguous");
            }
        }

        [Test]
        public void ExamplesFromTheSpecAreReadable()
        {
            var labels = TuningCatalog.All.Select(r => r.Label).ToList();
            CollectionAssert.Contains(labels, "Thunder stride level 1: dash distance");
            CollectionAssert.Contains(labels, "Storm wind level 2: gust width");
            CollectionAssert.Contains(labels, "Freeze time level 1: cooldown (s)");
        }

        [Test]
        public void FreezeTime_KeepsItsBaseDurationOfSixSeconds_AndFollowsF2()
        {
            Assert.IsTrue(_settings.TryGetValues(StoryPower.FreezeTime, 1, out var values));
            Assert.AreEqual(6f, values.Duration, 0.0001f);
            Assert.Greater(values.Cooldown, 0f);
            _settings.Apply(TuningKey.PowerFreezeTimeL2Duration, 11f);
            _settings.TryGetValues(StoryPower.FreezeTime, 2, out var upgraded);
            Assert.AreEqual(11f, upgraded.Duration);
            _settings.TryGetValues(StoryPower.FreezeTime, 1, out values);
            Assert.AreEqual(6f, values.Duration, "level 1 untouched");
            Assert.IsFalse(_settings.TryGetValues(StoryPower.FreezeTime, 3, out _));
            Assert.IsFalse(_settings.TryGetValues(StoryPower.None, 1, out _));
        }

        [Test]
        public void Load_TakesTheStoreValuesIncludingUnusedFields()
        {
            _tuning.Set(TuningKey.PowerStormWindL1PushStrength, 12f);
            _tuning.Set(TuningKey.PowerThunderStrideL2PushStrength, 3f);
            _settings.Load(_tuning);
            _settings.TryGetValues(StoryPower.StormWind, 1, out var wind);
            _settings.TryGetValues(StoryPower.ThunderStride, 2, out var stride);
            Assert.AreEqual(12f, wind.PushStrength);
            Assert.AreEqual(3f, stride.PushStrength);
        }

        [Test]
        public void ThePowerView_PicksTheLevelFromTheRunProgress_AndShowsThePhase()
        {
            var fx = new RunFixtures();
            try
            {
                var progress = new RunProgress(fx.MidgardRun());
                var bus = new EventBus();
                var freeze = new FreezeTimeEffect(_settings);
                var stack = new PowerStack(bus, new IPower[] { freeze }, new PowerCooldowns());
                var view = new PowerView(() => progress, _settings, stack);
                Assert.IsFalse(view.TryGetState(StoryPower.FreezeTime, out _), "not owned");
                progress.GrantPower(StoryPower.FreezeTime, 1);
                Assert.IsTrue(view.TryGetState(StoryPower.FreezeTime, out var state));
                Assert.AreEqual(1, state.Level);
                Assert.AreEqual(PowerPhase.Ready, state.Phase);
                progress.GrantPower(StoryPower.FreezeTime, 2);
                view.TryGetState(StoryPower.FreezeTime, out state);
                Assert.AreEqual(2, state.Level);
                Assert.AreEqual(_config.ValuesFor(StoryPower.FreezeTime, 2).Duration, state.Values.Duration);
                stack.TryFire(StoryPower.FreezeTime, 2, state.Values, new FakeBucket { Open = 1 });
                view.TryGetState(StoryPower.FreezeTime, out state);
                Assert.AreEqual(PowerPhase.Active, state.Phase);
                Assert.AreEqual(1f, state.Remaining01, 0.0001f);
            }
            finally
            {
                fx.Dispose();
            }
        }

        [Test]
        public void RunProgress_RemembersTheFlashPerLevel_AndForgetsItOnRevokeAndNewRun()
        {
            var fx = new RunFixtures();
            try
            {
                var progress = new RunProgress(fx.MidgardRun());
                progress.GrantPower(StoryPower.StormWind, 1);
                Assert.AreEqual(0, progress.FlashedLevel(StoryPower.StormWind));
                progress.MarkFlashed(StoryPower.StormWind, 1);
                progress.MarkFlashed(StoryPower.StormWind, 0);
                Assert.AreEqual(1, progress.FlashedLevel(StoryPower.StormWind));
                progress.RevokePower(StoryPower.StormWind);
                Assert.AreEqual(0, progress.FlashedLevel(StoryPower.StormWind));
                progress.GrantPower(StoryPower.StormWind, 2);
                progress.MarkFlashed(StoryPower.StormWind, 2);
                progress.StartNewRun();
                Assert.AreEqual(0, progress.FlashedLevel(StoryPower.StormWind));
            }
            finally
            {
                fx.Dispose();
            }
        }

        [Test]
        public void TheFlashEventReachesTheRunProgressThroughItsLink()
        {
            var fx = new RunFixtures();
            try
            {
                var bus = new EventBus();
                var progress = new RunProgress(fx.MidgardRun());
                using (new RunProgressLink(bus, progress))
                {
                    bus.Publish(new PowerGranted(StoryPower.FreezeTime, 1));
                    bus.Publish(new PowerIconFlashed(StoryPower.FreezeTime, 1));
                    Assert.AreEqual(1, progress.FlashedLevel(StoryPower.FreezeTime));
                }
            }
            finally
            {
                fx.Dispose();
            }
        }

        [Test]
        public void ALotSeedsItsPowerBarFillAmountsOverTheConfig()
        {
            var fx = new RunFixtures();
            var shift = ScriptableObject.CreateInstance<ShiftConfig>();
            var patrons = ScriptableObject.CreateInstance<PatronConfig>();
            var blessings = ScriptableObject.CreateInstance<BlessingConfig>();
            try
            {
                var lot = fx.Lot(World.Midgard, LotKind.Crazy);
                var so = new UnityEditor.SerializedObject(lot);
                so.FindProperty("_powerFill._perfectPark").floatValue = 25f;
                so.FindProperty("_powerFill._perfectDropOff").floatValue = 12f;
                so.FindProperty("_powerFill._driftPerSecond").floatValue = 1.5f;
                so.FindProperty("_powerFill._driftCap").floatValue = 6f;
                so.ApplyModifiedPropertiesWithoutUndo();
                var tuning = new TuningStore(new EventBus());
                TuningSeeder.Seed(tuning, shift, patrons, blessings, 6, powers: _config, lot: lot);
                Assert.AreEqual(25f, tuning.Get(TuningKey.PowerFillPerfectPark));
                Assert.AreEqual(12f, tuning.Get(TuningKey.PowerFillPerfectDropOff));
                Assert.AreEqual(1.5f, tuning.Get(TuningKey.PowerFillDriftPerSecond));
                Assert.AreEqual(6f, tuning.Get(TuningKey.PowerFillDriftCap));
                Assert.IsTrue(lot.TryValidate(out var problem), problem);
                var plain = new TuningStore(new EventBus());
                TuningSeeder.Seed(plain, shift, patrons, blessings, 6, powers: _config);
                Assert.AreEqual(18f, plain.Get(TuningKey.PowerFillPerfectPark), "no lot: the config numbers");
            }
            finally
            {
                fx.Dispose();
                UnityEngine.Object.DestroyImmediate(shift);
                UnityEngine.Object.DestroyImmediate(patrons);
                UnityEngine.Object.DestroyImmediate(blessings);
            }
        }

        [Test]
        public void ALotsDefaultFillAmountsAreTheOwnersNumbers()
        {
            var fx = new RunFixtures();
            try
            {
                var fill = fx.Lot(World.Midgard, LotKind.Training).PowerFill;
                Assert.AreEqual(18f, fill.PerfectPark);
                Assert.AreEqual(10f, fill.PerfectDropOff);
                Assert.IsTrue(fill.IsSane);
            }
            finally
            {
                fx.Dispose();
            }
        }

        [Test]
        public void Cooldowns_StartAfterwardsTickAndResetCleanly()
        {
            var cooldowns = new PowerCooldowns();
            Assert.IsTrue(cooldowns.IsReady(StoryPower.StormWind));
            cooldowns.Start(StoryPower.StormWind, 4f);
            Assert.IsFalse(cooldowns.IsReady(StoryPower.StormWind));
            cooldowns.Tick(1f);
            Assert.AreEqual(0.75f, cooldowns.Remaining01(StoryPower.StormWind), 0.0001f);
            cooldowns.Start(StoryPower.FreezeTime, 0f);
            Assert.IsTrue(cooldowns.IsReady(StoryPower.FreezeTime), "a zero cooldown never blocks");
            cooldowns.Tick(10f);
            Assert.IsTrue(cooldowns.IsReady(StoryPower.StormWind));
            Assert.AreEqual(0f, cooldowns.Remaining(StoryPower.StormWind));
            cooldowns.Start(StoryPower.StormWind, 4f);
            cooldowns.ResetAll();
            Assert.IsTrue(cooldowns.IsReady(StoryPower.StormWind));
        }

        [Test]
        public void TheDefinitionAssetCarriesItsTwoLevels()
        {
            var definition = ScriptableObject.CreateInstance<PowerDefinition>();
            try
            {
                definition.ApplyDefaults(StoryPower.StormWind);
                Assert.AreEqual("Storm wind", definition.DisplayName);
                Assert.IsNotEmpty(definition.Description);
                Assert.IsTrue(definition.TryGetValues(1, out var one));
                Assert.IsTrue(definition.TryGetValues(2, out var two));
                Assert.IsFalse(definition.TryGetValues(3, out _));
                Assert.Less(two.Cooldown, one.Cooldown, "the upgrade is better");
                Assert.AreEqual(PowerDefaults.Values(StoryPower.StormWind, 2).Width, two.Width);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(definition);
            }
        }
    }
}
