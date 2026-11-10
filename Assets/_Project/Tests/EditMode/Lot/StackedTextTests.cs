using NUnit.Framework;
using UncannyValet.Lot;

namespace UncannyValet.Tests.Lot
{
    public class StackedTextTests
    {
        [Test]
        public void StackPutsOneLetterPerLine()
        {
            Assert.AreEqual("V\nA\nL\nE\nT", StackedText.Stack("VALET"));
            Assert.AreEqual("X", StackedText.Stack("X"));
            Assert.AreEqual(string.Empty, StackedText.Stack(null));
        }

        [Test]
        public void ColumnsAreCentredOnZero()
        {
            Assert.AreEqual(-0.5f, StackedText.ColumnOffset(0, 2, 1f), 0.0001f);
            Assert.AreEqual(0.5f, StackedText.ColumnOffset(1, 2, 1f), 0.0001f);
            Assert.AreEqual(0f, StackedText.ColumnOffset(0, 1, 3f), 0.0001f);
        }
    }
}
