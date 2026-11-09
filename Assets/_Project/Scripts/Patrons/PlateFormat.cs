using System;

namespace UncannyValet.Patrons
{
    /// <summary>
    /// The shape of a licence plate: three letters, a space, three digits (for example "BKT 482"). Letters come from a
    /// consonant-only alphabet without I, O and Q, so a plate can never spell a word or be mistaken for a digit.
    /// Pure and allocation-free apart from the returned string.
    /// </summary>
    public static class PlateFormat
    {
        /// <summary>The letters a plate may use.</summary>
        public const string Letters = "BCDFGHJKLMNPRSTVWXZ";

        /// <summary>Number of letters in <see cref="Letters"/>.</summary>
        public const int LetterCount = 19;

        /// <summary>Distinct numbers 000 to 999.</summary>
        public const int NumberCount = 1000;

        /// <summary>Total number of distinct plates.</summary>
        public const int Total = LetterCount * LetterCount * LetterCount * NumberCount;

        /// <summary>Characters in a plate including the space.</summary>
        public const int Length = 7;

        private const int Radix = 10;

        /// <summary>The plate at an index; the index wraps into 0..<see cref="Total"/>-1.</summary>
        public static string FromIndex(int index)
        {
            var wrapped = ((index % Total) + Total) % Total;
            var number = wrapped % NumberCount;
            var letters = wrapped / NumberCount;
            var chars = new char[Length];
            for (var i = 2; i >= 0; i--)
            {
                chars[i] = Letters[letters % LetterCount];
                letters /= LetterCount;
            }
            chars[3] = ' ';
            chars[4] = (char)('0' + number / (Radix * Radix));
            chars[5] = (char)('0' + number / Radix % Radix);
            chars[6] = (char)('0' + number % Radix);
            return new string(chars);
        }

        /// <summary>True for a string of the plate shape: three allowed letters, a space, three digits.</summary>
        public static bool IsValid(string plate)
        {
            if (plate == null || plate.Length != Length || plate[3] != ' ') return false;
            for (var i = 0; i < 3; i++)
                if (Letters.IndexOf(plate[i]) < 0) return false;
            for (var i = 4; i < Length; i++)
                if (plate[i] < '0' || plate[i] > '9') return false;
            return true;
        }
    }
}
