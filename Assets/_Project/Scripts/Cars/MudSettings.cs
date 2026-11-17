using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Cars
{
    /// <summary>The four mud numbers a car reads: top speed and acceleration multipliers, the share of grip loss kept, and the release time.</summary>
    public readonly struct MudSettings
    {
        public readonly float TopSpeedScale;
        public readonly float AccelerationScale;
        public readonly float GripLossKept;
        public readonly float ReleaseSeconds;

        public MudSettings(float topSpeedScale, float accelerationScale, float gripLossKept, float releaseSeconds)
        {
            TopSpeedScale = topSpeedScale;
            AccelerationScale = accelerationScale;
            GripLossKept = gripLossKept;
            ReleaseSeconds = releaseSeconds;
        }

        /// <summary>The starting numbers of <see cref="MudRows"/>.</summary>
        public static MudSettings Defaults => new MudSettings(MudRows.DefaultTopSpeedScale, MudRows.DefaultAccelerationScale,
            MudRows.DefaultGripLossKept, MudRows.DefaultReleaseSeconds);

        /// <summary>The live values of the tuning store; a key that was never seeded keeps its default (no error).</summary>
        public static MudSettings Read(ITuning tuning)
        {
            var d = Defaults;
            return new MudSettings(Pick(tuning, TuningKey.MudTopSpeedScale, d.TopSpeedScale),
                Pick(tuning, TuningKey.MudAccelerationScale, d.AccelerationScale),
                Pick(tuning, TuningKey.MudGripLossKept, d.GripLossKept),
                Pick(tuning, TuningKey.MudReleaseSeconds, d.ReleaseSeconds));
        }

        private static float Pick(ITuning tuning, TuningKey key, float fallback)
            => tuning != null && tuning.TryGet(key, out var value) ? value : fallback;
    }
}
