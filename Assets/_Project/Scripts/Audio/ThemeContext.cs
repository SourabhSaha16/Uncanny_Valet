using System;
using UncannyValet.Core.Music;

namespace UncannyValet.Audio
{
    /// <summary>What a theme instrument needs to place its notes: the theme, the loop timing and a few lookups on the tune and chords.</summary>
    public readonly struct ThemeContext
    {
        /// <summary>Steps under one chord.</summary>
        public const int StepsPerChord = MusicThemeDefinition.Steps / MusicThemeDefinition.ChordCount;
        private const int MinorThird = 3;
        private const int MajorThird = 4;
        private const int Fifth = 7;

        public readonly MusicThemeDefinition Theme;
        public readonly LoopSettings Settings;

        public ThemeContext(MusicThemeDefinition theme, int sampleRate)
        {
            Theme = theme;
            Settings = new LoopSettings(sampleRate, theme.TempoBpm, theme.RootMidiNote, theme.Swing);
        }

        /// <summary>Seconds in one 16th step.</summary>
        public float StepSeconds => 60f / Settings.Bpm / LoopSettings.StepsPerBeat;

        /// <summary>First sample of a step, with the swing on off-beat 16ths.</summary>
        public int StepStart(int step)
        {
            double stepSamples = Settings.SamplesPerBeat / (double)LoopSettings.StepsPerBeat;
            double swung = step + (step % 2 == 1 ? Settings.Swing : 0f);
            return (int)Math.Round(swung * stepSamples);
        }

        /// <summary>True when the tune plays a note on this step.</summary>
        public bool HasNote(int step) => Theme.Melody[step] != MusicThemeDefinition.Rest;

        /// <summary>Frequency of the tune's note on a step, shifted by whole octaves.</summary>
        public float NoteHz(int step, int octaves) => Pitch.MidiToHz(Settings.RootMidi + Theme.Melody[step] + 12 * octaves);

        /// <summary>Steps the note on this step lasts: until the next note, or the end of the loop.</summary>
        public int NoteSteps(int step)
        {
            var length = 1;
            while (step + length < MusicThemeDefinition.Steps && !HasNote(step + length)) length++;
            return length;
        }

        /// <summary>Frequency of a chord tone (0 root, 1 third, 2 fifth) of the chord under a step, shifted by whole octaves.</summary>
        public float ChordHz(int step, int tone, int octaves)
        {
            int root = Theme.ChordRoots[Math.Min(step / StepsPerChord, MusicThemeDefinition.ChordCount - 1)];
            int offset = tone == 0 ? 0 : tone == 1 ? (Theme.Minor ? MinorThird : MajorThird) : Fifth;
            return Pitch.MidiToHz(Settings.RootMidi + root + offset + 12 * octaves);
        }

        /// <summary>True when the step starts a quarter-note beat.</summary>
        public static bool OnBeat(int step) => step % LoopSettings.StepsPerBeat == 0;
    }
}
