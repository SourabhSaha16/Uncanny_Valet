using NUnit.Framework;
using UncannyValet.Core.Events;
using UncannyValet.UI;

namespace UncannyValet.Tests.UI
{
    public class SummaryCardFormatterTests
    {
        private static NightSummary Summary(int night, int total, int tips, int parked, int notParked, int debt) =>
            new NightSummary(night, total, tips, parked, notParked, debt);

        [Test]
        public void NormalNight_HasTitleLinesAndNext()
        {
            var card = SummaryCardFormatter.Build(Summary(2, 7, 45, 5, 1, 1200));
            Assert.AreEqual("Night 2 complete", card.Title);
            Assert.IsFalse(card.HasHeadline);
            Assert.AreEqual(
                new[] { "Tips collected: $45", "Cars parked: 5", "Cars not parked: 1", "Debt left: $1200" },
                card.Lines);
            Assert.AreEqual("Next", card.ButtonLabel);
            Assert.AreEqual(2, card.Night);
        }

        [Test]
        public void FinalNight_PaidOff_ShowsWinHeadlineAndNewRun()
        {
            var card = SummaryCardFormatter.Build(Summary(7, 7, 300, 9, 0, 0));
            Assert.AreEqual("Debt paid off!", card.Headline);
            Assert.AreEqual("New run", card.ButtonLabel);
        }

        private static NightSummary Lost(int night, int debt) =>
            new NightSummary(night, 7, 10, 2, 4, debt, runLost: true);

        [Test]
        public void RunLost_TitleSaysBossIsFinished_AndButtonIsNewRun()
        {
            var card = SummaryCardFormatter.Build(Lost(3, 400));
            Assert.AreEqual(SummaryCardFormatter.RunLostTitle, card.Title);
            Assert.AreEqual("New run", card.ButtonLabel);
            Assert.AreEqual(3, card.Night);
            Assert.AreEqual(string.Empty, card.Tomorrow);
        }

        [Test]
        public void RunLost_HeadlineMentionsStrikes_NeverPaidOff()
        {
            var card = SummaryCardFormatter.Build(Lost(3, 400));
            StringAssert.Contains("strikes", card.Headline);
            StringAssert.DoesNotContain("paid off", card.Headline.ToLowerInvariant());
        }

        [Test]
        public void RunLost_WithNoDebtLeft_StillNotPaidOff()
        {
            var card = SummaryCardFormatter.Build(Lost(2, 0));
            Assert.AreNotEqual(SummaryCardFormatter.PaidOffHeadline, card.Headline);
        }

        [Test]
        public void FinalNight_NotPaidOff_ShowsDebtLeft()
        {
            var card = SummaryCardFormatter.Build(Summary(7, 7, 10, 2, 4, 350));
            Assert.AreEqual("Debt left: $350: you did not pay it off", card.Headline);
            Assert.AreEqual("New run", card.ButtonLabel);
        }

        private static NightSummary Detailed(int baseTips, int speed, int charge, int clean, int zeroed,
            string note = "", int night = 2) =>
            new NightSummary(night, 7, baseTips + speed + charge + clean, 5, 1, 900,
                baseTips, speed, charge, clean, zeroed, note);

        [Test]
        public void Calc_WithoutBonuses_ShowsBaseSpeedAndTotal()
        {
            var card = SummaryCardFormatter.Build(Detailed(40, 6, 0, 0, 0));
            Assert.AreEqual(new[] { "Base tips: $40", "Speed bonus: +$6", "= Tips collected: $46" }, card.CalcLines);
        }

        [Test]
        public void Calc_NegativeSpeed_IsAPenalty()
        {
            var card = SummaryCardFormatter.Build(Detailed(40, -4, 0, 0, 0));
            Assert.AreEqual("Speed penalty: -$4", card.CalcLines[1]);
        }

        [Test]
        public void Calc_AnyBonus_ShowsAllFourLines()
        {
            var card = SummaryCardFormatter.Build(Detailed(40, 0, 5, 0, 0));
            Assert.AreEqual(
                new[] { "Base tips: $40", "Speed bonus: $0", "Charge bonus: +$5", "Clean bonus: $0", "= Tips collected: $45" },
                card.CalcLines);
        }

        [Test]
        public void Calc_ZeroedTips_ShownOnlyWhenPositive()
        {
            Assert.AreEqual("Zeroed tips: 2", SummaryCardFormatter.Build(Detailed(40, 0, 0, 3, 2)).CalcLines[5]);
            Assert.AreEqual(3, SummaryCardFormatter.Build(Detailed(40, 6, 0, 0, 0)).CalcLines.Length);
        }

        [Test]
        public void Tomorrow_ShownWithNote_NotOnFinalNightOrWhenEmpty()
        {
            Assert.AreEqual("Tomorrow: EVs arrive", SummaryCardFormatter.Build(Detailed(1, 0, 0, 0, 0, "EVs arrive")).Tomorrow);
            Assert.IsTrue(SummaryCardFormatter.Build(Detailed(1, 0, 0, 0, 0, "EVs arrive")).HasTomorrow);
            Assert.IsFalse(SummaryCardFormatter.Build(Detailed(1, 0, 0, 0, 0, "")).HasTomorrow);
            Assert.IsFalse(SummaryCardFormatter.Build(Detailed(1, 0, 0, 0, 0, "x", 7)).HasTomorrow);
        }

        [Test]
        public void Signed_FormatsSignAndZero()
        {
            Assert.AreEqual("+$3", SummaryCardFormatter.Signed(3));
            Assert.AreEqual("-$3", SummaryCardFormatter.Signed(-3));
            Assert.AreEqual("$0", SummaryCardFormatter.Signed(0));
        }

        [Test]
        public void WrappedLineCount_WrapsOnWords()
        {
            Assert.AreEqual(0, SummaryCardLayout.WrappedLineCount("", 10));
            Assert.AreEqual(1, SummaryCardLayout.WrappedLineCount("aaa bbb", 10));
            Assert.AreEqual(2, SummaryCardLayout.WrappedLineCount("aaaaaa bbbbbb", 10));
            Assert.AreEqual(3, SummaryCardLayout.WrappedLineCount("aaaaaaaaaaaaaaaaaaaaaaaa", 10));
        }

        [Test]
        public void LayoutHeight_GrowsWithCalcAndNote()
        {
            var plain = SummaryCardLayout.Height(4, false, 10f, 5f, 20f);
            var full = SummaryCardLayout.Height(4, false, 5, 3, 10f, 5f, 20f);
            Assert.AreEqual(plain + 60f + 30f + 10f, full);
        }

        [Test]
        public void FitScale_ShrinksOnlyWhenTooTall()
        {
            Assert.AreEqual(1f, SummaryCardLayout.FitScale(500f, 600f, 10f));
            Assert.AreEqual(0.5f, SummaryCardLayout.FitScale(1160f, 600f, 10f), 0.001f);
        }

        [Test]
        public void WorstCaseCard_FitsAn800By600View()
        {
            // Final night is longest without a note; a normal night with zeroed tips, bonuses and a 3-line note.
            var height = SummaryCardLayout.Height(4, false, 7, 3, 18 * 1.5f, 14f, 44f);
            Assert.LessOrEqual(height, 600f - 20f);
        }

        [Test]
        public void LayoutHeight_GrowsWithHeadline()
        {
            var plain = SummaryCardLayout.Height(4, false, 10f, 5f, 20f);
            var final = SummaryCardLayout.Height(4, true, 10f, 5f, 20f);
            Assert.AreEqual(15f + 50f + 20f, plain);
            Assert.AreEqual(plain + 10f, final);
        }

        [Test]
        public void Centre_PlacesBoxInMiddle()
        {
            SummaryCardLayout.Centre(1000f, 600f, 400f, 200f, out var x, out var y);
            Assert.AreEqual(300f, x);
            Assert.AreEqual(200f, y);
        }
    }
}
