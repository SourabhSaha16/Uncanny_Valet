using System.Collections.Generic;

namespace UncannyValet.Core.Watchers
{
    /// <summary>The validation rules of a <see cref="WatcherDefinition"/> and its text. Pure, so the tests and the setup tool share them.</summary>
    public static class WatcherRules
    {
        /// <summary>Null when the watcher is usable, otherwise the first fault.</summary>
        public static string Check(WatcherDefinition watcher, int skins, int hairs, int outfits)
        {
            if (watcher == null) return "the watcher is missing";
            var name = watcher.name;
            if (watcher.World == Ids.World.None) return name + ": no world";
            if (watcher.Kind == WatcherKind.None) return name + ": no kind (god or trainer)";
            if (string.IsNullOrEmpty(watcher.SpeakerName)) return name + ": no speaker name";
            if (watcher.Look == null) return name + ": no look";
            if (!watcher.Look.TryValidate(skins, hairs, outfits, out var lookProblem)) return name + ": " + lookProblem;
            return watcher.Text == null ? name + ": no text asset" : CheckText(name, watcher.Text, watcher.Kind == WatcherKind.God);
        }

        /// <summary>Null when every list has lines and every line is short; a god also needs all three verdict lists.</summary>
        public static string CheckText(string owner, WatcherTextAsset text, bool god)
        {
            if (text.Tips.Count == 0) return owner + ": no tips";
            for (var i = 0; i < text.Tips.Count; i++)
            {
                var tip = text.Tips[i];
                if (tip == null || !IsGoodLine(tip.Text)) return owner + ": tip " + i + " is empty or longer than " + WatcherTextAsset.MaxLineLength + " characters";
            }
            foreach (WatcherEvent e in System.Enum.GetValues(typeof(WatcherEvent)))
            {
                var problem = CheckList(owner + " " + e, text.Lines(e));
                if (problem != null) return problem;
            }
            if (!god) return null;
            foreach (WatcherVerdict v in System.Enum.GetValues(typeof(WatcherVerdict)))
            {
                var problem = CheckList(owner + " verdict " + v, text.Verdict(v));
                if (problem != null) return problem;
            }
            return null;
        }

        /// <summary>True for a non-empty line of at most <see cref="WatcherTextAsset.MaxLineLength"/> characters.</summary>
        public static bool IsGoodLine(string line) => !string.IsNullOrWhiteSpace(line) && line.Length <= WatcherTextAsset.MaxLineLength;

        private static string CheckList(string label, IReadOnlyList<string> lines)
        {
            if (lines.Count == 0) return label + ": no lines";
            for (var i = 0; i < lines.Count; i++)
                if (!IsGoodLine(lines[i])) return label + ": line " + i + " is empty or longer than " + WatcherTextAsset.MaxLineLength + " characters";
            return null;
        }
    }
}
