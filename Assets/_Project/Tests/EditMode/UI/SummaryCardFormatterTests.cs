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

        [Test]
        public void FinalNight_NotPaidOff_ShowsDebtLeft()
        {
            var card = SummaryCardFormatter.Build(Summary(7, 7, 10, 2, 4, 350));
            Assert.AreEqual("Debt left: $350: you did not pay it off", card.Headline);
            Assert.AreEqual("New run", card.ButtonLabel);
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
