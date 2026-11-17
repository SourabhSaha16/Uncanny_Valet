using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UnityEngine;

namespace UncannyValet.Run
{
    /// <summary>
    /// Mud patches of a lot: brown puddles on the shortest driving lines that slow any car driving through them and dull its drifting.
    /// The valet on foot and the pedestrians are not affected, and mud never makes a car Muddy. Read-only at runtime.
    /// </summary>
    [CreateAssetMenu(menuName = "Uncanny Valet/Lot Modifiers/Mud Patches", fileName = "MudPatchModifier")]
    public sealed class MudPatchModifier : LotModifier
    {
        [SerializeField, Tooltip("The puddles: centre in the lot's own space and width (about 2.5 to 4), listed along the shortest driving lines.")]
        private List<MudPatch> _patches = new List<MudPatch>();
        [SerializeField, Range(0.1f, 1f), Tooltip("Top speed multiplier in mud. 0.6 means 60% of the top speed.")]
        private float _topSpeedScale = MudRows.DefaultTopSpeedScale;
        [SerializeField, Range(0.1f, 1f), Tooltip("Acceleration multiplier in mud. 0.6 means 60%.")]
        private float _accelerationScale = MudRows.DefaultAccelerationScale;
        [SerializeField, Range(0f, 1f), Tooltip("Share of the normal grip loss (drifting) a car keeps in mud. 0.4 means 40%.")]
        private float _gripLossKept = MudRows.DefaultGripLossKept;
        [SerializeField, Range(0f, 2f), Tooltip("Seconds the slowdown takes to fade after the car leaves a puddle.")]
        private float _releaseSeconds = MudRows.DefaultReleaseSeconds;

        public IReadOnlyList<MudPatch> Patches => _patches;
        public float TopSpeedScale => _topSpeedScale;
        public float AccelerationScale => _accelerationScale;
        public float GripLossKept => _gripLossKept;
        public float ReleaseSeconds => _releaseSeconds;

        /// <summary>Adds every puddle to the world spec.</summary>
        public override void Describe(LotWorldSpec spec)
        {
            for (var i = 0; i < _patches.Count; i++) spec.AddMudPatch(_patches[i]);
        }

        /// <summary>Seeds the four mud numbers of this lot over the defaults.</summary>
        public override void SeedInto(ITuning tuning)
            => MudRows.Seed(tuning, _topSpeedScale, _accelerationScale, _gripLossKept, _releaseSeconds);
    }
}
