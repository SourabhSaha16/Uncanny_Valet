using System;

namespace UncannyValet.Audio
{
    /// <summary>Everything the loop builder needs, copied from <see cref="AudioConfig"/> so the builder stays pure.</summary>
    public readonly struct LoopSettings
    {
        public const int BeatsPerBar = 4;
        public const int StepsPerBeat = 4;
        private const float SecondsPerMinute = 60f;

        public readonly int SampleRate;
        public readonly float Bpm;
        /// <summary>MIDI note number of the scale root (the bass plays from here).</summary>
        public readonly int RootMidi;
        /// <summary>Delay of every off-beat 16th as a fraction of a 16th (0 = straight).</summary>
        public readonly float Swing;

        public LoopSettings(int sampleRate, float bpm, int rootMidi, float swing)
        {
            SampleRate = sampleRate; Bpm = bpm; RootMidi = rootMidi; Swing = swing;
        }

        public int SamplesPerBeat => (int)Math.Round(SampleRate * SecondsPerMinute / Bpm);

        public int TotalBeats => FunkPattern.Bars * BeatsPerBar;

        /// <summary>Whole number of samples, so the loop wraps without a click.</summary>
        public int LengthSamples => TotalBeats * SamplesPerBeat;

        public int TotalSteps => TotalBeats * StepsPerBeat;
    }
}
