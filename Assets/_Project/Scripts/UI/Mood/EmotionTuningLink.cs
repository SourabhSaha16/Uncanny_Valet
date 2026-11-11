using System;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.UI.Mood
{
    /// <summary>Feeds the F2 emotion levels (angry, will not tip) into an <see cref="EmotionRule"/>, now and whenever they change.</summary>
    public sealed class EmotionTuningLink : IDisposable
    {
        private readonly IEventBus _bus;
        private readonly ITuning _tuning;
        private readonly EmotionRule _rule;
        private readonly float _angryFallback;
        private readonly float _willNotTipFallback;

        /// <param name="angryFallback">Level used while the key is unseeded.</param>
        /// <param name="willNotTipFallback">Level used while the key is unseeded.</param>
        public EmotionTuningLink(IEventBus bus, ITuning tuning, EmotionRule rule, float angryFallback, float willNotTipFallback)
        {
            _bus = bus;
            _tuning = tuning;
            _rule = rule;
            _angryFallback = angryFallback;
            _willNotTipFallback = willNotTipFallback;
            Apply();
            _bus.Subscribe<TuningChanged>(OnChanged);
        }

        /// <summary>Stops following the tuning.</summary>
        public void Dispose() => _bus.Unsubscribe<TuningChanged>(OnChanged);

        private void OnChanged(TuningChanged e)
        {
            if (e.Key == TuningKey.EmotionAngryFrom || e.Key == TuningKey.EmotionWillNotTipFrom) Apply();
        }

        private void Apply()
        {
            if (!_tuning.TryGet(TuningKey.EmotionAngryFrom, out var angry)) angry = _angryFallback;
            if (!_tuning.TryGet(TuningKey.EmotionWillNotTipFrom, out var willNotTip)) willNotTip = _willNotTipFallback;
            _rule.SetLevels(angry, willNotTip);
        }
    }
}
