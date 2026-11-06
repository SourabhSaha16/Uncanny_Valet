namespace UncannyValet.Audio
{
    /// <summary>Mute flag for all game audio.</summary>
    public sealed class MuteState
    {
        public bool Muted { get; private set; }

        public void Toggle() => Muted = !Muted;

        /// <summary>The volume to actually use: zero while muted.</summary>
        public float Apply(float volume) => Muted ? 0f : volume;
    }
}
