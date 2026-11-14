using UncannyValet.Core.Events;

namespace UncannyValet.Core.Contracts
{
    /// <summary>Advanced panel rows of mud patches, in designer language, and their starting values. A lot's mud modifier seeds its own numbers over the defaults.</summary>
    public static class MudRows
    {
        public const string Group = "Mud patches";

        /// <summary>Starting top speed multiplier inside mud.</summary>
        public const float DefaultTopSpeedScale = 0.6f;
        /// <summary>Starting acceleration multiplier inside mud.</summary>
        public const float DefaultAccelerationScale = 0.6f;
        /// <summary>Starting share of the normal grip loss (drifting) kept inside mud.</summary>
        public const float DefaultGripLossKept = 0.4f;
        /// <summary>Starting seconds the mud effect takes to fade after leaving a puddle.</summary>
        public const float DefaultReleaseSeconds = 0.3f;

        public static readonly TuningInfo[] Rows =
        {
            new TuningInfo(TuningKey.MudTopSpeedScale, Group, "Mud: top speed multiplier", 0.1f, 1f, 0.05f,
                "A car with a wheel in mud can only reach this share of its top speed. 0.6 means 60%. 1 means mud does not slow it."),
            new TuningInfo(TuningKey.MudAccelerationScale, Group, "Mud: acceleration multiplier", 0.1f, 1f, 0.05f,
                "A car in mud speeds up at this share of its normal pace. 0.6 means 60%."),
            new TuningInfo(TuningKey.MudGripLossKept, Group, "Mud: drift (slip) kept", 0f, 1f, 0.05f,
                "How much of the normal grip loss a car keeps in mud: the loss from speed and from the handbrake. 0.4 means 40%, so drifting is dull and the car slides less. 1 means no change."),
            new TuningInfo(TuningKey.MudReleaseSeconds, Group, "Mud: seconds to recover", 0f, 2f, 0.05f,
                "After leaving a puddle the slowdown fades out over this many seconds. 0 means it ends at once."),
        };

        /// <summary>Seeds the four mud numbers; a mud modifier calls it with its own values.</summary>
        public static void Seed(ITuning t, float topSpeedScale, float accelerationScale, float gripLossKept, float releaseSeconds)
        {
            t.Seed(TuningKey.MudTopSpeedScale, topSpeedScale);
            t.Seed(TuningKey.MudAccelerationScale, accelerationScale);
            t.Seed(TuningKey.MudGripLossKept, gripLossKept);
            t.Seed(TuningKey.MudReleaseSeconds, releaseSeconds);
        }

        /// <summary>Seeds the defaults, so the panel rows always have a value even in a lot without mud.</summary>
        public static void SeedDefaults(ITuning t) => Seed(t, DefaultTopSpeedScale, DefaultAccelerationScale, DefaultGripLossKept, DefaultReleaseSeconds);
    }
}
