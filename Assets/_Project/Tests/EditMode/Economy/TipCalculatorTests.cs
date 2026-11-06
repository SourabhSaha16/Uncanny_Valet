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
    }
}
