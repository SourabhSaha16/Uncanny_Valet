using System;
using UncannyValet.Core.Music;

namespace UncannyValet.Audio
{
    /// <summary>Finds the synth for each <see cref="ThemeVoice"/>. The parts keep no state, so one shared instance each is fine.</summary>
    public static class ThemeParts
    {
        private static readonly IThemePart Guitar = new GuitarPart();
        private static readonly IThemePart Shaker = new ShakerPart();
        private static readonly IThemePart Fiddle = new FiddlePart();
        private static readonly IThemePart Flute = new FlutePart();
        private static readonly IThemePart FrameDrum = new FrameDrumPart();
        private static readonly IThemePart Organ = new OrganPart();
        private static readonly IThemePart Brass = new BrassPart();
        private static readonly IThemePart Stomp = new StompPart();
        private static readonly IThemePart Clap = new ClapPart();

        /// <summary>The instrument's synth.</summary>
        public static IThemePart For(ThemeVoice voice)
        {
            switch (voice)
            {
                case ThemeVoice.FolkGuitar: return Guitar;
                case ThemeVoice.Shaker: return Shaker;
                case ThemeVoice.FiddleSaw: return Fiddle;
                case ThemeVoice.WoodenFlute: return Flute;
                case ThemeVoice.FrameDrum: return FrameDrum;
                case ThemeVoice.FairOrgan: return Organ;
                case ThemeVoice.Brass: return Brass;
                case ThemeVoice.Stomp: return Stomp;
                case ThemeVoice.Clap: return Clap;
                default: throw new ArgumentOutOfRangeException(nameof(voice), voice, "No synth for this instrument.");
            }
        }
    }
}
