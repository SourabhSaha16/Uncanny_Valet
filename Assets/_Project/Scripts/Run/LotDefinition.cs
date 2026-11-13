using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Ids;
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
        [SerializeField, Tooltip("Dials that start somewhere other than the global baseline in this lot.")]
        private List<DialPosition> _dials = new List<DialPosition>();
        [SerializeField, Tooltip("The mechanics this lot uses.")]
        private List<LotModifier> _modifiers = new List<LotModifier>();
        [SerializeField, Min(1), Tooltip("Blessings needed to clear the lot. Placeholder until the blessing task tunes it.")]
        private int _blessingThreshold = 3;
        [SerializeField, Tooltip("The power or upgrade clearing this lot grants (None for training lots).")]
        private PowerGrant _grant;

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
        public IReadOnlyList<DialPosition> Dials => _dials;
        public IReadOnlyList<LotModifier> Modifiers => _modifiers;
        public int BlessingThreshold => _blessingThreshold;
        public PowerGrant Grant => _grant;

        /// <summary>Seeds shift length, pacing and the arrival budget (see <see cref="LotPacing.SeedInto"/>); the waiting cap never exceeds <see cref="QueueSlots"/>.</summary>
        public void SeedPacing(ITuning tuning, float gameMinutesPerSecond) =>
            _pacing.SeedInto(tuning, gameMinutesPerSecond, _queueSlots);

        /// <summary>Seeds this lot's dial positions; call before the dials capture their baselines.</summary>
        public void SeedDialPositions(ITuning tuning)
        {
            for (var i = 0; i < _dials.Count; i++) tuning.Seed(_dials[i].Dial, _dials[i].Position);
        }

        /// <summary>Checks the asset; on failure <paramref name="problem"/> says what is wrong.</summary>
        public bool TryValidate(out string problem) => LotDefinitionRules.TryValidate(this, out problem);
    }
}
