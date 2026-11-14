using UncannyValet.Core.Contracts;
using UncannyValet.Core.Ids;
using UncannyValet.Core.Story;
using UncannyValet.UI.Kit;
using UnityEngine;

namespace UncannyValet.UI.Flow
{
    /// <summary>
    /// Builds the card of a flow state from the story text, the lot and power facts and the run progress. Pure (no Unity scene, no drawing),
    /// built once when a screen starts. A missing text or fact is reported once with <see cref="Debug.LogError(object)"/> and replaced by the raw id,
    /// so a gap in the data never stops the game.
    /// </summary>
    public static class StoryCardBuilder
    {
        internal const int CardTop = 22;
        internal const string NamePlaceholder = "{0}";

        /// <summary>The card for the machine's current state; an empty card (prompt only) for a state without one.</summary>
        public static StoryCard Build(FlowMachine machine, StoryTextAsset text, IStoryFacts facts, IRunProgress progress)
        {
            var card = new StoryCard { Prompt = text.Generic.Prompt };
            var lot = machine.State == FlowState.LotIntro ? machine.IntroLot : machine.LoadedLot;
            SetAccent(card, text, lot.World);
            var layout = new StoryCardLayout(card, CardTop);
            switch (machine.State)
            {
                case FlowState.LotIntro: IntroCards.Fill(layout, card, text, facts, lot); break;
                case FlowState.Reward: RewardCards.Fill(layout, card, machine, text, facts, progress); break;
                case FlowState.SentBack: EndCards.FillSentBack(layout, card, text, facts, progress); break;
                case FlowState.EndCard: EndCards.FillEnd(layout, card, machine, text, facts, progress); break;
            }
            return card;
        }

        internal static string Fill(string template, string value) => template.Replace(NamePlaceholder, value);

        internal static string Upper(string text) => string.IsNullOrEmpty(text) ? string.Empty : text.ToUpperInvariant();

        private static void SetAccent(StoryCard card, StoryTextAsset text, World world)
        {
            card.Accent = PixelPalette.Gold;
            card.AccentSecond = PixelPalette.Grey;
            if (text.TryGetWorld(world, out var story))
            {
                card.Accent = story.Accent;
                card.AccentSecond = story.AccentSecond;
            }
            else Debug.LogError("The story text has no entry for world " + world + "; the card uses default colours.", text);
        }
    }
}
