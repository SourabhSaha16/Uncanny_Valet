using System;
using System.Collections.Generic;
using UnityEngine;

namespace UncannyValet.Core.Story
{
    /// <summary>
    /// The words every story card shares: tags, frames, headings and the generic cards (sent back to Midgard, the two end cards).
    /// A <c>{0}</c> in a line is replaced by the card (a name, a number). Uppercase, only the pixel font's characters.
    /// </summary>
    [Serializable]
    public sealed class StoryGenericText
    {
        [Header("Intro card")]
        [SerializeField] private string _trainingTag = "TRAINING";
        [SerializeField] private string _crazyTag = "CRAZY";
        [SerializeField] private string _finaleTag = "FINALE";
        [SerializeField, Tooltip("Crazy lots are the local god's test. {0} is the god's name.")] private string _godFrame = "THE TEST OF {0}";
        [SerializeField, Tooltip("{0} is the trainer's name.")] private string _trainerFrame = "TRAINER: {0}";
        [SerializeField, Tooltip("{0} is the lot's blessing threshold.")] private string _goalLine = "EARN {0} BLESSINGS TO CLEAR THE LOT";

        [Header("Reward card")]
        [SerializeField] private string _newPowerHeading = "NEW POWER!";
        [SerializeField] private string _upgradeHeading = "POWER UPGRADED!";
        [SerializeField, Tooltip("{0} is the key number.")] private string _keyHint = "PRESS {0} TO USE IT";

        [Header("Sent back to Midgard")]
        [SerializeField] private string _sentBackHeading = "THROWN BACK!";
        [SerializeField] private List<string> _sentBackLines = new List<string>
            { "THE TEST WAS FAILED.", "THOR FALLS BACK TO MIDGARD." };
        [SerializeField] private string _keptHeading = "POWERS KEPT:";
        [SerializeField] private string _keptNone = "NONE YET";
        [SerializeField, Tooltip("Added to the name of an upgraded power.")] private string _upgradedMark = "+";

        [Header("End cards")]
        [SerializeField] private string _continuedHeading = "TO BE CONTINUED";
        [SerializeField] private List<string> _continuedLines = new List<string>
            { "THE ROAD GOES ON. MORE REALMS", "AWAIT THE HAMMER'S HEIR." };
        [SerializeField] private string _homeHeading = "THOR IS HOME";
        [SerializeField] private List<string> _homeLines = new List<string>
            { "ODIN OPENS THE GREAT DOORS.", "THE VALET SPEAKS FOR THE LAST TIME." };
        [SerializeField] private string _earnedHeading = "POWERS EARNED:";
        [SerializeField, Tooltip("Finale only. {0} is the finale's blessings.")] private string _finaleBlessings = "FINALE BLESSINGS: {0}";

        [SerializeField] private string _prompt = "PRESS ANY KEY";

        public string TrainingTag => _trainingTag;
        public string CrazyTag => _crazyTag;
        public string FinaleTag => _finaleTag;
        public string GodFrame => _godFrame;
        public string TrainerFrame => _trainerFrame;
        public string GoalLine => _goalLine;
        public string NewPowerHeading => _newPowerHeading;
        public string UpgradeHeading => _upgradeHeading;
        public string KeyHint => _keyHint;
        public string SentBackHeading => _sentBackHeading;
        public IReadOnlyList<string> SentBackLines => _sentBackLines;
        public string KeptHeading => _keptHeading;
        public string KeptNone => _keptNone;
        public string UpgradedMark => _upgradedMark;
        public string ContinuedHeading => _continuedHeading;
        public IReadOnlyList<string> ContinuedLines => _continuedLines;
        public string HomeHeading => _homeHeading;
        public IReadOnlyList<string> HomeLines => _homeLines;
        public string EarnedHeading => _earnedHeading;
        public string FinaleBlessings => _finaleBlessings;
        public string Prompt => _prompt;

        /// <summary>Every single-string phrase, for checks.</summary>
        public IEnumerable<string> AllPhrases()
        {
            yield return _trainingTag; yield return _crazyTag; yield return _finaleTag; yield return _godFrame;
            yield return _trainerFrame; yield return _goalLine; yield return _newPowerHeading; yield return _upgradeHeading;
            yield return _keyHint; yield return _sentBackHeading; yield return _keptHeading; yield return _keptNone;
            yield return _upgradedMark; yield return _continuedHeading; yield return _homeHeading; yield return _earnedHeading;
            yield return _finaleBlessings; yield return _prompt;
            foreach (var line in _sentBackLines) yield return line;
            foreach (var line in _continuedLines) yield return line;
            foreach (var line in _homeLines) yield return line;
        }
    }
}
