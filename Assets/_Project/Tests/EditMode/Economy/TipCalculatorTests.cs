using NUnit.Framework;
using UnityEngine;
using UncannyValet.Core.Contracts;
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
    }
}
