using UncannyValet.Core.Events;

namespace UncannyValet.Core.Contracts
{
    /// <summary>Advanced panel rows of the car handling model (docs/redesign/car_handling_spec.md section 2), in designer language.</summary>
    public static class HandlingRows
    {
        public const string Group = "Car handling";

        public static readonly TuningInfo[] Rows =
        {
            new TuningInfo(TuningKey.HandlingLateralFriction, Group, "Tyre bite (higher = less sliding)", 0.5f, 20f, 0.5f,
                "How quickly sideways sliding dies out when the tyres have full grip."),
            new TuningInfo(TuningKey.HandlingForwardGrip, Group, "Speed kept out of a slide", 0f, 1f, 0.05f,
                "0 = a slide scrubs speed away, 1 = the car keeps all its momentum when the slide ends."),
            new TuningInfo(TuningKey.HandlingGripLowSpeed, Group, "Full grip up to (speed)", 0.5f, 8f, 0.1f,
                "Below this speed the car goes exactly where you point it."),
            new TuningInfo(TuningKey.HandlingGripHighSpeed, Group, "Slidiest from (speed)", 1f, 12f, 0.1f,
                "Grip fades from full to its fast-driving value between the two speeds."),
            new TuningInfo(TuningKey.HandlingGripAtSpeed, Group, "Grip when driving fast", 0.05f, 1f, 0.05f,
                "1 = never slides, lower = the back end steps out in fast turns."),
            new TuningInfo(TuningKey.HandlingDriftGripScale, Group, "Grip while holding Shift", 0.05f, 1f, 0.05f,
                "How much grip is left when the handbrake is held. Lower = a wilder slide."),
            new TuningInfo(TuningKey.HandlingTurnFullSpeed, Group, "Full steering from (speed)", 0.5f, 10f, 0.1f,
                "Below this speed the steering is eased down so a stopped car cannot spin on the spot."),
            new TuningInfo(TuningKey.HandlingMinTurnRadius, Group, "Tightest turn radius (units)", 1f, 8f, 0.1f,
                "Radius of the tightest slow turn, set directly. Smaller = sharper turns; at higher speed the turn opens up."),
            new TuningInfo(TuningKey.HandlingSlideThreshold, Group, "Skid starts at (sideways speed)", 0.2f, 5f, 0.1f,
                "How fast the car must be moving sideways before it leaves skid marks and squeals."),
            new TuningInfo(TuningKey.HandlingShiftSlideSpeed, Group, "Shift skid from (speed)", 0.5f, 8f, 0.1f,
                "Holding Shift above this speed leaves skid marks even when the car is not sliding much."),
            new TuningInfo(TuningKey.HandlingMarkOpacity, Group, "Skid mark darkness", 0.1f, 1f, 0.05f,
                "How dark the marks of a full drift are. Gentle slides are fainter."),
            new TuningInfo(TuningKey.HandlingMarkFadeSeconds, Group, "Skid mark fade time (s)", 1f, 20f, 0.5f,
                "Seconds a skid mark takes to fade away."),
            new TuningInfo(TuningKey.HandlingTopSpeedScale, Group, "Top speed (x car's own)", 0.5f, 2f, 0.05f,
                "Multiplies every car's top speed. 1 keeps each car's own value, so cars still differ from each other."),
            new TuningInfo(TuningKey.HandlingAccelerationScale, Group, "Acceleration (x car's own)", 0.5f, 2f, 0.05f,
                "Multiplies how quickly every car picks up speed."),
            new TuningInfo(TuningKey.HandlingBrakingScale, Group, "Braking strength (x car's own)", 0.5f, 2f, 0.05f,
                "Multiplies how hard every car brakes. Lower = stops later."),
            new TuningInfo(TuningKey.HandlingCoastDragScale, Group, "Coasting slow-down (x car's own)", 0.25f, 3f, 0.05f,
                "Multiplies how fast a car slows when you let go of the throttle."),
            new TuningInfo(TuningKey.HandlingReverseScale, Group, "Reverse speed (x car's own)", 0.5f, 2f, 0.05f,
                "Multiplies the top speed when reversing."),
        };

        /// <summary>Advanced rows of the tyre sounds (Audio owns the values).</summary>
        public static readonly TuningInfo[] TyreRows =
        {
            new TuningInfo(TuningKey.TyreSoundVolume, Group, "Tyre sound volume", 0f, 1f, 0.05f,
                "Loudness of the skid chirp, squeal and braking screech."),
            new TuningInfo(TuningKey.TyreSoundSharpness, Group, "Tyre sound sharpness", 0f, 1f, 0.05f,
                "0 = dull and round, 1 = bright and sharp. Lower it if the tyre sounds are piercing."),
        };
    }
}
