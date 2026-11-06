using UnityEngine;

namespace UncannyValet.Cars
{
    /// <summary>Pure size maths for the car number label.</summary>
    public static class CarLabelMath
    {
        /// <summary>TextMesh fontSize used by the label.</summary>
        public const int FontSize = 100;

        /// <summary>
        /// World height of one digit per 1.0 of characterSize at <see cref="FontSize"/>.
        /// Calibrated from the Web build: characterSize 1.08 gave a digit about 4 units tall.
        /// </summary>
        public const float DigitHeightPerCharacterSize = 3.7f;

        /// <summary>characterSize that makes a digit <paramref name="targetHeight"/> world units tall.</summary>
        public static float CharacterSizeFor(float targetHeight)
        {
            return Mathf.Max(0.01f, targetHeight / DigitHeightPerCharacterSize);
        }

        /// <summary>Text for an assigned letter; empty until a real letter is assigned.</summary>
        public static string LetterText(char letter)
        {
            return char.IsLetter(letter) ? char.ToUpperInvariant(letter).ToString() : string.Empty;
        }

        /// <summary>Target digit height: a fraction of the car's width.</summary>
        public static float TargetHeight(float carWidth, float heightFraction)
        {
            return Mathf.Max(0f, carWidth) * Mathf.Clamp01(heightFraction);
        }
    }
}
