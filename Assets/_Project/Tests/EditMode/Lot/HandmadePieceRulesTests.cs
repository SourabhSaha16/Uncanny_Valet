using NUnit.Framework;
using UncannyValet.Lot.Handmade;
using UnityEngine;

namespace UncannyValet.Tests.Lot
{
    /// <summary>Validator rules 1 to 9, each with one failing case; the valid fixture passes clean.</summary>
    public class HandmadePieceRulesTests
    {
        private HandmadeLotData _d;

        [SetUp]
        public void SetUp() => _d = HandmadeLotFixture.Valid();

        private bool Fails(LotRule rule) => HandmadeLotFixture.Has(HandmadeLotFixture.Check(_d), rule, LotSeverity.Error);

        [Test]
        public void ValidFixture_HasNoIssues() => Assert.AreEqual(0, HandmadeLotFixture.Check(_d).Count);

        [Test]
        public void Rule1_TwoRoots_Fails()
        {
            _d.RootCount = 2;
            Assert.IsTrue(Fails(LotRule.SingleRoot));
        }

        [Test]
        public void Rule2_NoCurb_Fails()
        {
            _d.Curbs.Clear();
            Assert.IsTrue(Fails(LotRule.OneCurb));
        }

        [Test]
        public void Rule2_TwoCurbs_Fails()
        {
            _d.Curbs.Add(new LotPiece("Curb 2", new OrientedRect(new Vector2(40f, 0f), new Vector2(3f, 5f), 0f)));
            Assert.IsTrue(Fails(LotRule.OneCurb));
        }

        [Test]
        public void Rule3_RotatedCurb_Fails()
        {
            _d.Curbs[0] = new LotPiece("Curb", new OrientedRect(Vector2.zero, new Vector2(3f, 5f), 10f));
            Assert.IsTrue(Fails(LotRule.CurbUnrotated));
        }

        [Test]
        public void Rule4_TooFewSlots_Fails()
        {
            _d.Requirements.MinQueueSlots = 3;
            Assert.IsTrue(Fails(LotRule.QueueNumbering));
        }

        [Test]
        public void Rule4_TooManySlots_Fails()
        {
            _d.Requirements.MaxQueueSlots = 1;
            Assert.IsTrue(Fails(LotRule.QueueNumbering));
        }

        [Test]
        public void Rule4_GapInSlotIndices_Fails()
        {
            _d.Slots[1] = new LotPiece("Slot 1", _d.Slots[1].Rect, 3);
            Assert.IsTrue(Fails(LotRule.QueueNumbering));
        }

        [Test]
        public void Rule5_SlotWithOtherRotation_Fails()
        {
            _d.Slots[1] = new LotPiece("Slot 1", new OrientedRect(_d.Slots[1].Rect.Center, HandmadeLotFixture.CarSlot, 20f), 1);
            Assert.IsTrue(Fails(LotRule.QueueSharedRotation));
        }

        [Test]
        public void Rule6_SlotZeroFarFromCurb_Fails()
        {
            _d.Slots[0] = new LotPiece("Slot 0", new OrientedRect(new Vector2(0f, -9f), HandmadeLotFixture.CarSlot, 0f), 0);
            Assert.IsTrue(Fails(LotRule.SlotTouchesCurb));
        }

        [Test]
        public void Rule7_BayNumberRepeated_Fails()
        {
            _d.Bays[1] = new BayData("Bay 2", 1, BayKind.Regular, _d.Bays[1].Rect);
            Assert.IsTrue(Fails(LotRule.BayNumbering));
        }

        [Test]
        public void Rule7_BayNumbersWithGap_Fails()
        {
            _d.Bays[1] = new BayData("Bay 2", 3, BayKind.Regular, _d.Bays[1].Rect);
            Assert.IsTrue(Fails(LotRule.BayNumbering));
        }

        [Test]
        public void Rule7_ChargingBayNeededButMissing_Fails()
        {
            _d.Requirements.NeedsCharging = true;
            Assert.IsTrue(Fails(LotRule.BayNumbering));
        }

        [Test]
        public void Rule7_WashBayNeededAndPresent_Passes()
        {
            _d.Requirements.NeedsWash = true;
            _d.Bays.Add(new BayData("Wash", 1, BayKind.Wash, new OrientedRect(new Vector2(14f, 0f), HandmadeLotFixture.BaySize, 0f)));
            Assert.IsFalse(Fails(LotRule.BayNumbering));
        }

        [Test]
        public void Rule8_BayOverlapsBay_Fails()
        {
            _d.Bays[1] = new BayData("Bay 2", 2, BayKind.Regular, new OrientedRect(new Vector2(7f, 0f), HandmadeLotFixture.BaySize, 0f));
            Assert.IsTrue(Fails(LotRule.PiecesOverlap));
        }

        [Test]
        public void Rule8_RotatedBayOverlapsCurb_Fails()
        {
            _d.Bays[0] = new BayData("Bay 1", 1, BayKind.Regular, new OrientedRect(new Vector2(3.5f, 0f), HandmadeLotFixture.BaySize, 45f));
            Assert.IsTrue(Fails(LotRule.PiecesOverlap));
        }

        [Test]
        public void Rule8_TouchingBays_Pass()
        {
            _d.Bays[1] = new BayData("Bay 2", 2, BayKind.Regular, new OrientedRect(new Vector2(8.6f, 0f), HandmadeLotFixture.BaySize, 0f));
            Assert.IsFalse(Fails(LotRule.PiecesOverlap));
        }

        [Test]
        public void Rule9_BayNarrowerThanACar_Fails()
        {
            _d.Bays[0] = new BayData("Bay 1", 1, BayKind.Regular, new OrientedRect(new Vector2(6f, 0f), new Vector2(0.8f, 4.4f), 0f));
            Assert.IsTrue(Fails(LotRule.BayTooSmall));
        }
    }
}
