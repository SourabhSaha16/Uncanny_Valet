using UnityEngine;

namespace UncannyValet.Cars
{
    /// <summary>One FixedUpdate snapshot of how a car moves relative to its facing; what skid marks and tyre sound read.</summary>
    public readonly struct SlipReading
    {
        /// <summary>Speed along the facing (negative when reversing), units per second.</summary>
        public readonly float Forward;
        /// <summary>Speed across the facing (right is positive), units per second.</summary>
        public readonly float Sideways;
        /// <summary>Throttle after the input modifiers, -1..1.</summary>
        public readonly float Throttle;
        /// <summary>True while the drift button (handbrake) is held.</summary>
        public readonly bool Drift;

        public SlipReading(float forward, float sideways, float throttle, bool drift)
        {
            Forward = forward;
            Sideways = sideways;
            Throttle = throttle;
            Drift = drift;
        }

        /// <summary>Total speed, units per second.</summary>
        public float Speed => Mathf.Sqrt(Forward * Forward + Sideways * Sideways);

        /// <summary>Angle between facing and travel in degrees (the same maths as <see cref="DriftModel"/>).</summary>
        public float SlipAngleDegrees => DriftModel.SlipAngleDegrees(Forward, Sideways);
    }
}
