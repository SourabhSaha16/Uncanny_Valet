using NUnit.Framework;
using UnityEngine;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Modifiers;
using UncannyValet.Economy;

namespace UncannyValet.Tests.Economy
{
    public class BlessingCalculatorTests
    {
        private BlessingCalculator _calc;

        [SetUp]
        public void SetUp()
        {
            // Defaults: window 10 min, bonus 2, impatient bonus 6, penalty 0.5 per minute.
            _calc = new BlessingCalculator(ScriptableObject.CreateInstance<BlessingConfig>(), null, ModifierTestKit.Catalog());
        }

        private int Blessing(int baseBlessing, float wait, bool impatient = false)
            => _calc.Calculate(new BlessingInput(baseBlessing, wait, impatient));

        [Test] public void InsideWindow_AddsBonus() => Assert.AreEqual(12, Blessing(10, 5f));
        [Test] public void ExactlyAtWindow_StillGetsBonus() => Assert.AreEqual(12, Blessing(10, 10f));
        [Test] public void Impatient_GetsBiggerBonus() => Assert.AreEqual(16, Blessing(10, 5f, true));
        [Test] public void AfterWindow_PenaltyPerMinute() => Assert.AreEqual(8, Blessing(10, 14f));
        [Test] public void PenaltyGrowsWithWait() => Assert.Less(Blessing(10, 18f), Blessing(10, 14f));
        [Test] public void NeverNegative() => Assert.AreEqual(0, Blessing(10, 500f));
        [Test] public void NegativeBaseClampsToZero() => Assert.AreEqual(0, Blessing(-5, 0f));
        [Test] public void Impatient_LateGetsNoBonus() => Assert.AreEqual(8, Blessing(10, 14f, true));

        private static readonly ModifierSet Charged = ModifierSet.Of(ModifierId.NeedsCharge);
        private static readonly ModifierSet Washed = ModifierSet.Of(ModifierId.Muddy);

        private BlessingBreakdown Explain(BlessingInput input) => _calc.Explain(input);

        [Test]
        public void Explain_SplitsBaseAndSpeed()
        {
            var b = Explain(new BlessingInput(10, 5f, false));
            Assert.AreEqual(10, b.BaseBlessing);
            Assert.AreEqual(2, b.SpeedAdjust);
            Assert.AreEqual(12, b.Total);
            Assert.AreEqual(BlessingZeroReason.None, b.ZeroReason);
        }

        [Test]
        public void Explain_PenaltyIsNegative() => Assert.AreEqual(-2, Explain(new BlessingInput(10, 14f, false)).SpeedAdjust);

        [Test]
        public void Explain_ModifierBonusOnlyForClearedModifiers()
        {
            Assert.AreEqual(15, Explain(new BlessingInput(10, 5f, false, clearedModifiers: Charged)).ModifierBonus);
            Assert.AreEqual(27, Explain(new BlessingInput(10, 5f, false, clearedModifiers: Charged)).Total);
            Assert.AreEqual(0, Explain(new BlessingInput(10, 5f, false)).ModifierBonus);
            Assert.AreEqual(0, Explain(new BlessingInput(10, 5f, false, openModifiers: Charged)).ModifierBonus);
        }

        [Test]
        public void Explain_MuddyClearedPaysItsBonus()
        {
            Assert.AreEqual(15, Explain(new BlessingInput(10, 5f, false, clearedModifiers: Washed)).ModifierBonus);
            Assert.AreEqual(27, Explain(new BlessingInput(10, 5f, false, clearedModifiers: Washed)).Total);
        }

        [Test]
        public void Explain_TwoClearedModifiersPayBothBonuses()
        {
            var both = ModifierSet.Of(ModifierId.Muddy, ModifierId.NeedsCharge);
            Assert.AreEqual(30, Explain(new BlessingInput(10, 5f, false, clearedModifiers: both)).ModifierBonus);
        }

        [Test]
        public void Explain_WithoutACatalogNoModifierBonusIsPaid()
        {
            var bare = new BlessingCalculator(ScriptableObject.CreateInstance<BlessingConfig>());
            Assert.AreEqual(0, bare.Explain(new BlessingInput(10, 5f, false, clearedModifiers: Charged)).ModifierBonus);
        }

        [Test]
        public void Explain_PickupExpiredZeroes()
        {
            var b = Explain(new BlessingInput(10, 5f, false, pickupExpired: true, clearedModifiers: Charged));
            Assert.AreEqual(0, b.Total);
            Assert.AreEqual(BlessingZeroReason.PickupExpired, b.ZeroReason);
        }

        [Test]
        public void Explain_UnclearedMuddyCar_PaysBaseAndSpeed_WithNoModifierBonus()
        {
            var clean = Explain(new BlessingInput(10, 5f, false, clearedModifiers: Washed));
            var unclean = Explain(new BlessingInput(10, 5f, false, openModifiers: Washed));
            Assert.AreEqual(BlessingZeroReason.None, unclean.ZeroReason);
            Assert.AreEqual(0, unclean.ModifierBonus);
            Assert.AreEqual(10 + unclean.SpeedAdjust, unclean.Total);
            Assert.Greater(unclean.Total, 0);
            Assert.Less(unclean.Total, clean.Total);
        }

        [Test]
        public void Explain_PickupExpiredWinsOverUncleaned()
        {
            var b = Explain(new BlessingInput(10, 5f, false, pickupExpired: true, openModifiers: Washed));
            Assert.AreEqual(BlessingZeroReason.PickupExpired, b.ZeroReason);
        }

        [Test]
        public void Calculate_EqualsExplainTotal()
        {
            var input = new BlessingInput(10, 14f, true, clearedModifiers: ModifierSet.Of(ModifierId.Muddy, ModifierId.NeedsCharge));
            Assert.AreEqual(Explain(input).Total, _calc.Calculate(input));
        }

        [Test]
        public void Calculate_ForcedZeroNeverNegative()
            => Assert.AreEqual(0, _calc.Calculate(new BlessingInput(10, 0f, false, pickupExpired: true)));

        [Test]
        public void PatienceLeft_AddsAScaledBonus()
        {
            var full = Explain(new BlessingInput(10, 5f, false, patienceRemaining01: 1f));
            Assert.AreEqual(10, full.PatienceBonus);
            Assert.AreEqual(22, full.Total);
            Assert.AreEqual(5, Explain(new BlessingInput(10, 5f, false, patienceRemaining01: 0.5f)).PatienceBonus);
            Assert.AreEqual(0, Explain(new BlessingInput(10, 5f, false, patienceRemaining01: 0f)).PatienceBonus);
            Assert.AreEqual(10, Explain(new BlessingInput(10, 5f, false, patienceRemaining01: 7f)).PatienceBonus, "clamped to 1");
        }

        [TestCase(0.24f, 0)]
        [TestCase(0.25f, 25)]
        [TestCase(0.5f, 50)]
        [TestCase(0.99f, 75)]
        public void Damage_TakesOffAQuarterPerStage(float damage, int percentLost)
        {
            var b = Explain(new BlessingInput(98, 5f, false, damage01: damage));
            Assert.AreEqual(-(100 * percentLost / 100), b.DamageAdjust);
            Assert.AreEqual(100 - 100 * percentLost / 100, b.Total);
        }

        [Test]
        public void Wreck_PaysNothing()
        {
            var b = Explain(new BlessingInput(10, 0f, false, damage01: 1f));
            Assert.AreEqual(0, b.Total);
            Assert.AreEqual(BlessingZeroReason.Wrecked, b.ZeroReason);
        }

        [Test]
        public void Favour_MultipliesTheFinalBlessing()
        {
            Assert.AreEqual(12, Explain(new BlessingInput(10, 5f, false, favourMultiplier: 1)).Total);
            var b = Explain(new BlessingInput(10, 5f, false, favourMultiplier: 3));
            Assert.AreEqual(36, b.Total);
            Assert.AreEqual(3, b.FavourMultiplier);
        }

        [Test]
        public void Favour_BelowOneCountsAsOne_AndDefaultInputIsSafe()
        {
            Assert.AreEqual(12, Explain(new BlessingInput(10, 5f, false, favourMultiplier: 0)).Total);
            Assert.GreaterOrEqual(_calc.Calculate(default), 0);
        }

        [Test]
        public void Favour_AppliesAfterDamage()
        {
            var b = Explain(new BlessingInput(98, 5f, false, damage01: 0.5f, favourMultiplier: 2));
            Assert.AreEqual(100, b.Total, "(98 + 2) * 0.5 * 2");
        }

        [Test]
        public void ForcedZero_IgnoresFavour()
            => Assert.AreEqual(0, _calc.Calculate(new BlessingInput(10, 0f, false, pickupExpired: true, favourMultiplier: 4)));

        [Test]
        public void DamagePenalty_IsPure()
        {
            Assert.AreEqual(0f, DamagePenalty.Share(0f, 0.25f, 0.25f));
            Assert.AreEqual(0.5f, DamagePenalty.Share(0.5f, 0.25f, 0.25f), 1e-5f);
            Assert.AreEqual(1f, DamagePenalty.Share(1f, 0.25f, 0.25f), 1e-5f);
            Assert.IsTrue(DamagePenalty.IsWrecked(1f));
            Assert.IsFalse(DamagePenalty.IsWrecked(0.99f));
        }
    }
}
