using UnityEngine;

namespace UncannyValet.Pedestrians
{
    /// <summary>Pure rule: which car contacts may knock a person away or bounce them. Only the car the valet drives can; every other car never does.</summary>
    public static class PedestrianHitRule
    {
        /// <summary>True when a car with (or without) a driver may knock or bounce a person: only a driven car.</summary>
        public static bool MayKnock(bool carHasDriver) => carHasDriver;

        /// <summary>
        /// The speed of a car hit: the contact's relative speed minus the person's own walking speed (which the contact counts), so a
        /// person strolling into a standing car is not a hit.
        /// </summary>
        public static float HitSpeed(float relativeSpeed, float walkerSpeed) => Mathf.Max(0f, relativeSpeed - Mathf.Max(0f, walkerSpeed));
    }
}
