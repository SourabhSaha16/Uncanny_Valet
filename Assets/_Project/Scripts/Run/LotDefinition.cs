using System.Collections.Generic;
using UncannyValet.Core.Art;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Ids;
using UncannyValet.Core.Modifiers;
using UncannyValet.Core.Watchers;
using UnityEngine;

namespace UncannyValet.Run
{
    /// <summary>
    /// Describes one lot: it points at the shared config assets (wired into the lot's scene) and stores only what is
    /// different for this lot. Read-only at runtime. Each lot has its own generated scene that holds this asset.
    /// </summary>
    [CreateAssetMenu(menuName = "Uncanny Valet/Lot Definition", fileName = "LotDefinition")]
    public sealed class LotDefinition : ScriptableObject
    {
        [SerializeField] private LotId _id;
        [SerializeField, Tooltip("Shown on the picker, the intro card and the results.")]
        private string _displayName;
        [SerializeField, Tooltip("Name of this lot's generated scene (a scene in Build Settings).")]
        private string _sceneName;
        [SerializeField, Tooltip("Which parametric layout the lot is built from.")]
        private LotLayoutKind _layout = LotLayoutKind.Diner;
        [SerializeField, Range(1, MaxQueueSlots), Tooltip("Arrival queue slots below the curb. Training lots 3, crazy lots 4. The lot grows taller for more.")]
        private int _queueSlots = DefaultQueueSlots;
        [SerializeField] private LotPacing _pacing = new LotPacing();
        [SerializeField, Tooltip("The service modifiers cars of this lot arrive with and the chance of each; the list order is the roll order.")]
        private List<ModifierChance> _modifierChances = new List<ModifierChance>();
        [SerializeField, Tooltip("Dials that start somewhere other than the global baseline in this lot.")]
        private List<DialPosition> _dials = new List<DialPosition>();
        [SerializeField, Tooltip("Power bar gained per Perfect Park, perfect drop-off and drift in this lot.")]
        private LotPowerFill _powerFill = new LotPowerFill();
        [SerializeField, Tooltip("The mechanics this lot uses.")]
        private List<LotModifier> _modifiers = new List<LotModifier>();
        [SerializeField, Min(1), Tooltip("Blessings needed to clear the lot. Placeholder until the blessing task tunes it.")]
        private int _blessingThreshold = 150;
        [SerializeField, Tooltip("Pedestrian collision curses: on, hitting a patron walker with the car you drive is a curse. Off in training lots (knockback, slowdown and yelp still happen).")]
        private bool _pedestrianCurses;
        [SerializeField, Tooltip("The power or upgrade clearing this lot grants (None for training lots).")]
        private PowerGrant _grant;
        [SerializeField, Tooltip("The colours of this lot's world; the props are drawn with it and the diner takes its wall and roof colours. Empty keeps the old look.")]
        private WorldPalette _palette;
        [SerializeField, Tooltip("The props of this lot (recipe and centre in lot space), drawn once when the lot loads. Needs the palette.")]
        private List<PropPlacement> _props = new List<PropPlacement>();
        [SerializeField, Tooltip("The lot's lighting: global light plus up to 16 point lights. Empty keeps the default lighting.")]
        private LightingPreset _lighting;
        [SerializeField, Tooltip("The character who watches this lot from the side (the local god in a crazy lot, the trainer in a training lot). Empty: nobody watches.")]
        private WatcherDefinition _watcher;
        [SerializeField, Tooltip("Where the watcher stands, in lot space: on a sidewalk or the lot edge, outside the drivable area.")]
        private Vector2 _watcherSpot;

        /// <summary>Most queue slots a layout is built for.</summary>
        public const int MaxQueueSlots = 6;
        /// <summary>Slots of a new lot (the training diner).</summary>
        public const int DefaultQueueSlots = 3;

        public LotId Id => _id;
        public string DisplayName => _displayName;
        public string SceneName => _sceneName;
        public LotLayoutKind Layout => _layout;
        public int QueueSlots => _queueSlots;
        public LotPacing Pacing => _pacing;
        public IReadOnlyList<ModifierChance> ModifierChances => _modifierChances;
        public IReadOnlyList<DialPosition> Dials => _dials;
        public LotPowerFill PowerFill => _powerFill;
        public IReadOnlyList<LotModifier> Modifiers => _modifiers;
        public int BlessingThreshold => _blessingThreshold;
        public PowerGrant Grant => _grant;
        public WorldPalette Palette => _palette;
        public IReadOnlyList<PropPlacement> Props => _props;
        public LightingPreset Lighting => _lighting;
        public WatcherDefinition Watcher => _watcher;
        public Vector2 WatcherSpot => _watcherSpot;

        /// <summary>True when hitting a patron walker with the driven car is a curse in this lot.</summary>
        public bool PedestrianCurses => _pedestrianCurses;

        /// <summary>
        /// Seeds shift length, pacing, the arrival budget (see <see cref="LotPacing.SeedInto"/>), the modifier arrival chances and the power bar fill amounts;
        /// the waiting cap never exceeds <see cref="QueueSlots"/>.
        /// </summary>
        public void SeedPacing(ITuning tuning, float gameMinutesPerSecond)
        {
            _pacing.SeedInto(tuning, gameMinutesPerSecond, _queueSlots);
            _powerFill.SeedInto(tuning);
            for (var i = 0; i < _modifierChances.Count; i++)
                tuning.Seed(_modifierChances[i].Modifier.ChanceKey, _modifierChances[i].Chance);
            for (var i = 0; i < _modifiers.Count; i++) _modifiers[i].SeedInto(tuning);
        }

        /// <summary>What the lot's mechanics add to the world (the mud patch list); the scene builder hands it to the lot builder.</summary>
        public LotWorldSpec BuildWorldSpec()
        {
            var spec = new LotWorldSpec();
            for (var i = 0; i < _modifiers.Count; i++) _modifiers[i].Describe(spec);
            return spec;
        }

        /// <summary>The definitions of the lot's modifiers in roll order (the modifier catalog of the lot).</summary>
        public ModifierCatalog BuildModifierCatalog()
        {
            var definitions = new List<ServiceModifierDefinition>(_modifierChances.Count);
            for (var i = 0; i < _modifierChances.Count; i++) definitions.Add(_modifierChances[i].Modifier);
            return new ModifierCatalog(definitions);
        }

        /// <summary>Seeds this lot's dial positions; call before the dials capture their baselines.</summary>
        public void SeedDialPositions(ITuning tuning)
        {
            for (var i = 0; i < _dials.Count; i++) tuning.Seed(_dials[i].Dial, _dials[i].Position);
        }

        /// <summary>Checks the asset; on failure <paramref name="problem"/> says what is wrong.</summary>
        public bool TryValidate(out string problem) => LotDefinitionRules.TryValidate(this, out problem);
    }
}
