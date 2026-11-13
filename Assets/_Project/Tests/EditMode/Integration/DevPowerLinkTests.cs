using System;
using System.Linq;
using NUnit.Framework;
using UncannyValet.Audio;
using UncannyValet.Bootstrap;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Run;
using UncannyValet.Tests.Run;

namespace UncannyValet.Tests.Integration
{
    public class DevPowerLinkTests
    {
        private RunFixtures _fx;
        private EventBus _bus;
        private TuningStore _tuning;
        private RunProgress _progress;
        private RunProgressLink _runLink;

        [SetUp]
        public void SetUp()
        {
            _fx = new RunFixtures();
            _bus = new EventBus();
            _tuning = new TuningStore(_bus);
            _tuning.Seed(TuningKey.DevOwnFreezeTime, 0f);
            _progress = new RunProgress(_fx.MidgardRun());
            _runLink = new RunProgressLink(_bus, _progress);
        }

        [TearDown]
        public void TearDown()
        {
            _runLink.Dispose();
            _fx.Dispose();
        }

        [Test]
        public void TheRowGrantsAndRevokesFreezeTimeInTheRunProgress()
        {
            using (new DevPowerLink(_bus, _tuning, _progress))
            {
                _tuning.Set(TuningKey.DevOwnFreezeTime, 1f);
                Assert.AreEqual(1, _progress.PowerLevel(StoryPower.FreezeTime));
                Assert.AreEqual(1, _progress.EarnedPowers.Count);
                _tuning.Set(TuningKey.DevOwnFreezeTime, 0f);
                Assert.AreEqual(0, _progress.PowerLevel(StoryPower.FreezeTime));
                Assert.AreEqual(0, _progress.EarnedPowers.Count);
            }
        }

        [Test]
        public void TheRowShowsWhatIsAlreadyOwnedAtStart_AndStopsAfterDispose()
        {
            _progress.GrantPower(StoryPower.FreezeTime, 1);
            var link = new DevPowerLink(_bus, _tuning, _progress);
            Assert.AreEqual(1f, _tuning.Get(TuningKey.DevOwnFreezeTime));
            link.Dispose();
            _tuning.Set(TuningKey.DevOwnFreezeTime, 0f);
            Assert.AreEqual(1, _progress.PowerLevel(StoryPower.FreezeTime), "no subscription left behind");
        }

        [Test]
        public void Revoking_ANotOwnedPower_ChangesNothing()
        {
            _progress.RevokePower(StoryPower.FreezeTime);
            _progress.GrantPower(StoryPower.ThunderStride, 1);
            _progress.RevokePower(StoryPower.FreezeTime);
            Assert.AreEqual(1, _progress.EarnedPowers.Count);
        }

        [Test]
        public void TheCatalogHasTheNewRowsAndNoneOfTheRetiredOnes()
        {
            var keys = TuningCatalog.All.Select(i => i.Key).ToList();
            foreach (var key in new[] { TuningKey.PowerFillPerfectPark, TuningKey.PowerFillPerfectDropOff,
                TuningKey.PowerFillDriftPerSecond, TuningKey.PowerFillDriftCap, TuningKey.DevOwnFreezeTime })
                Assert.Contains(key, keys);
            foreach (var info in TuningCatalog.All.Where(i => i.Group == PowerRows.BarGroup))
                StringAssert.StartsWith("Power bar: ", info.Label);
            Assert.AreEqual(4, TuningCatalog.All.Count(i => i.Group == PowerRows.BarGroup));
            Assert.IsTrue(TuningCatalog.All.Single(i => i.Key == TuningKey.DevOwnFreezeTime).Advanced);
            foreach (var name in Enum.GetNames(typeof(TuningKey)).Where(n => n.StartsWith("RetiredPower", StringComparison.Ordinal)))
                Assert.IsFalse(keys.Contains((TuningKey)Enum.Parse(typeof(TuningKey), name)), name + " must have no row");
        }

        [Test]
        public void NoEnumNumberShifted()
        {
            var fill = (int)TuningKey.PowerFillPerfectPark;
            Assert.AreEqual(fill + 4, (int)TuningKey.RetiredPowerLevel1Threshold);
            Assert.AreEqual(fill + 7, (int)TuningKey.RetiredPowerSpeedBoostSeconds);
            Assert.AreEqual(fill + 9, (int)TuningKey.PowerFreezeSeconds);
            Assert.AreEqual(fill + 11, (int)TuningKey.PerfectDropOffAngle);
            var target = (int)TuningKey.TrafficTargetOnScreen;
            Assert.AreEqual(target + 1, (int)TuningKey.RetiredPowerStumbleSeconds);
            Assert.AreEqual(target + 3, (int)TuningKey.QueueClearDistance);
            Assert.AreEqual((int)TuningKey.ShakeFullForce + 1, (int)TuningKey.DevOwnFreezeTime, "appended at the end");
            Assert.AreEqual(2, (int)PowerKind.FreezeTime, "freeze time keeps number 2");
            Assert.AreEqual(16, (int)SfxId.PerfectDing);
            Assert.AreEqual(18, (int)SfxId.TimeStop);
            Assert.AreEqual(19, (int)SfxId.TimeResume);
        }
    }
}
