using System.Text;
using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.UI;

namespace UncannyValet.Tests.UI
{
    public class ResultsFormatterTests
    {
        private sealed class FakeScore : IShiftScoreView
        {
            public int Score { get; set; }
            public int ComboMultiplier { get; set; }
            public int Complaints { get; set; }
            public int Stars { get; set; }
            public int OneStarScore => 150;
            public int TwoStarScore => 300;
            public int ThreeStarScore => 450;
        }

        private static ShiftResult Result(int score, int stars, int complaints = 0)
            => new ShiftResult(score, stars, complaints, 150, 300, 450);

        [TestCase(0, "---")]
        [TestCase(1, "*--")]
        [TestCase(2, "**-")]
        [TestCase(3, "***")]
        public void StarText_OneCharacterPerPossibleStar(int stars, string expected)
            => Assert.AreEqual(expected, ResultsFormatter.StarText(stars));

        [Test]
        public void Build_ShowsStarsScoreComplaintsAndTheRetryButton()
        {
            var content = ResultsFormatter.Build(Result(320, 2, 1));
            Assert.AreEqual("**-", content.Stars);
            Assert.AreEqual(ResultsFormatter.RetryLabel, content.ButtonLabel);
            Assert.AreEqual("Score: $320", content.Lines[0]);
            Assert.AreEqual("Complaints: 1", content.Lines[1]);
        }

        [Test]
        public void Build_NamesTheNextStarTarget()
        {
            Assert.AreEqual("Next star at $150 (50 to go)", ResultsFormatter.Build(Result(100, 0)).Lines[2]);
            Assert.AreEqual("Next star at $450 (130 to go)", ResultsFormatter.Build(Result(320, 2)).Lines[2]);
        }

        [Test]
        public void Build_ThreeStars_HasNoNextTarget()
            => StringAssert.Contains("Three stars", ResultsFormatter.Build(Result(500, 3)).Lines[2]);

        [Test]
        public void ScoreLines_ShowScoreComboComplaintsStarsAndThresholds()
        {
            var text = new StringBuilder();
            ScoreLines.Append(text, new FakeScore { Score = 120, ComboMultiplier = 3, Complaints = 2, Stars = 0 });
            var lines = text.ToString().Split('\n');
            StringAssert.Contains("Score: $120", lines[0]);
            StringAssert.Contains("Combo: x3", lines[0]);
            StringAssert.Contains("Complaints: 2", lines[0]);
            StringAssert.Contains("Stars: 0", lines[1]);
            StringAssert.Contains("$150", lines[1]);
            StringAssert.Contains("$300", lines[1]);
            StringAssert.Contains("$450", lines[1]);
        }
    }
}
