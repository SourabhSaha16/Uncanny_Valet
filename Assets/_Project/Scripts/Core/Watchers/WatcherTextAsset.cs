using System.Collections.Generic;
using UnityEngine;

namespace UncannyValet.Core.Watchers
{
    /// <summary>
    /// Every line one watcher can say, in plain lists the owner edits without code: tips (each with its situation), about three comments for
    /// each event, and (gods only) the verdict lines of the results screen. Lines are uppercase, up to <see cref="MaxLineLength"/> characters
    /// and only use characters of the pixel font. Task 84's story-text asset can take these lists over. Read-only at runtime.
    /// </summary>
    [CreateAssetMenu(menuName = "Uncanny Valet/Watcher Text", fileName = "WatcherText")]
    public sealed class WatcherTextAsset : ScriptableObject
    {
        /// <summary>Longest line the bubble is laid out for, in characters.</summary>
        public const int MaxLineLength = 40;

        private static readonly IReadOnlyList<string> NoLines = new string[0];

        [SerializeField, Tooltip("Tips about this lot's own mechanics (never about powers). Each is said at most once per lot, when its situation arises.")]
        private List<WatcherTip> _tips = new List<WatcherTip>();
        [SerializeField, Tooltip("A patron turned angry, or a curse.")] private List<string> _curse = new List<string>();
        [SerializeField] private List<string> _perfectPark = new List<string>();
        [SerializeField] private List<string> _perfectDropOff = new List<string>();
        [SerializeField, Tooltip("A hit that damaged a car by more than the crash amount.")] private List<string> _bigCrash = new List<string>();
        [SerializeField, Tooltip("The shift's blessings passed half of the threshold.")] private List<string> _halfwayToGoal = new List<string>();
        [SerializeField, Tooltip("The shift's blessings reached the threshold.")] private List<string> _goalReached = new List<string>();
        [SerializeField] private List<string> _last30Seconds = new List<string>();
        [SerializeField, Tooltip("Gods only: shown on the results screen when the lot was failed.")] private List<string> _verdictFailed = new List<string>();
        [SerializeField, Tooltip("Gods only: shown when the lot was cleared.")] private List<string> _verdictCleared = new List<string>();
        [SerializeField, Tooltip("Gods only: shown when the lot was cleared well.")] private List<string> _verdictClearedWell = new List<string>();

        public IReadOnlyList<WatcherTip> Tips => _tips;

        /// <summary>The lines said after <paramref name="watcherEvent"/>.</summary>
        public IReadOnlyList<string> Lines(WatcherEvent watcherEvent)
        {
            switch (watcherEvent)
            {
                case WatcherEvent.Curse: return _curse;
                case WatcherEvent.PerfectPark: return _perfectPark;
                case WatcherEvent.PerfectDropOff: return _perfectDropOff;
                case WatcherEvent.BigCrash: return _bigCrash;
                case WatcherEvent.HalfwayToGoal: return _halfwayToGoal;
                case WatcherEvent.GoalReached: return _goalReached;
                case WatcherEvent.Last30Seconds: return _last30Seconds;
                default: return NoLines;
            }
        }

        /// <summary>The results screen lines for a verdict; empty for a trainer.</summary>
        public IReadOnlyList<string> Verdict(WatcherVerdict verdict)
        {
            switch (verdict)
            {
                case WatcherVerdict.Failed: return _verdictFailed;
                case WatcherVerdict.Cleared: return _verdictCleared;
                case WatcherVerdict.ClearedWell: return _verdictClearedWell;
                default: return NoLines;
            }
        }

        /// <summary>Makes a text asset (used by the setup tool and the tests); null lists stay empty.</summary>
        public static WatcherTextAsset Create(string assetName, List<WatcherTip> tips, Dictionary<WatcherEvent, string[]> comments,
            Dictionary<WatcherVerdict, string[]> verdicts)
        {
            var text = CreateInstance<WatcherTextAsset>();
            text.name = assetName;
            text._tips = tips ?? new List<WatcherTip>();
            if (comments != null)
                foreach (var pair in comments) text.ListFor(pair.Key)?.AddRange(pair.Value);
            if (verdicts != null)
                foreach (var pair in verdicts) text.VerdictList(pair.Key)?.AddRange(pair.Value);
            return text;
        }

        private List<string> ListFor(WatcherEvent watcherEvent) => (List<string>)Lines(watcherEvent);

        private List<string> VerdictList(WatcherVerdict verdict) => (List<string>)Verdict(verdict);
    }
}
