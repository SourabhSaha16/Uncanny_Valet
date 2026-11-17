using UncannyValet.Core.Watchers;

namespace UncannyValet.Core.Events
{
    // Speech bubble events, grouped in one file by domain.

    /// <summary>
    /// Somebody (a watcher, a gag-line pedestrian) wants to say something. The speech director (Watchers module) decides what is
    /// shown: one bubble at a time, with lockouts, highest priority first; every other request is dropped.
    /// </summary>
    public readonly struct SpeechRequested : IGameEvent
    {
        public readonly SpeechRequest Request;
        public SpeechRequested(SpeechRequest request) { Request = request; }
    }

    /// <summary>The speech director chose this request: the UI draws its bubble for Seconds (real time).</summary>
    public readonly struct SpeechShown : IGameEvent
    {
        public readonly SpeechRequest Request;
        public readonly float Seconds;
        public SpeechShown(SpeechRequest request, float seconds) { Request = request; Seconds = seconds; }
    }
}
