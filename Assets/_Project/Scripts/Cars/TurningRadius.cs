using UnityEngine;

namespace UncannyValet.Cars
{
    /// <summary>
    /// Pure turning-radius and bay-geometry maths from docs/redesign/car_handling_spec.md section 3. All lengths are
    /// Unity units, rates are radians per second.
    /// </summary>
    public static class TurningRadius
    {
        /// <summary>Tightest radius that swings a car from the aisle centre into a perpendicular bay: aisle / 2 + margin.</summary>
        public static float RMin(float aisleWidth, float margin) => aisleWidth * 0.5f + margin;

        /// <summary>Turn rate (rad/s) that gives <paramref name="rMin"/> at <paramref name="turnFullSpeed"/>.</summary>
        public static float MaxTurnRate(float turnFullSpeed, float rMin) => turnFullSpeed / Mathf.Max(0.01f, rMin);

        /// <summary>Radius at a speed: constant R_min up to TurnFullSpeed, then speed / MaxTurnRate.</summary>
        public static float RadiusAtSpeed(float speed, float rMin, float maxTurnRate)
            => Mathf.Max(rMin, Mathf.Abs(speed) / Mathf.Max(0.01f, maxTurnRate));

        /// <summary>Smallest aisle that lets a car reverse out of a bay at full lock: R_min + car width / 2 + clearance.</summary>
        public static float PullOutAisle(float rMin, float carWidth, float clearance) => rMin + carWidth * 0.5f + clearance;

        /// <summary>True when the aisle is wide enough to pull out (see <see cref="PullOutAisle"/>).</summary>
        public static bool PullOutClears(float aisleWidth, float rMin, float carWidth, float clearance)
            => aisleWidth >= PullOutAisle(rMin, carWidth, clearance);

        /// <summary>Side clearance of a car centred in a bay, per side.</summary>
        public static float BaySlack(float bayWidth, float carWidth) => (bayWidth - carWidth) * 0.5f;
    }
}
