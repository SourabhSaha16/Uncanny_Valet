using UnityEngine;

namespace UncannyValet.Cars
{
    /// <summary>
    /// Pure rubber-bounce maths for car-on-car contacts: a 1D collision with restitution along the contact normal.
    /// Speeds are along the unit normal that points from the other car to this one, so a car driving into the other has a negative speed.
    /// </summary>
    public static class CarBounceRule
    {
        /// <summary>Mass of a car that cannot be shoved (a road trip car, pose driven): the other car just reflects off it.</summary>
        public const float ImmovableMass = 1000000f;

        /// <summary>True when the impact is big enough to bounce and to be heard.</summary>
        public static bool IsImpact(float approachSpeed, float minImpactSpeed) => approachSpeed >= minImpactSpeed;

        /// <summary>Closing speed of the two cars along the normal (positive when they approach).</summary>
        public static float ApproachSpeed(float ownSpeed, float otherSpeed) => otherSpeed - ownSpeed;

        /// <summary>
        /// This car's normal speed after the collision (momentum conserved, separation speed = restitution * approach speed).
        /// A heavier car changes less, so it wins the shove. Unchanged when the impact is below <paramref name="minImpactSpeed"/>.
        /// </summary>
        public static float FinalNormalSpeed(float ownSpeed, float otherSpeed, float ownMass, float otherMass, float restitution, float minImpactSpeed)
        {
            var approach = ApproachSpeed(ownSpeed, otherSpeed);
            if (!IsImpact(approach, minImpactSpeed)) return ownSpeed;
            var total = Mathf.Max(0.0001f, ownMass + otherMass);
            return (ownMass * ownSpeed + otherMass * otherSpeed + otherMass * Mathf.Clamp01(restitution) * approach) / total;
        }

        /// <summary>Impact strength 0..1: 0 at the minimum impact speed, 1 at <paramref name="fullImpactSpeed"/> and above.</summary>
        public static float Strength01(float approachSpeed, float minImpactSpeed, float fullImpactSpeed)
            => Mathf.InverseLerp(minImpactSpeed, Mathf.Max(minImpactSpeed + 0.0001f, fullImpactSpeed), approachSpeed);
    }
}
