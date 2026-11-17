using NUnit.Framework;
using UncannyValet.Core.Ids;
using UncannyValet.UI.Hud;
using UnityEngine;

namespace UncannyValet.Tests.UI
{
    public class BlessingPopTests
    {
        private readonly CarId _a = new CarId(1);
        private readonly CarId _b = new CarId(2);
        private PendingBlessingPops _pending;

        [SetUp]
        public void SetUp() => _pending = new PendingBlessingPops();

        [Test]
        public void DeliveredThenClosedThenBoarded_GivesTheBlessing()
        {
            _pending.Delivered(_a);
            _pending.Closed(_a, 37, Vector2.zero);
            Assert.IsTrue(_pending.TryBoard(_a, out var blessing, out _));
            Assert.AreEqual(37, blessing);
            Assert.AreEqual(0, _pending.Count);
        }

        [Test]
        public void BoardedBeforeTheAmountIsKnown_GivesNothingAndForgets()
        {
            _pending.Delivered(_a);
            Assert.IsFalse(_pending.TryBoard(_a, out _, out _));
            Assert.AreEqual(0, _pending.Count);
        }

        [Test]
        public void ClosedWithoutDelivery_IsNeverHeld()
        {
            _pending.Closed(_a, 0, Vector2.zero);
            Assert.AreEqual(0, _pending.Count);
            Assert.IsFalse(_pending.TryBoard(_a, out _, out _));
        }

        [Test]
        public void ZeroBlessing_IsStillHeldAndPops()
        {
            _pending.Delivered(_a);
            _pending.Closed(_a, 0, Vector2.zero);
            Assert.IsTrue(_pending.TryBoard(_a, out var blessing, out _));
            Assert.AreEqual(0, blessing);
        }

        [Test]
        public void KeepsTheSpotGivenWhenTheTicketClosed()
        {
            _pending.Delivered(_a);
            _pending.Closed(_a, 20, new Vector2(3f, 4f));
            Assert.IsTrue(_pending.TryBoard(_a, out _, out var at));
            Assert.AreEqual(new Vector2(3f, 4f), at);
        }

        [Test]
        public void TwoCarsAtOnce_PairIndependently()
        {
            _pending.Delivered(_a);
            _pending.Delivered(_b);
            _pending.Closed(_a, 10, Vector2.zero);
            _pending.Closed(_b, 55, Vector2.zero);
            Assert.IsTrue(_pending.TryBoard(_b, out var second, out _));
            Assert.IsTrue(_pending.TryBoard(_a, out var first, out _));
            Assert.AreEqual(55, second);
            Assert.AreEqual(10, first);
        }

        [Test]
        public void Clear_ForgetsEverything()
        {
            _pending.Delivered(_a);
            _pending.Closed(_a, 10, Vector2.zero);
            _pending.Delivered(_b);
            _pending.Clear();
            Assert.AreEqual(0, _pending.Count);
            Assert.IsFalse(_pending.TryBoard(_a, out _, out _));
        }

        [Test]
        public void Text_ShowsPlusAmountOrZero()
        {
            Assert.AreEqual("+25", BlessingPopText.Format(25));
            Assert.AreEqual("0", BlessingPopText.Format(0));
            Assert.AreEqual("0", BlessingPopText.Format(-5));
        }

        [Test]
        public void Kind_SplitsZeroNormalAndBig()
        {
            Assert.AreEqual(BlessingPopKind.Zero, BlessingPopText.KindOf(0, 40));
            Assert.AreEqual(BlessingPopKind.Normal, BlessingPopText.KindOf(39, 40));
            Assert.AreEqual(BlessingPopKind.Big, BlessingPopText.KindOf(40, 40));
            Assert.AreEqual(BlessingPopKind.Normal, BlessingPopText.KindOf(500, 0), "threshold 0 turns the big look off");
        }

        [Test]
        public void Alpha_HoldsThenFadesToNothing()
        {
            Assert.AreEqual(1f, BlessingPopText.Alpha(0f, 1.2f, 0.4f), 1e-4f);
            Assert.AreEqual(1f, BlessingPopText.Alpha(0.48f, 1.2f, 0.4f), 1e-4f);
            Assert.AreEqual(0.5f, BlessingPopText.Alpha(0.84f, 1.2f, 0.4f), 1e-4f);
            Assert.AreEqual(0f, BlessingPopText.Alpha(5f, 1.2f, 0.4f), 1e-4f);
        }

        [Test]
        public void Rise_EasesFromZeroToOne()
        {
            Assert.AreEqual(0f, BlessingPopText.Rise(0f, 1.2f), 1e-4f);
            Assert.Greater(BlessingPopText.Rise(0.6f, 1.2f), 0.5f);
            Assert.AreEqual(1f, BlessingPopText.Rise(9f, 1.2f), 1e-4f);
        }

        [Test]
        public void Pool_ExpiresAfterTheDuration()
        {
            var settings = new BlessingPopSettings();
            var pops = new BlessingPops(settings);
            pops.Start(new Vector2(1f, 2f), 12, 5f);
            Assert.AreEqual(1, pops.ActiveCount);
            pops.Expire(5f + settings.DurationSeconds - 0.01f);
            Assert.AreEqual(1, pops.ActiveCount);
            pops.Expire(5f + settings.DurationSeconds + 0.01f);
            Assert.AreEqual(0, pops.ActiveCount);
        }

        [Test]
        public void Pool_WhenFullReplacesTheOldest()
        {
            var pops = new BlessingPops(new BlessingPopSettings());
            for (var i = 0; i < pops.Capacity; i++) pops.Start(Vector2.zero, i + 1, i);
            pops.Start(Vector2.zero, 99, 100f);
            Assert.AreEqual(pops.Capacity, pops.ActiveCount);
            var found = false;
            for (var i = 0; i < pops.Capacity; i++)
            {
                Assert.AreNotEqual("+1", pops[i].Text, "the oldest was replaced");
                found |= pops[i].Text == "+99";
            }
            Assert.IsTrue(found);
        }

        [Test]
        public void Pool_KeepsItsStartPosition()
        {
            var pops = new BlessingPops(new BlessingPopSettings());
            pops.Start(new Vector2(3f, 4f), 5, 0f);
            Assert.AreEqual(new Vector2(3f, 4f), pops[0].World);
        }
    }
}
