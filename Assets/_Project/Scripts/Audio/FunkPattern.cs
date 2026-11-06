namespace UncannyValet.Audio
{
    /// <summary>The funk groove as data: two bars on a 16th grid (32 steps), plus the level of each part.</summary>
    public static class FunkPattern
    {
        public const int Bars = 2;
        public const int Steps = 32;
        public const int Rest = int.MinValue;

        // Bass: semitones above the root (minor pentatonic with a dorian sixth), Rest = silent step.
        public static readonly int[] Bass =
        {
            0, Rest, Rest, 0, Rest, Rest, 12, Rest, 7, Rest, 10, Rest, 5, Rest, 3, Rest,
            0, Rest, 12, 10, Rest, Rest, 7, Rest, 0, Rest, Rest, 3, 5, Rest, 9, 7
        };

        public static readonly bool[] Kick = Steps32(0, 6, 10, 16, 19, 26);
        public static readonly bool[] Snare = Steps32(4, 12, 20, 28);
        public static readonly bool[] StabSteps = Steps32(6, 14, 22, 27);

        /// <summary>Chord stab voicing: a minor 9th, semitones above the root, one octave plus a fifth up.</summary>
        public static readonly int[] Chord = { 0, 3, 7, 10, 14 };

        public const int ChordOctaveShift = 24;

        public const float KickGain = 0.9f;
        public const float SnareGain = 0.5f;
        public const float SnareGhostGain = 0.12f;
        public const int SnareGhostStep = 31;
        public const float HatGain = 0.1f;
        public const float HatAccentGain = 0.18f;
        public const float BassGain = 0.55f;
        public const float BassSeconds = 0.11f;
        public const float StabGain = 0.07f;
        public const float StabSeconds = 0.1f;
        public const float NormalizedPeak = 0.9f;

        private static bool[] Steps32(params int[] on)
        {
            var steps = new bool[Steps];
            foreach (int i in on) steps[i] = true;
            return steps;
        }
    }
}
