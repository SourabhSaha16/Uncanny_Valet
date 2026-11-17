using NUnit.Framework;
using UncannyValet.Cars;
using UncannyValet.Cars.Fx;
using UnityEngine;

namespace UncannyValet.Tests.Cars
{
    public class DecalLedgerTests
    {
        [Test]
        public void Acquire_UsesFreeSlotsThenNeverExceedsTheCap()
        {
            var ledger = new DecalLedger(3);
            for (var i = 0; i < 10; i++) ledger.Acquire(6f, 0.8f);
            Assert.AreEqual(3, ledger.ActiveCount);
            Assert.AreEqual(3, ledger.Capacity);
        }

        [Test]
        public void Acquire_WhenFullRecyclesTheOldest()
        {
            var ledger = new DecalLedger(3);
            var a = ledger.Acquire(60f, 1f);
            ledger.Tick(1f);
            var b = ledger.Acquire(60f, 1f);
            ledger.Tick(1f);
            ledger.Acquire(60f, 1f);
            ledger.Tick(1f);
            Assert.AreEqual(a, ledger.Acquire(60f, 1f), "a is the oldest");
            ledger.Tick(1f);
            Assert.AreEqual(b, ledger.Acquire(60f, 1f), "then b");
        }

        [Test]
        public void Alpha_FadesLinearlyToZeroAndTheSlotIsFreedAfterTheLife()
        {
            var ledger = new DecalLedger(2);
            var slot = ledger.Acquire(6f, 0.8f);
            Assert.AreEqual(0.8f, ledger.Alpha(slot), 1e-5f);
            ledger.Tick(3f);
            Assert.AreEqual(0.4f, ledger.Alpha(slot), 1e-5f);
            ledger.Tick(3f);
            Assert.IsFalse(ledger.IsActive(slot));
            Assert.AreEqual(0, ledger.ActiveCount);
        }

        [Test]
        public void Alpha_HoldsThenFadesOverTheTailOnly()
        {
            var ledger = new DecalLedger(1);
            var slot = ledger.Acquire(30f, 0.8f, 8f);
            ledger.Tick(21f);
            Assert.AreEqual(0.8f, ledger.Alpha(slot), 1e-5f, "full opacity until the tail starts");
            ledger.Tick(5f);
            Assert.AreEqual(0.8f * 4f / 8f, ledger.Alpha(slot), 1e-4f);
            ledger.Tick(4f);
            Assert.IsFalse(ledger.IsActive(slot));
        }

        [Test]
        public void Alpha_TailLongerThanTheLifeFallsBackToTheWholeLife()
        {
            var ledger = new DecalLedger(1);
            var slot = ledger.Acquire(4f, 1f, 99f);
            ledger.Tick(1f);
            Assert.AreEqual(0.75f, ledger.Alpha(slot), 1e-5f);
        }

        [Test]
        public void DefaultPool_HoldsThirtySecondsOfNonStopFullSpeedDrifting()
        {
            var config = ScriptableObject.CreateInstance<CarHandlingConfig>();
            var car = ScriptableObject.CreateInstance<CarDefinition>();
            var skid = config.Skid;
            var marksPerSecond = 2f * car.TopSpeed * config.TopSpeedScale / skid.MarkSpacing;
            var ledger = new DecalLedger(skid.DecalCap);
            var steps = Mathf.FloorToInt(marksPerSecond * (skid.MarkFadeSeconds - 1f));
            for (var i = 0; i < steps; i++) { ledger.Acquire(skid.MarkFadeSeconds, 1f, skid.MarkFadeOutSeconds); ledger.Tick(1f / marksPerSecond); }
            Assert.AreEqual(steps, ledger.ActiveCount, "no mark recycled or expired before its 30 s are up");
            Assert.Less(marksPerSecond * skid.MarkFadeSeconds, skid.DecalCap);
            Object.DestroyImmediate(car);
            Object.DestroyImmediate(config);
        }

        [Test]
        public void FreedSlotsAreReusedBeforeRecycling()
        {
            var ledger = new DecalLedger(2);
            ledger.Acquire(1f, 1f);
            var keep = ledger.Acquire(10f, 1f);
            ledger.Tick(2f);
            var next = ledger.Acquire(10f, 1f);
            Assert.AreNotEqual(keep, next);
            Assert.AreEqual(2, ledger.ActiveCount);
        }

        [Test]
        public void Pool_DrawsCappedTintedDecalsAndHidesExpiredOnes()
        {
            var texture = new Texture2D(1, 1);
            var sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            var parent = new GameObject("Parent").transform;
            var pool = DecalPool.Create(parent, 4, sprite, -3);
            var tint = new Color(0.8f, 0.1f, 0.1f, 1f);
            for (var i = 0; i < 9; i++) pool.Place(new Vector2(i, 0f), 10f, 1f, 0.07f, tint, 0.8f, 6f);
            Assert.AreEqual(4, pool.ActiveCount);
            var renderers = pool.GetComponentsInChildren<SpriteRenderer>();
            Assert.AreEqual(4, renderers.Length);
            Assert.AreEqual(0.8f, renderers[0].color.a, 1e-4f);
            Assert.AreEqual(0.8f, renderers[0].color.r, 1e-4f, "the tint is kept, so blood tracks can reuse the pool");
            Assert.AreEqual(-3, renderers[0].sortingOrder);
            pool.Tick(3f);
            Assert.AreEqual(0.4f, renderers[0].color.a, 1e-3f);
            pool.Tick(3.1f);
            Assert.AreEqual(0, pool.ActiveCount);
            foreach (var r in renderers) Assert.IsFalse(r.enabled);
            Object.DestroyImmediate(parent.gameObject);
            Object.DestroyImmediate(sprite);
            Object.DestroyImmediate(texture);
        }
    }
}
