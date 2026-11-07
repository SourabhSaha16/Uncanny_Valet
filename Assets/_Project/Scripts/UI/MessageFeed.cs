using System;
using System.Collections.Generic;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;

namespace UncannyValet.UI
{
    /// <summary>
    /// The boss message list and strike count, built from OrderIssued, OrderResolved, StrikeGiven and
    /// NightStarted only. Time comes from an injected source so tests control it. Strikes last the whole run;
    /// night 1 starting means a new run.
    /// </summary>
    public sealed class MessageFeed : IDisposable
    {
        private readonly IEventBus _bus;
        private readonly Func<float> _now;
        private readonly MessageFeedSettings _settings;
        private readonly List<FeedMessage> _messages = new List<FeedMessage>();

        public int Count => _messages.Count;
        public FeedMessage this[int index] => _messages[index];
        public int Strikes { get; private set; }
        public int MaxStrikes { get; private set; }
        /// <summary>True once the final strike landed, until the next run starts.</summary>
        public bool RunLost { get; private set; }

        public MessageFeed(IEventBus bus, Func<float> now, MessageFeedSettings settings)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _now = now ?? throw new ArgumentNullException(nameof(now));
            _settings = settings;
            MaxStrikes = settings.DefaultMaxStrikes;
            _bus.Subscribe<OrderIssued>(OnIssued);
            _bus.Subscribe<OrderResolved>(OnResolved);
            _bus.Subscribe<StrikeGiven>(OnStrike);
            _bus.Subscribe<NightStarted>(OnNightStarted);
        }

        public void Dispose()
        {
            _bus.Unsubscribe<OrderIssued>(OnIssued);
            _bus.Unsubscribe<OrderResolved>(OnResolved);
            _bus.Unsubscribe<StrikeGiven>(OnStrike);
            _bus.Unsubscribe<NightStarted>(OnNightStarted);
        }

        /// <summary>Drops messages whose hold and fade are over. Call once per frame; allocation-free.</summary>
        public void Tick()
        {
            var now = _now();
            for (var i = _messages.Count - 1; i >= 0; i--)
            {
                var m = _messages[i];
                if (m.Status != MessageStatus.Open && now - m.ClosedAt >= Hold(m.Status) + _settings.FadeSeconds)
                    _messages.RemoveAt(i);
            }
        }

        /// <summary>0..1 opacity: 1 while open or held, then fading to 0.</summary>
        public float Alpha(FeedMessage m)
        {
            if (m.Status == MessageStatus.Open) return 1f;
            var fade = (_now() - m.ClosedAt - Hold(m.Status)) / _settings.FadeSeconds;
            return fade <= 0f ? 1f : fade >= 1f ? 0f : 1f - fade;
        }

        /// <summary>1 right after arrival, falling to 0 over the flash time.</summary>
        public float Flash(FeedMessage m)
        {
            var left = 1f - (_now() - m.IssuedAt) / _settings.FlashSeconds;
            return left <= 0f ? 0f : left >= 1f ? 1f : left;
        }

        private float Hold(MessageStatus status)
        {
            switch (status)
            {
                case MessageStatus.Done: return _settings.DoneSeconds;
                case MessageStatus.Failed: return _settings.FailedSeconds;
                default: return 0f;
            }
        }

        private void OnIssued(OrderIssued e)
        {
            while (_messages.Count >= _settings.MaxVisible) _messages.RemoveAt(0);
            _messages.Add(new FeedMessage(e.Order, MessageFormatter.Order(e.Letter, e.TargetSpot), _now()));
        }

        private void OnResolved(OrderResolved e)
        {
            var m = Find(e.Order);
            if (m == null || m.Status != MessageStatus.Open) return;
            m.Status = e.Outcome == OrderOutcome.Passed ? MessageStatus.Done
                : e.Outcome == OrderOutcome.Failed ? MessageStatus.Failed : MessageStatus.Cancelled;
            m.ClosedAt = _now();
        }

        private void OnStrike(StrikeGiven e)
        {
            Strikes = e.Total;
            MaxStrikes = e.Max;
            if (e.IsFinal) RunLost = true;
        }

        private void OnNightStarted(NightStarted e)
        {
            _messages.Clear();
            if (e.Night > 1) return;
            Strikes = 0;
            RunLost = false;
        }

        private FeedMessage Find(OrderId id)
        {
            for (var i = 0; i < _messages.Count; i++) if (_messages[i].Order == id) return _messages[i];
            return null;
        }
    }
}
