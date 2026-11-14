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

        /// <summary>Makes a text asset (used by the setup tool and the tests).</summary>
        public static StoryTextAsset Create(string assetName, IEnumerable<WorldStory> worlds, IEnumerable<LotStory> lots, IEnumerable<PowerStory> powers)
        {
            var text = CreateInstance<StoryTextAsset>();
            text.name = assetName;
            if (worlds != null) text._worlds.AddRange(worlds);
            if (lots != null) text._lots.AddRange(lots);
            if (powers != null) text._powers.AddRange(powers);
            return text;
        }
    }
}
