using System;
using UnityEngine;

namespace UncannyValet.Valet
{
    /// <summary>The tunable timing of the Thunder stride glow: how fast it pulses and when and how fast it blinks to warn.</summary>
    [Serializable]
    public struct ThunderAuraTiming
    {
        [Min(0.1f), Tooltip("Pulses per second of the glow and tint while the boost runs.")]
        public float PulseHz;
        [Range(0f, 1f), Tooltip("How strong the glow is at the bottom of a pulse (1 = no pulsing).")]
        public float PulseFloor;
        [Min(0f), Tooltip("Seconds before the boost ends when the glow starts to blink as a warning.")]
        public float WarningSeconds;
        [Min(0.5f), Tooltip("Blinks per second during the warning.")]
        public float BlinkHz;

        /// <summary>The shipped look: a 2 Hz pulse, 1 second of 5 Hz blinking before the end.</summary>
        public static ThunderAuraTiming Default => new ThunderAuraTiming { PulseHz = 2f, PulseFloor = 0.55f, WarningSeconds = 1f, BlinkHz = 5f };
    }
}
