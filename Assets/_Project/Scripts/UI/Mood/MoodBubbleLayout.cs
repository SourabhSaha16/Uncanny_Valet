namespace UncannyValet.UI.Mood
{
    /// <summary>Pure shape of the chat bubble over a waiting patron: a coloured box with a tail and a tiny face ('#' = dark pixel).</summary>
    public static class MoodBubbleLayout
    {
        public const int FaceWidth = 9;
        public const int FaceHeight = 7;
        /// <summary>Border plus one pixel of padding on every side of the face.</summary>
        public const int Frame = 2;
        /// <summary>Rows the tail adds under the box.</summary>
        public const int TailHeight = 2;
        /// <summary>Width of the tail's first row.</summary>
        public const int TailWidth = 3;

        private static readonly string[] HappyFace =
        {
            ".........", "..#...#..", "..#...#..", ".........", ".#.....#.", "..#####..", "........."
        };

        private static readonly string[] ImpatientFace =
        {
            ".........", "..#...#..", "..#...#..", ".........", ".........", "..#####..", "........."
        };

        private static readonly string[] AngryFace =
        {
            "##.....##", ".##...##.", "..#...#..", ".........", "..#####..", ".#.....#.", "........."
        };

        public static int Width => FaceWidth + 2 * Frame;

        /// <summary>Box height without the tail.</summary>
        public static int BoxHeight => FaceHeight + 2 * Frame;

        /// <summary>The face rows of an emotion, <see cref="FaceHeight"/> strings of <see cref="FaceWidth"/> characters.</summary>
        public static string[] Face(Emotion emotion)
        {
            if (emotion == Emotion.Angry) return AngryFace;
            return emotion == Emotion.Impatient ? ImpatientFace : HappyFace;
        }
    }
}
