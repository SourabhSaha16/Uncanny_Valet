namespace UncannyValet.Core.Music
{
    /// <summary>The checks of a <see cref="MusicThemeDefinition"/>.</summary>
    public static class MusicThemeRules
    {
        /// <summary>Null when the theme is fine, otherwise what is wrong.</summary>
        public static string Check(MusicThemeDefinition theme)
        {
            if (theme == null) return "the music theme is missing";
            var name = theme.name;
            if (theme.TempoBpm < MusicThemeDefinition.MinTempo || theme.TempoBpm > MusicThemeDefinition.MaxTempo)
                return name + ": the tempo must be " + MusicThemeDefinition.MinTempo + " to " + MusicThemeDefinition.MaxTempo;
            if (theme.Melody == null || theme.Melody.Count != MusicThemeDefinition.Steps)
                return name + ": the melody needs exactly " + MusicThemeDefinition.Steps + " steps";
            if (theme.ChordRoots == null || theme.ChordRoots.Count != MusicThemeDefinition.ChordCount)
                return name + ": it needs exactly " + MusicThemeDefinition.ChordCount + " chords";
            if (theme.ClipOverride == null && theme.Voices.Count == 0) return name + ": no instruments and no clip override";
            if (theme.EscalationTempoRise < 0f) return name + ": the escalation tempo rise cannot be negative";
            for (var i = 0; i < theme.Voices.Count; i++)
            {
                if (theme.Voices[i].Volume < 0f || theme.Voices[i].Volume > 1f) return name + ": instrument " + i + " has a level outside 0 to 1";
                for (var j = 0; j < i; j++)
                    if (theme.Voices[j].Voice == theme.Voices[i].Voice) return name + ": " + theme.Voices[i].Voice + " is listed twice";
            }
            return null;
        }
    }
}
