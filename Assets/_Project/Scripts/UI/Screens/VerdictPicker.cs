using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Core.Story;

namespace UncannyValet.UI.Screens
{
    /// <summary>Chooses the <see cref="VerdictSpeech"/> of a finished shift from the story text. Pure.</summary>
    public static class VerdictPicker
    {
        /// <summary>The speech for the lot and its result; <c>default</c> (no panel, no line) when the text has no entry for the lot.</summary>
        public static VerdictSpeech Pick(StoryTextAsset text, LotId lot, in ShiftResult result)
        {
            if (text == null) return default;
            var tier = VerdictRule.Tier(lot.Kind, result.Blessings, result.Threshold, text.ClearedWellMultiple);
            if (lot.Kind == LotKind.Training)
                return text.TryGetTrainer(lot.World, out var trainer) ? new VerdictSpeech(string.Empty, trainer.Line(tier)) : default;
            return text.TryGetVerdict(lot, out var verdict) ? new VerdictSpeech(verdict.Speaker, verdict.Line(tier)) : default;
        }
    }
}
