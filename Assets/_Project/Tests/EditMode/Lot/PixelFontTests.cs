using NUnit.Framework;
using UncannyValet.Lot;

namespace UncannyValet.Tests.Lot
{
    public class PixelFontTests
    {
        private const int LetterWidth = 5;

        [TestCase("ENTRY", 5)]
        [TestCase("EXIT", 4)]
        public void AWordIsOneRowSetWithTheLettersSideBySideAndOneClearColumnBetween(string word, int letters)
        {
            var rows = PixelFont.Compose(word);
            Assert.AreEqual(PixelFont.Height, rows.Length);
            var width = letters * LetterWidth + (letters - 1) * PixelFont.Spacing;
            foreach (var row in rows) Assert.AreEqual(width, row.Length);
        }

        [Test]
        public void EveryLetterTheRoadLabelsNeedExists()
        {
            foreach (var letter in "ENTRYXI") Assert.IsTrue(PixelFont.Has(letter), letter.ToString());
        }

        [Test]
        public void ALetterTheFontLacksIsSkipped()
            => CollectionAssert.AreEqual(PixelFont.Compose("E"), PixelFont.Compose("E?"));

        [Test]
        public void TheTopRowOfEIsFullyInked()
            => Assert.AreEqual(new string(PixelFont.Ink, LetterWidth), PixelFont.Compose("E")[0]);
    }
}
