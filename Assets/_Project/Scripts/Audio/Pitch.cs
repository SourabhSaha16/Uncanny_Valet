using System;

namespace UncannyValet.Audio
{
    /// <summary>Note-number to frequency conversion.</summary>
    public static class Pitch
    {
        private const float ConcertAHz = 440f;
        private const int ConcertAMidi = 69;
        private const double SemitonesPerOctave = 12.0;

        /// <summary>Frequency in Hz of a MIDI note number (69 = A4 = 440 Hz).</summary>
        public static float MidiToHz(float midi)
        {
            return ConcertAHz * (float)Math.Pow(2.0, (midi - ConcertAMidi) / SemitonesPerOctave);
        }
    }
}
