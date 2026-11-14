using UncannyValet.Core.Contracts;
using UncannyValet.Core.Ids;
using UncannyValet.Core.Story;
using UncannyValet.UI.Kit;
using UnityEngine;

namespace UncannyValet.UI.Flow
{
    /// <summary>The intro card of a lot: the world, the TRAINING or CRAZY tag, the lot's name, the god or trainer, flavour lines and the goal.</summary>
    internal static class IntroCards
    {
        private const int WorldScale = 4;
        private const int TagScale = 2;
        private const int NameScale = 3;
        private const int NameScaleLong = 2;
        private const int FrameScale = 2;
        private const int FlavourScale = 1;
        private const int GoalScale = 2;
        private const int TagGap = 6;
        private const int NameGap = 10;
        private const int FrameGap = 8;
        private const int FlavourGap = 12;
        private const int GoalGap = 16;

        public static void Fill(StoryCardLayout layout, StoryCard card, StoryTextAsset text, IStoryFacts facts, LotId lot)
        {
            var generic = text.Generic;
            var hasFacts = facts.TryGetLot(lot, out var info);
            if (!hasFacts) Debug.LogError("No lot facts for " + lot + "; the intro card shows the id.", text);
            var worldName = text.TryGetWorld(lot.World, out var world) ? world.WorldName : lot.World.ToString();
            var lotName = StoryCardBuilder.Upper(hasFacts ? info.LotName : lot.ToString());
            layout.Add(StoryCardBuilder.Upper(worldName), WorldScale, card.Accent, 0);
            layout.Add(Tag(generic, lot.Kind), TagScale, TagColor(lot.Kind), TagGap);
            layout.Add(lotName, StoryCardLayout.FittingScale(lotName, NameScale, NameScaleLong), PixelPalette.Cream, NameGap);
            AddWatcher(layout, generic, lot.Kind, info.WatcherName, card);
            AddFlavour(layout, text, lot);
            layout.Add(StoryCardBuilder.Fill(generic.GoalLine, info.Threshold.ToString()), GoalScale, PixelPalette.Gold, GoalGap);
        }

        private static void AddWatcher(StoryCardLayout layout, StoryGenericText generic, LotKind kind, string name, StoryCard card)
        {
            if (string.IsNullOrEmpty(name)) return;
            var frame = kind == LotKind.Training ? generic.TrainerFrame : generic.GodFrame;
            layout.Add(StoryCardBuilder.Fill(frame, StoryCardBuilder.Upper(name)), FrameScale, card.Accent, FrameGap);
        }

        private static void AddFlavour(StoryCardLayout layout, StoryTextAsset text, LotId lot)
        {
            if (!text.TryGetLot(lot, out var story))
            {
                Debug.LogError("The story text has no intro lines for " + lot + ".", text);
                return;
            }
            for (var i = 0; i < story.IntroLines.Count; i++)
                layout.Add(story.IntroLines[i], FlavourScale, PixelPalette.Cream, i == 0 ? FlavourGap : 0);
        }

        private static string Tag(StoryGenericText generic, LotKind kind)
        {
            if (kind == LotKind.Training) return generic.TrainingTag;
            return kind == LotKind.Crazy ? generic.CrazyTag : generic.FinaleTag;
        }

        private static Color TagColor(LotKind kind)
        {
            if (kind == LotKind.Training) return PixelPalette.Green;
            return kind == LotKind.Crazy ? PixelPalette.OrangeRed : PixelPalette.Gold;
        }
    }
}
