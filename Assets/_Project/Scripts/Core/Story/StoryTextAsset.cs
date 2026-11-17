using System.Collections.Generic;
using UncannyValet.Core.Ids;
using UnityEngine;

namespace UncannyValet.Core.Story
{
    /// <summary>
    /// Every word of the between-lot story cards in one asset the owner edits without code: per world its name and accent colours, per lot the
    /// flavour lines of the intro card, per power the description and the god's farewell, and the generic cards (sent back to Midgard, end cards).
    /// Lines are uppercase, up to <see cref="MaxLineLength"/> characters and only use characters of the pixel font; long lines wrap on the card.
    /// Read-only at runtime. Follows the pattern of the watcher text asset.
    /// </summary>
    [CreateAssetMenu(menuName = "Uncanny Valet/Story Text", fileName = "StoryText")]
    public sealed class StoryTextAsset : ScriptableObject
    {
        /// <summary>Longest line in characters (the same as a watcher's).</summary>
        public const int MaxLineLength = Watchers.WatcherTextAsset.MaxLineLength;

        [SerializeField] private List<WorldStory> _worlds = new List<WorldStory>();
        [SerializeField] private List<LotStory> _lots = new List<LotStory>();
        [SerializeField] private List<PowerStory> _powers = new List<PowerStory>();
        [SerializeField] private StoryGenericText _generic = new StoryGenericText();
        [SerializeField] private List<VerdictStory> _verdicts = new List<VerdictStory>();
        [SerializeField] private List<TrainerStory> _trainers = new List<TrainerStory>();
        [SerializeField, Min(1f), Tooltip("Blessings at or above this multiple of the threshold earn the CLEARED WELL verdict line.")]
        private float _clearedWellMultiple = 1.5f;

        /// <summary>Blessings over the threshold, as a multiple of it, that earn the cleared-well verdict.</summary>
        public float ClearedWellMultiple => _clearedWellMultiple;
        public IReadOnlyList<VerdictStory> Verdicts => _verdicts;
        public IReadOnlyList<TrainerStory> Trainers => _trainers;
        public IReadOnlyList<WorldStory> Worlds => _worlds;
        public IReadOnlyList<LotStory> Lots => _lots;
        public IReadOnlyList<PowerStory> Powers => _powers;
        public StoryGenericText Generic => _generic;

        /// <summary>The world's name and accent colours; false for a world without an entry.</summary>
        public bool TryGetWorld(World world, out WorldStory story)
        {
            for (var i = 0; i < _worlds.Count; i++)
            {
                if (_worlds[i].World != world) continue;
                story = _worlds[i];
                return true;
            }
            story = default;
            return false;
        }

        /// <summary>The lot's intro lines; false for a lot without an entry.</summary>
        public bool TryGetLot(LotId lot, out LotStory story)
        {
            for (var i = 0; i < _lots.Count; i++)
            {
                if (_lots[i].Lot != lot) continue;
                story = _lots[i];
                return true;
            }
            story = default;
            return false;
        }

        /// <summary>The power's reward lines; false for a power without an entry.</summary>
        public bool TryGetPower(StoryPower power, out PowerStory story)
        {
            for (var i = 0; i < _powers.Count; i++)
            {
                if (_powers[i].Power != power) continue;
                story = _powers[i];
                return true;
            }
            story = default;
            return false;
        }

        /// <summary>The verdict lines of a crazy lot or the finale; false for a lot without an entry.</summary>
        public bool TryGetVerdict(LotId lot, out VerdictStory story)
        {
            for (var i = 0; i < _verdicts.Count; i++)
            {
                if (_verdicts[i].Lot != lot) continue;
                story = _verdicts[i];
                return true;
            }
            story = default;
            return false;
        }

        /// <summary>The trainer lines of a world's training lot; false for a world without an entry.</summary>
        public bool TryGetTrainer(World world, out TrainerStory story)
        {
            for (var i = 0; i < _trainers.Count; i++)
            {
                if (_trainers[i].World != world) continue;
                story = _trainers[i];
                return true;
            }
            story = default;
            return false;
        }

        /// <summary>Adds the verdict and trainer entries the asset lacks, leaving existing (owner edited) ones alone. Used by the setup tool. Returns true when something was added.</summary>
        public bool AddMissingVerdicts(IEnumerable<VerdictStory> verdicts, IEnumerable<TrainerStory> trainers)
        {
            var added = false;
            foreach (var verdict in verdicts)
            {
                if (TryGetVerdict(verdict.Lot, out _)) continue;
                _verdicts.Add(verdict);
                added = true;
            }
            foreach (var trainer in trainers)
            {
                if (TryGetTrainer(trainer.World, out _)) continue;
                _trainers.Add(trainer);
                added = true;
            }
            return added;
        }

        /// <summary>Makes a text asset (used by the setup tool and the tests).</summary>
        public static StoryTextAsset Create(string assetName, IEnumerable<WorldStory> worlds, IEnumerable<LotStory> lots, IEnumerable<PowerStory> powers,
            IEnumerable<VerdictStory> verdicts = null, IEnumerable<TrainerStory> trainers = null)
        {
            var text = CreateInstance<StoryTextAsset>();
            text.name = assetName;
            if (worlds != null) text._worlds.AddRange(worlds);
            if (lots != null) text._lots.AddRange(lots);
            if (powers != null) text._powers.AddRange(powers);
            if (verdicts != null) text._verdicts.AddRange(verdicts);
            if (trainers != null) text._trainers.AddRange(trainers);
            return text;
        }
    }
}
