using UncannyValet.Core.Events;
using UncannyValet.Core.Watchers;
using UnityEngine;

namespace UncannyValet.Watchers
{
    /// <summary>One watcher's mouth: turns a line into a speech request at the watcher's spot.</summary>
    public sealed class WatcherVoice
    {
        private readonly IEventBus _bus;
        private readonly Vector2 _position;
        private readonly Transform _anchor;

        public WatcherVoice(IEventBus bus, string speaker, Vector2 position, Transform anchor)
        {
            _bus = bus;
            Speaker = speaker;
            _position = position;
            _anchor = anchor;
        }

        /// <summary>The name shown above this watcher's bubbles.</summary>
        public string Speaker { get; }

        /// <summary>Asks the speech director to show <paramref name="text"/> above the watcher.</summary>
        public void Say(string text, SpeechPriority priority)
            => _bus.Publish(new SpeechRequested(new SpeechRequest(Speaker, text, _position, _anchor, priority)));
    }
}
