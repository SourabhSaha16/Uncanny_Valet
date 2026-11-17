namespace UncannyValet.Audio
{
    /// <summary>One instrument of a music theme: adds its notes for the whole loop into the buffer.</summary>
    public interface IThemePart
    {
        /// <summary>Renders the instrument at the given level (0..1).</summary>
        void Render(MixBuffer buffer, in ThemeContext context, float volume);
    }
}
