using UnityEngine;

namespace UncannyValet.Cars
{
    /// <summary>
    /// Pure damage maths: an impact force (the speed change a hit forces on a car, units per second) becomes damage on a 0..1 meter.
    /// Hits at or below the threshold are free, damage grows linearly above it and the meter stops at 1 (a wreck).
    /// </summary>
    public static class DamageRule
    {
        /// <summary>The whole meter: 1 is a wreck.</summary>
        public const float Wrecked = 1f;

        private const float PercentToShare = 0.01f;

        /// <summary>Impact force of a hit: the summed contact impulse divided by the car's mass (its speed change). Never negative.</summary>
        public static float Force(float normalImpulse, float mass) => mass > 0f ? Mathf.Max(0f, normalImpulse) / mass : 0f;

        /// <summary>Damage (0..1) one hit adds: 0 up to the threshold, then <paramref name="percentPerForce"/> percent per unit of force above it.</summary>
        public static float Delta01(float force, float minForce, float percentPerForce)
        {
            if (force <= minForce || percentPerForce <= 0f) return 0f;
            return (force - minForce) * percentPerForce * PercentToShare;
        }

        /// <summary>The meter after adding a hit, kept between 0 and <see cref="Wrecked"/>.</summary>
        public static float Add(float current01, float delta01) => Mathf.Clamp(current01 + Mathf.Max(0f, delta01), 0f, Wrecked);
    }
}
