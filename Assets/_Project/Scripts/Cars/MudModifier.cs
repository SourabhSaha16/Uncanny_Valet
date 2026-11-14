using UnityEngine;

namespace UncannyValet.Cars
{
    /// <summary>
    /// The mud effect on a driven car, through the drive modifier seam: scales top speed and acceleration and the grip loss by the
    /// <see cref="MudBlend"/> weight. It never touches throttle, steering or the handbrake, and never the car's service modifiers (Muddy).
    /// </summary>
    public sealed class MudModifier : IDriveModifier
    {
        /// <summary>Runs after fault modifiers that edit steering or throttle, before the Thunder boost.</summary>
        public const int MudOrder = 90;

        private readonly MudBlend _blend;

        public MudModifier(MudBlend blend, MudSettings settings)
        {
            _blend = blend;
            Settings = settings;
        }

        /// <summary>The numbers to apply; the owner refreshes them from the tuning store.</summary>
        public MudSettings Settings { get; set; }

        public int Order => MudOrder;

        /// <summary>Speed scale of the blend: 1 clear of mud, the top speed multiplier in mud.</summary>
        public float SpeedScale => Mathf.Lerp(1f, Settings.TopSpeedScale, _blend.Weight);

        public DriveInput Modify(DriveInput input)
        {
            var w = _blend.Weight;
            if (w <= 0f) return input;
            return input.Scaled(Mathf.Lerp(1f, Settings.TopSpeedScale, w), Mathf.Lerp(1f, Settings.AccelerationScale, w))
                .WithGripLossScaled(Mathf.Lerp(1f, Settings.GripLossKept, w));
        }
    }
}
