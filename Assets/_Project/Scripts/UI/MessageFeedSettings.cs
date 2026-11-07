namespace UncannyValet.UI
{
    /// <summary>Timings and cap for <see cref="MessageFeed"/>. All times in seconds.</summary>
    public readonly struct MessageFeedSettings
    {
        public readonly int MaxVisible;
        public readonly int DefaultMaxStrikes;
        public readonly float DoneSeconds;
        public readonly float FailedSeconds;
        public readonly float FadeSeconds;
        public readonly float FlashSeconds;

        public MessageFeedSettings(int maxVisible, int defaultMaxStrikes, float doneSeconds,
            float failedSeconds, float fadeSeconds, float flashSeconds)
        {
            MaxVisible = maxVisible < 1 ? 1 : maxVisible;
            DefaultMaxStrikes = defaultMaxStrikes < 1 ? 1 : defaultMaxStrikes;
            DoneSeconds = doneSeconds;
            FailedSeconds = failedSeconds;
            FadeSeconds = fadeSeconds < 0.01f ? 0.01f : fadeSeconds;
            FlashSeconds = flashSeconds < 0.01f ? 0.01f : flashSeconds;
        }
    }
}
