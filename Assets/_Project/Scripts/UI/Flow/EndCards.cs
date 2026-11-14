using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Core.Story;
using UncannyValet.UI.Kit;

namespace UncannyValet.UI.Flow
{
    /// <summary>The cards that close a stretch of the story: thrown back to Midgard, and the end of the run (TO BE CONTINUED or THOR IS HOME).</summary>
    internal static class EndCards
    {
        private const int HeadingScale = 4;
        private const int SentBackHeadingScale = 3;
        private const int LineScale = 2;
        private const int ListHeadingScale = 1;
        private const int PowerScale = 2;
        private const int LineGap = 6;
        private const int ListGap = 10;
        private const int PowerGap = 3;

        public static void FillSentBack(StoryCardLayout layout, StoryCard card, StoryTextAsset text, IStoryFacts facts, IRunProgress progress)
        {
            var generic = text.Generic;
            layout.Add(generic.SentBackHeading, SentBackHeadingScale, card.Accent, 0);
            for (var i = 0; i < generic.SentBackLines.Count; i++)
                layout.Add(generic.SentBackLines[i], LineScale, PixelPalette.Cream, i == 0 ? LineGap : 0);
            AddPowers(layout, card, generic.KeptHeading, text, facts, progress);
        }

        public static void FillEnd(StoryCardLayout layout, StoryCard card, FlowMachine machine, StoryTextAsset text, IStoryFacts facts, IRunProgress progress)
        {
            var generic = text.Generic;
            var home = machine.End == RunEnd.Finale;
            layout.Add(home ? generic.HomeHeading : generic.ContinuedHeading, HeadingScale, card.Accent, 0);
            var lines = home ? generic.HomeLines : generic.ContinuedLines;
            for (var i = 0; i < lines.Count; i++) layout.Add(lines[i], LineScale, PixelPalette.Cream, i == 0 ? LineGap : 0);
            if (home) layout.Add(StoryCardBuilder.Fill(generic.FinaleBlessings, progress.BestBlessings(machine.LoadedLot).ToString()), LineScale, PixelPalette.Gold, LineGap);
            AddPowers(layout, card, generic.EarnedHeading, text, facts, progress);
        }

        private static void AddPowers(StoryCardLayout layout, StoryCard card, string heading, StoryTextAsset text, IStoryFacts facts, IRunProgress progress)
        {
            layout.Add(heading, ListHeadingScale, card.Accent, ListGap);
            var earned = progress.EarnedPowers;
            if (earned.Count == 0) layout.Add(text.Generic.KeptNone, PowerScale, PixelPalette.Grey, PowerGap);
            for (var i = 0; i < earned.Count; i++)
                layout.Add(PowerLine(text, facts, progress, earned[i]), PowerScale, PixelPalette.Gold, PowerGap);
        }

        private static string PowerLine(StoryTextAsset text, IStoryFacts facts, IRunProgress progress, StoryPower power)
        {
            var name = StoryCardBuilder.Upper(facts.TryGetPower(power, out var info) ? info.Name : power.ToString());
            return progress.PowerLevel(power) > 1 ? name + " " + text.Generic.UpgradedMark : name;
        }
    }
}
