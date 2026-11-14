using System.Collections.Generic;
using UncannyValet.Core.Ids;
using UncannyValet.Core.Watchers;

namespace UncannyValet.Core.Story
{
    /// <summary>Checks a <see cref="StoryTextAsset"/> against the lots and powers it must cover, and every line against the pixel font. Pure.</summary>
    public static class StoryTextRules
    {
        private const string Placeholder = "{0}";
        /// <summary>Longest wrapped text (a description or a farewell), in characters.</summary>
        public const int WrappedLength = 90;

        /// <summary>Null when the text is complete and valid, otherwise the first fault.</summary>
        public static string Check(StoryTextAsset text, IReadOnlyList<LotId> lots, IReadOnlyList<StoryPower> powers)
        {
            for (var i = 0; i < lots.Count; i++)
            {
                var problem = CheckLot(text, lots[i]);
                if (problem != null) return problem;
            }
            for (var i = 0; i < powers.Count; i++)
            {
                var problem = CheckPower(text, powers[i]);
                if (problem != null) return problem;
            }
            foreach (var phrase in text.Generic.AllPhrases())
            {
                var problem = CheckLine(phrase, "generic text", WrappedLength);
                if (problem != null) return problem;
            }
            return null;
        }

        private static string CheckLot(StoryTextAsset text, LotId lot)
        {
            if (!text.TryGetWorld(lot.World, out var world) || string.IsNullOrWhiteSpace(world.WorldName)) return "no world name for " + lot.World;
            var problem = CheckLine(world.WorldName, "world " + lot.World, StoryTextAsset.MaxLineLength);
            if (problem != null) return problem;
            if (!text.TryGetLot(lot, out var story) || story.IntroLines.Count == 0) return "no intro lines for " + lot;
            for (var i = 0; i < story.IntroLines.Count && problem == null; i++) problem = CheckLine(story.IntroLines[i], "intro of " + lot, StoryTextAsset.MaxLineLength);
            return problem;
        }

        private static string CheckPower(StoryTextAsset text, StoryPower power)
        {
            if (!text.TryGetPower(power, out var story)) return "no reward lines for " + power;
            if (string.IsNullOrWhiteSpace(story.Farewell)) return "no farewell line for " + power;
            return CheckLine(story.Farewell, "farewell of " + power, WrappedLength) ?? (string.IsNullOrEmpty(story.Description) ? null : CheckLine(story.Description, "description of " + power, WrappedLength));
        }

        private static string CheckLine(string line, string where, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(line)) return "empty line in " + where;
            var shown = line.Replace(Placeholder, string.Empty);
            if (line.Length > maxLength) return where + ": line over " + maxLength + " characters: " + line;
            for (var i = 0; i < shown.Length; i++)
                if (GagLineAsset.AllowedCharacters.IndexOf(shown[i]) < 0) return where + ": character '" + shown[i] + "' has no glyph in: " + line;
            return null;
        }
    }
}
