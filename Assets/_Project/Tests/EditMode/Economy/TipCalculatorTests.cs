using NUnit.Framework;
using UnityEngine;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Economy;

namespace UncannyValet.Tests.Economy
{
    public class TipCalculatorTests
    {
        private TipCalculator _calc;

        [SetUp]
        public void SetUp()
        {
            // Defaults: window 10 min, bonus 2, impatient bonus 6, penalty 0.5 per minute.
            _calc = new TipCalculator(ScriptableObject.CreateInstance<TipConfig>());
        }

        private int Tip(int baseTip, float wait, bool impatient = false)
            => _calc.Calculate(new TipInput(baseTip, wait, impatient));

        [Test] public void InsideWindow_AddsBonus() => Assert.AreEqual(12, Tip(10, 5f));
        [Test] public void ExactlyAtWindow_StillGetsBonus() => Assert.AreEqual(12, Tip(10, 10f));
        [Test] public void Impatient_GetsBiggerBonus() => Assert.AreEqual(16, Tip(10, 5f, true));
        [Test] public void AfterWindow_PenaltyPerMinute() => Assert.AreEqual(8, Tip(10, 14f));
        [Test] public void PenaltyGrowsWithWait() => Assert.Less(Tip(10, 18f), Tip(10, 14f));
        [Test] public void NeverNegative() => Assert.AreEqual(0, Tip(10, 500f));
        [Test] public void NegativeBaseClampsToZero() => Assert.AreEqual(0, Tip(-5, 0f));
        [Test] public void Impatient_LateGetsNoBonus() => Assert.AreEqual(8, Tip(10, 14f, true));

        private TipBreakdown Explain(TipInput input) => _calc.Explain(input);

        [Test]
        public void Explain_SplitsBaseAndSpeed()
        {
            var b = Explain(new TipInput(10, 5f, false));
            Assert.AreEqual(10, b.BaseTip);
            Assert.AreEqual(2, b.SpeedAdjust);
            Assert.AreEqual(12, b.Total);
            Assert.AreEqual(TipZeroReason.None, b.ZeroReason);
        }

        [Test]
        public void Explain_PenaltyIsNegative() => Assert.AreEqual(-2, Explain(new TipInput(10, 14f, false)).SpeedAdjust);

        [Test]
        public void Explain_ChargeBonusOnlyWhenCharged()
        {
            Assert.AreEqual(15, Explain(new TipInput(10, 5f, false, chargedEv: true)).ChargeBonus);
            Assert.AreEqual(27, Explain(new TipInput(10, 5f, false, chargedEv: true)).Total);
            Assert.AreEqual(0, Explain(new TipInput(10, 5f, false)).ChargeBonus);
        }

        [Test]
        public void Explain_CleanBonusOnlyWhenCleaned()
        {
            Assert.AreEqual(15, Explain(new TipInput(10, 5f, false, cleanedDirty: true)).CleanBonus);
            Assert.AreEqual(27, Explain(new TipInput(10, 5f, false, cleanedDirty: true)).Total);
        }

        [Test]
        public void Explain_PickupExpiredZeroes()
        {
            var b = Explain(new TipInput(10, 5f, false, pickupExpired: true, chargedEv: true));
            Assert.AreEqual(0, b.Total);
            Assert.AreEqual(TipZeroReason.PickupExpired, b.ZeroReason);
        }

        [Test]
        public void Explain_UncleanedZeroes()
        {
            var b = Explain(new TipInput(10, 5f, false, uncleanedDirty: true));
            Assert.AreEqual(0, b.Total);
            Assert.AreEqual(TipZeroReason.NotCleaned, b.ZeroReason);
        }

        [Test]
        public void Explain_PickupExpiredWinsOverUncleaned()
        {
            var b = Explain(new TipInput(10, 5f, false, pickupExpired: true, uncleanedDirty: true));
            Assert.AreEqual(TipZeroReason.PickupExpired, b.ZeroReason);
        }

        [Test]
        public void Calculate_EqualsExplainTotal()
        {
            var input = new TipInput(10, 14f, true, chargedEv: true, cleanedDirty: true);
            Assert.AreEqual(Explain(input).Total, _calc.Calculate(input));
        }

        [Test]
        public void Calculate_ForcedZeroNeverNegative()
            => Assert.AreEqual(0, _calc.Calculate(new TipInput(10, 0f, false, pickupExpired: true)));

        [Test]
        public void PatienceLeft_AddsAScaledBonus()
        {
            var full = Explain(new TipInput(10, 5f, false, patienceRemaining01: 1f));
            Assert.AreEqual(10, full.PatienceBonus);
            Assert.AreEqual(22, full.Total);
            Assert.AreEqual(5, Explain(new TipInput(10, 5f, false, patienceRemaining01: 0.5f)).PatienceBonus);
            Assert.AreEqual(0, Explain(new TipInput(10, 5f, false, patienceRemaining01: 0f)).PatienceBonus);
            Assert.AreEqual(10, Explain(new TipInput(10, 5f, false, patienceRemaining01: 7f)).PatienceBonus, "clamped to 1");
        }

        [TestCase(0.24f, 0)]
        [TestCase(0.25f, 25)]
        [TestCase(0.5f, 50)]
        [TestCase(0.99f, 75)]
        public void Damage_TakesOffAQuarterPerStage(float damage, int percentLost)
        {
            var b = Explain(new TipInput(98, 5f, false, damage01: damage));
            Assert.AreEqual(-(100 * percentLost / 100), b.DamageAdjust);
            Assert.AreEqual(100 - 100 * percentLost / 100, b.Total);
        }

        [Test]
        public void Wreck_PaysNothing()
        {
            var b = Explain(new TipInput(10, 0f, false, damage01: 1f));
            Assert.AreEqual(0, b.Total);
            Assert.AreEqual(TipZeroReason.Wrecked, b.ZeroReason);
        }

        [Test]
        public void Combo_MultipliesTheFinalTip()
        {
            Assert.AreEqual(12, Explain(new TipInput(10, 5f, false, comboMultiplier: 1)).Total);
            var b = Explain(new TipInput(10, 5f, false, comboMultiplier: 3));
            Assert.AreEqual(36, b.Total);
            Assert.AreEqual(3, b.ComboMultiplier);
        }

        [Test]
        public void Combo_BelowOneCountsAsOne_AndDefaultInputIsSafe()
        {
            Assert.AreEqual(12, Explain(new TipInput(10, 5f, false, comboMultiplier: 0)).Total);
            Assert.GreaterOrEqual(_calc.Calculate(default), 0);
        }

        [Test]
        public void Combo_AppliesAfterDamage()
        {
            var b = Explain(new TipInput(98, 5f, false, damage01: 0.5f, comboMultiplier: 2));
            Assert.AreEqual(100, b.Total, "(98 + 2) * 0.5 * 2");
        }

        [Test]
        public void ForcedZero_IgnoresCombo()
            => Assert.AreEqual(0, _calc.Calculate(new TipInput(10, 0f, false, pickupExpired: true, comboMultiplier: 4)));

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
