using NUnit.Framework;
using UncannyValet.Cars;

namespace UncannyValet.Tests.Cars
{
    public class CarLabelMathTests
    {
        [Test]
        public void TargetHeightIsFractionOfWidth()
        {
            Assert.AreEqual(0.99f, CarLabelMath.TargetHeight(1.8f, 0.55f), 1e-4f);
        }

        [Test]
        public void CharacterSizeRoundTripsToTargetHeight()
        {
            float size = CarLabelMath.CharacterSizeFor(0.99f);
            Assert.AreEqual(0.99f, size * CarLabelMath.DigitHeightPerCharacterSize, 1e-3f);
            Assert.Less(size, 0.5f);
        }

        [Test]
        public void LetterTextShowsUppercaseLetter()
        {
            Assert.AreEqual("A", CarLabelMath.LetterText('A'));
            Assert.AreEqual("B", CarLabelMath.LetterText('b'));
        }

        [Test]
        public void LetterTextIsEmptyUntilAssigned()
        {
            Assert.AreEqual(string.Empty, CarLabelMath.LetterText('\0'));
            Assert.AreEqual(string.Empty, CarLabelMath.LetterText(' '));
        }

        [Test]
        public void CharacterSizeNeverZero()
        {
            Assert.Greater(CarLabelMath.CharacterSizeFor(0f), 0f);
        }
    }
}
