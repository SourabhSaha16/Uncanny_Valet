namespace UncannyValet.Core.Events
{
    // Night events, grouped in one file by domain.

    public readonly struct NightStarted : IGameEvent
    {
        public readonly int Night;
        public NightStarted(int night) { Night = night; }
    }

    /// <summary>Published once per game minute. MinuteOfNight counts from the start of the shift.</summary>
    public readonly struct NightTick : IGameEvent
    {
        public readonly int Night;
        public readonly float MinuteOfNight;
        public NightTick(int night, float minuteOfNight) { Night = night; MinuteOfNight = minuteOfNight; }
    }

    public readonly struct NightEnded : IGameEvent
    {
        public readonly int Night;
        public NightEnded(int night) { Night = night; }
    }
}
