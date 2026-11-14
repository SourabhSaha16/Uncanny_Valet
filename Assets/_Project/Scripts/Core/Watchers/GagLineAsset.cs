using System.Collections.Generic;
using UnityEngine;

namespace UncannyValet.Core.Watchers
{
    /// <summary>
    /// The floating gag lines of one lot: the one-line pops above a pedestrian when the valet (or the car he drives) passes close, with the
    /// distance and the lockouts that pace them. Lines are uppercase, up to <see cref="MaxLineLength"/> characters and only use the pixel font's
    /// characters, so the owner can edit them without code. Read-only at runtime.
    /// </summary>
    [CreateAssetMenu(menuName = "Uncanny Valet/Gag Lines", fileName = "GagLines")]
    public sealed class GagLineAsset : ScriptableObject
    {
        /// <summary>Longest line, in characters (the same as a watcher's).</summary>
        public const int MaxLineLength = WatcherTextAsset.MaxLineLength;

        /// <summary>The characters the pixel font can draw (kept equal to the UI's glyph table by a test).</summary>
        public const string AllowedCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 .,:!?-+/$%'#()*";

        [SerializeField, Tooltip("The name shown above the line (FAIRGOER, PATRON).")] private string _speaker = "PATRON";
        [SerializeField, Min(0.5f), Tooltip("A walker pops a line when the valet or his car is within this many world units.")]
        private float _triggerDistance = 3f;
        [SerializeField, Min(0f), Tooltip("Seconds after any pop before another one may start.")]
        private float _globalLockoutSeconds = 6f;
        [SerializeField, Min(0f), Tooltip("Seconds before the same walker may pop again.")]
        private float _walkerLockoutSeconds = 20f;
        [SerializeField, Tooltip("The lines, said in turn. Uppercase, up to 40 characters, only A-Z 0-9 and . , : ! ? - + / $ % ' # ( ) *.")]
        private List<string> _lines = new List<string>();

        public string Speaker => _speaker;
        public float TriggerDistance => _triggerDistance;
        public float GlobalLockoutSeconds => _globalLockoutSeconds;
        public float WalkerLockoutSeconds => _walkerLockoutSeconds;
        public IReadOnlyList<string> Lines => _lines;

        /// <summary>Null when the asset is usable, otherwise the first fault (no speaker, no lines, a line too long, empty or with a character the font lacks).</summary>
        public string Check()
        {
            if (string.IsNullOrWhiteSpace(_speaker)) return name + ": no speaker name";
            if (_lines.Count == 0) return name + ": no lines";
            for (var i = 0; i < _lines.Count; i++)
            {
                var line = _lines[i];
                if (string.IsNullOrWhiteSpace(line) || line.Length > MaxLineLength) return name + ": line " + i + " is empty or longer than " + MaxLineLength + " characters";
                for (var c = 0; c < line.Length; c++)
                    if (AllowedCharacters.IndexOf(line[c]) < 0) return name + ": line " + i + " has the character '" + line[c] + "', which the pixel font cannot draw";
            }
            return null;
        }

        /// <summary>Makes an asset (used by the setup tool and the tests).</summary>
        public static GagLineAsset Create(string assetName, string speaker, IEnumerable<string> lines)
        {
            var asset = CreateInstance<GagLineAsset>();
            asset.name = assetName;
            asset._speaker = speaker;
            asset._lines = new List<string>(lines);
            return asset;
        }
    }
}
