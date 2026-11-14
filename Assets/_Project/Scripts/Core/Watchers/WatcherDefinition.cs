using UncannyValet.Core.Ids;
using UnityEngine;

namespace UncannyValet.Core.Watchers
{
    /// <summary>
    /// One watching character: the local god of a crazy lot or the named trainer of a training lot. It says who they are, what they look like
    /// and which text asset holds their lines; where they stand is a value of the lot that names them. Read-only at runtime. A later world adds
    /// its own god and trainer as new assets.
    /// </summary>
    [CreateAssetMenu(menuName = "Uncanny Valet/Watcher Definition", fileName = "WatcherDefinition")]
    public sealed class WatcherDefinition : ScriptableObject
    {
        [SerializeField, Tooltip("The world this watcher belongs to.")] private World _world;
        [SerializeField] private WatcherKind _kind;
        [SerializeField, Tooltip("The name shown above their speech bubbles and on the results screen (Midgard's god shows MRS. OKAFOR).")]
        private string _speakerName;
        [SerializeField] private WatcherLook _look;
        [SerializeField, Tooltip("Their tips, event comments and (gods only) verdict lines.")] private WatcherTextAsset _text;

        public World World => _world;
        public WatcherKind Kind => _kind;
        public string SpeakerName => _speakerName;
        public WatcherLook Look => _look;
        public WatcherTextAsset Text => _text;

        /// <summary>
        /// The results screen line for a verdict: the <paramref name="pick"/>-th line, wrapping round. False for a trainer or when the list is
        /// empty. Task 85 shows <see cref="SpeakerName"/> and this line.
        /// </summary>
        public bool TryGetVerdictLine(WatcherVerdict verdict, int pick, out string line)
        {
            line = null;
            if (_kind != WatcherKind.God || _text == null) return false;
            var lines = _text.Verdict(verdict);
            if (lines.Count == 0) return false;
            line = lines[((pick % lines.Count) + lines.Count) % lines.Count];
            return true;
        }

        /// <summary>Checks the asset against the pedestrian sprite set's row counts; on failure <paramref name="problem"/> says what is wrong.</summary>
        public bool TryValidate(int skins, int hairs, int outfits, out string problem)
        {
            problem = WatcherRules.Check(this, skins, hairs, outfits);
            return problem == null;
        }

        /// <summary>Makes a definition (used by the setup tool and the tests).</summary>
        public static WatcherDefinition Create(string assetName, World world, WatcherKind kind, string speaker, WatcherLook look, WatcherTextAsset text)
        {
            var definition = CreateInstance<WatcherDefinition>();
            definition.name = assetName;
            definition._world = world; definition._kind = kind; definition._speakerName = speaker;
            definition._look = look; definition._text = text;
            return definition;
        }
    }
}
