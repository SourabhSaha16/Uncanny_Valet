using System;
using System.Collections.Generic;
using UncannyValet.Core.Events;
using UncannyValet.Core.Watchers;
using UncannyValet.UI.Screens;

namespace UncannyValet.UI.Speech
{
    /// <summary>
    /// Keeps the one speech bubble that is on screen: the request the speech director chose, its wrapped lines and when it ends. A new
    /// <see cref="SpeechShown"/> replaces the old one (the director never sends two at once).
    /// </summary>
    public sealed class SpeechBubbleTracker : IDisposable
    {
        private readonly IEventBus _bus;
        private readonly Func<float> _now;
        private readonly List<WordWrap.Span> _lines = new List<WordWrap.Span>(4);
        private SpeechRequest _request;
        private float _until = float.NegativeInfinity;

        public SpeechBubbleTracker(IEventBus bus, Func<float> now)
        {
            _bus = bus;
            _now = now;
            _bus.Subscribe<SpeechShown>(OnShown);
        }

        /// <summary>The wrapped text lines of the current bubble.</summary>
        public IReadOnlyList<WordWrap.Span> Lines => _lines;

        /// <summary>The bubble to draw now; false when none is showing.</summary>
        public bool TryGetCurrent(float now, out SpeechRequest request)
        {
            request = _request;
            return now < _until;
        }

        public void Dispose() => _bus.Unsubscribe<SpeechShown>(OnShown);

        private void OnShown(SpeechShown e)
        {
            _request = e.Request;
            _until = _now() + e.Seconds;
            _lines.Clear();
            WordWrap.Wrap(e.Request.Text, SpeechBubbleLayout.LineChars, _lines);
        }
    }
}
