using UncannyValet.Core.Contracts;
using UncannyValet.Core.Ids;
using UncannyValet.Core.Story;
using UncannyValet.UI.Kit;
using UnityEngine;

namespace UncannyValet.UI.Flow
{
    /// <summary>The reward card of a power or upgrade: the icon, the name, what it does, the key to press and the farewell of the world's god.</summary>
    internal static class RewardCards
    {
        private const int HeadingScale = 3;
        private const int NameScale = 3;
        private const int DescriptionScale = 2;
        private const int KeyScale = 2;
        private const int FarewellScale = 1;
        private const int IconGap = 6;
        private const int NameGap = 6;
        private const int DescriptionGap = 6;
        private const int KeyGap = 6;
        private const int FarewellGap = 8;
        private const int AttributionGap = 2;
        private const string Quote = "'";
        private const string Dash = "- ";

        public static void Fill(StoryCardLayout layout, StoryCard card, FlowMachine machine, StoryTextAsset text, IStoryFacts facts, IRunProgress progress)
        {
            var reward = machine.Reward;
            var generic = text.Generic;
            var hasStory = text.TryGetPower(reward.Power, out var story);
            if (!hasStory) Debug.LogError("The story text has no reward lines for " + reward.Power + ".", text);
            var hasFacts = facts.TryGetPower(reward.Power, out var power);
            if (!hasFacts) Debug.LogError("No power facts for " + reward.Power + "; the reward card shows the id.", text);
            layout.Add(reward.Level > 1 ? generic.UpgradeHeading : generic.NewPowerHeading, HeadingScale, card.Accent, 0);
            layout.AddIcon(reward.Power, IconGap);
            layout.Add(StoryCardBuilder.Upper(hasFacts ? power.Name : reward.Power.ToString()), NameScale, PixelPalette.Cream, NameGap);
            var description = hasStory && !string.IsNullOrEmpty(story.Description) ? story.Description : power.Description;
            layout.Add(description, DescriptionScale, PixelPalette.Cream, DescriptionGap);
            layout.Add(StoryCardBuilder.Fill(generic.KeyHint, KeyLabel(progress, reward.Power)), KeyScale, PixelPalette.Gold, KeyGap);
            if (hasStory) AddFarewell(layout, card, story.Farewell, GodName(facts, machine.LoadedLot));
        }

        /// <summary>The key that fires the power: its place in the earn order (key 1 to 4); "?" when it is not on the power bar.</summary>
        public static string KeyLabel(IRunProgress progress, StoryPower power)
        {
            var earned = progress.EarnedPowers;
            for (var i = 0; i < earned.Count; i++)
                if (earned[i] == power) return (i + 1).ToString();
            return "?";
        }

        private static string GodName(IStoryFacts facts, LotId lot) =>
            facts.TryGetLot(lot, out var info) ? StoryCardBuilder.Upper(info.WatcherName) : string.Empty;

        private static void AddFarewell(StoryCardLayout layout, StoryCard card, string farewell, string god)
        {
            layout.Add(Quote + farewell + Quote, FarewellScale, PixelPalette.Cream, FarewellGap);
            if (!string.IsNullOrEmpty(god)) layout.Add(Dash + god, FarewellScale, card.Accent, AttributionGap);
        }
    }
}
