using UncannyValet.Core.Contracts;
using UncannyValet.Core.Story;

namespace UncannyValet.UI.Flow
{
    /// <summary>Builds the card of the current screen once, when the screen starts, and hands the same card back until the screen changes.</summary>
    public sealed class StoryCardCache
    {
        private readonly FlowMachine _machine;
        private readonly StoryTextAsset _text;
        private readonly IStoryFacts _facts;
        private readonly IRunProgress _progress;
        private int _version = -1;
        private StoryCard _card;

        public StoryCardCache(FlowMachine machine, StoryTextAsset text, IStoryFacts facts, IRunProgress progress)
        {
            _machine = machine;
            _text = text;
            _facts = facts;
            _progress = progress;
        }

        /// <summary>The card of the machine's current state; null for a screen without one (title, how-to-play, HUD, results).</summary>
        public StoryCard Current
        {
            get
            {
                if (_machine.Version == _version) return _card;
                _version = _machine.Version;
                _card = HasCard(_machine.State) ? StoryCardBuilder.Build(_machine, _text, _facts, _progress) : null;
                return _card;
            }
        }

        private static bool HasCard(FlowState state) =>
            state == FlowState.LotIntro || state == FlowState.Reward || state == FlowState.SentBack || state == FlowState.EndCard;
    }
}
