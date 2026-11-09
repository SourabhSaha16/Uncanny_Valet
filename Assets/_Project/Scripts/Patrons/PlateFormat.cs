namespace UncannyValet.Patrons
{
    /// <summary>
    /// The shape of a licence plate: two letters, a space, one digit (for example "KT 4"). Letters come from a
    /// consonant-only alphabet without I, O and Q, so a plate can never spell a word or be mistaken for a digit.
    /// Pure and allocation-free apart from the returned string.
    /// </summary>
    public static class PlateFormat
    {
        /// <summary>The letters a plate may use.</summary>
        public const string Letters = "BCDFGHJKLMNPRSTVWXZ";

        /// <summary>Number of letters in <see cref="Letters"/>.</summary>
        public const int LetterCount = 19;

        /// <summary>Letters on a plate.</summary>
        public const int LetterSlots = 2;

        /// <summary>Distinct digits 0 to 9.</summary>
        public const int NumberCount = 10;

        /// <summary>Total number of distinct plates (19 x 19 x 10 = 3610).</summary>
        public const int Total = LetterCount * LetterCount * NumberCount;

        /// <summary>Characters in a plate including the space.</summary>
        public const int Length = 4;

        /// <summary>The plate at an index; the index wraps into 0..<see cref="Total"/>-1.</summary>
        public static string FromIndex(int index)
        {
            var wrapped = ((index % Total) + Total) % Total;
            var letters = wrapped / NumberCount;
            var chars = new char[Length];
            chars[0] = Letters[letters / LetterCount];
            chars[1] = Letters[letters % LetterCount];
            chars[2] = ' ';
            chars[3] = (char)('0' + wrapped % NumberCount);
            return new string(chars);
        }

        /// <summary>True for a string of the plate shape: two allowed letters, a space, one digit.</summary>
        public static bool IsValid(string plate)
        {
            if (plate == null || plate.Length != Length || plate[LetterSlots] != ' ') return false;
            for (var i = 0; i < LetterSlots; i++)
                if (Letters.IndexOf(plate[i]) < 0) return false;
            return plate[Length - 1] >= '0' && plate[Length - 1] <= '9';
        }
    }
}
