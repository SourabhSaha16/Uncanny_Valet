namespace UncannyValet.Core.Music
{
    /// <summary>An instrument a music theme can use; Audio holds the synth for each. Stored as integers in assets: append-only.</summary>
    public enum ThemeVoice
    {
        /// <summary>Plucked folk guitar: the tune in single notes and a soft strum.</summary>
        FolkGuitar = 0,
        /// <summary>Shaker on the eighth notes.</summary>
        Shaker = 1,
        /// <summary>Fiddle-like bowed saw carrying the tune.</summary>
        FiddleSaw = 2,
        /// <summary>Wooden flute doubling the tune an octave up in the second bar.</summary>
        WoodenFlute = 3,
        /// <summary>Soft frame drum on the beats.</summary>
        FrameDrum = 4,
        /// <summary>Fairground organ: the tune with oom-pah chords.</summary>
        FairOrgan = 5,
        /// <summary>Brass stabs on the chord changes.</summary>
        Brass = 6,
        /// <summary>Boot stomps on the beats.</summary>
        Stomp = 7,
        /// <summary>Hand claps on the backbeat.</summary>
        Clap = 8,
    }
}
