using System;
using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Audio;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.UI;
using UncannyValet.UI.Hud;
using UncannyValet.UI.Screens;
using UnityEngine;

namespace UncannyValet.Tests.UI
{
    public class SmashFxTests
    {
        private sealed class NoCars : ICarFactory
        {
            public IDrivable Spawn(CarId id, Vector2 position, float rotationDegrees) => null;
            public IDrivable SpawnArriving(CarId id, Vector2 stopPosition, float rotationDegrees, int arrivalSlot) => null;
            public void DriveAwayAndDespawn(CarId id) { }
            public void Despawn(CarId id) { }
            public bool TryGet(CarId id, out IDrivable car) { car = null; return false; }
        }

        [Test]
        public void Flash_StartsStrong_FadesToNothing()
        {
            Assert.AreEqual(SmashLook.FlashPeakAlpha, SmashLook.FlashAlpha(0f), 1e-4f);
            Assert.Less(SmashLook.FlashAlpha(SmashLook.FlashSeconds * 0.5f), SmashLook.FlashPeakAlpha);
            Assert.AreEqual(0f, SmashLook.FlashAlpha(SmashLook.FlashSeconds));
            Assert.AreEqual(0f, SmashLook.FlashAlpha(-1f));
        }

        [Test]
        public void Ring_GrowsAcrossTheLot_AndFades()
        {
            Assert.AreEqual(0, SmashLook.RingRadius(0f));
            Assert.AreEqual(-1, SmashLook.RingRadius(-0.1f));
            Assert.AreEqual(-1, SmashLook.RingRadius(SmashLook.RingSeconds));
            Assert.Greater(SmashLook.RingRadius(SmashLook.RingSeconds * 0.9f), SmashLook.RingRadius(SmashLook.RingSeconds * 0.5f));
            Assert.Greater(SmashLook.RingAlpha(0.1f), SmashLook.RingAlpha(0.6f));
        }

        [Test]
        public void HalfWidth_IsACircle()
        {
            Assert.AreEqual(10, SmashLook.HalfWidth(10, 0));
            Assert.AreEqual(0, SmashLook.HalfWidth(10, 10));
            Assert.AreEqual(0, SmashLook.HalfWidth(10, 20));
            Assert.AreEqual(6, SmashLook.HalfWidth(10, 8));
        }

        [Test]
        public void Pop_HoldsThenFades_AndRises()
        {
            Assert.AreEqual(1f, SmashLook.PopAlpha(0f));
            Assert.AreEqual(1f, SmashLook.PopAlpha(SmashLook.PopSeconds * SmashLook.PopHoldShare));
            Assert.Less(SmashLook.PopAlpha(SmashLook.PopSeconds * 0.9f), 1f);
            Assert.AreEqual(0f, SmashLook.PopAlpha(SmashLook.PopSeconds));
            Assert.AreEqual(0, SmashLook.PopRise(0f));
            Assert.AreEqual(SmashLook.PopRisePixels, SmashLook.PopRise(SmashLook.PopSeconds * 2f));
        }

        [Test]
        public void Puff_AndBurst_FillThePool_AndExpire()
        {
            var fx = new SmashFx();
            fx.Puff(Vector2.zero, 0f);
            fx.Burst(Vector2.zero, 0f);
            var active = 0;
            for (var i = 0; i < fx.Capacity; i++) if (fx[i].Active) active++;
            Assert.AreEqual(SmashLook.SparklesPerPuff + SmashLook.CoinsPerBurst + SmashLook.ConfettiPerBurst, active);

            fx.Expire(SmashLook.FallLife + 0.01f);
            active = 0;
            for (var i = 0; i < fx.Capacity; i++) if (fx[i].Active) active++;
            Assert.AreEqual(0, active);
        }

        [Test]
        public void ManyBursts_ReplaceTheOldest_NeverGrowThePool()
        {
            var fx = new SmashFx();
            for (var i = 0; i < SmashLook.MaxParticles; i++) fx.Burst(Vector2.zero, i * 0.001f);
            Assert.AreEqual(SmashLook.MaxParticles, fx.Capacity);
        }

        [Test]
        public void CoinsFall_SparklesDoNot()
        {
            var fx = new SmashFx();
            fx.Puff(Vector2.zero, 0f);
            fx.Burst(Vector2.zero, 0f);
            for (var i = 0; i < fx.Capacity; i++)
            {
                var p = fx[i];
                if (!p.Active) continue;
                var still = SmashFx.PositionAt(p, 0.5f).y - (p.Start.y + p.Velocity.y * 0.5f);
                if (p.Kind == SmashParticleKind.Sparkle) Assert.AreEqual(0f, still, 1e-4f);
                else Assert.Less(still, 0f);
            }
        }

        [Test]
        public void Tracker_ShowsThePop_FromCompletedAndStartsTheRingOnActivation()
        {
            var bus = new EventBus();
            var fx = new SmashFx();
            using (new SmashFxTracker(bus, new NoCars(), fx, () => 5f))
            {
                bus.Publish(new PowerActivated(PowerKind.FreezeTime));
                Assert.Greater(fx.RingAge(5f), 1000f, "only Park Smash starts the ring");
                bus.Publish(new PowerActivated(PowerKind.ParkSmash));
                Assert.AreEqual(0f, fx.RingAge(5f));
                bus.Publish(new ParkSmashCompleted(2, 1, 1, 3, 145));
                Assert.AreEqual("SMASH! +$145", fx.PopText);
                Assert.AreEqual(0f, fx.PopAge(5f));
            }
        }

        [Test]
        public void ShiftStats_CountsSmashTips_AndResetsOnANewShift()
        {
            var bus = new EventBus();
            using (var stats = new ShiftStats(bus))
            {
                bus.Publish(new ParkSmashCompleted(0, 0, 0, 2, 40));
                bus.Publish(new ParkSmashCompleted(0, 0, 0, 1, 25));
                Assert.AreEqual(65, stats.SmashTips);
                bus.Publish(new ShiftStarted());
                Assert.AreEqual(0, stats.SmashTips);
            }
        }

        [Test]
        public void Results_ShowSmashTipsOnlyWhenThereWereSome()
        {
            var result = new ShiftResult(100, 2, 0, 50, 90, 150);
            Assert.AreEqual(4, ResultsFormatter.Build(result, 3, 2, 1).Stats.Length);
            var with = ResultsFormatter.Build(result, 3, 2, 1, 80);
            Assert.AreEqual(5, with.Stats.Length);
            Assert.AreEqual("$80", with.Stats[4].Value);
        }

        [Test]
        public void Audio_BoomOnActivation_ChimesClimbPerCarAndResetWhenComplete()
        {
            var bus = new EventBus();
            var played = new List<SfxId>();
            var router = new SfxEventRouter(bus, played.Add);
            router.Subscribe();
            bus.Publish(new PowerActivated(PowerKind.ParkSmash));
            for (var i = 0; i < SfxSmash.ChimeSteps + 2; i++) bus.Publish(new ParkSmashCarParked(new CarId(i), 1, false, false));
            bus.Publish(new ParkSmashCarDelivered(new TicketId(1), new CarId(1), 10));
            Assert.AreEqual(SfxId.SmashBoom, played[0]);
            Assert.AreEqual(SfxId.SmashChime0, played[1]);
            Assert.AreEqual(SfxId.SmashChime1, played[2]);
            Assert.AreEqual(SfxId.SmashChime3, played[SfxSmash.ChimeSteps], "the top step repeats");
            Assert.AreEqual(SfxId.SmashChime3, played[played.Count - 1]);

            bus.Publish(new ParkSmashCompleted(1, 0, 0, 1, 10));
            played.Clear();
            bus.Publish(new ParkSmashCarParked(new CarId(1), 1, false, false));
            Assert.AreEqual(SfxId.SmashChime0, played[0], "the next smash starts low again");
            router.Unsubscribe();
        }
    }
}
