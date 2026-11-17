using UnityEngine;

namespace UncannyValet.Cars
{
    /// <summary>
    /// How much mud a car feels, 0 to 1. It is 1 while the car is in a puddle and fades out in a straight line over the release
    /// time after the car leaves; nothing eases in, the first wheel in mud slows the car at once.
    /// </summary>
    public sealed class MudBlend
    {
        /// <summary>0 = clear of mud, 1 = in mud.</summary>
        public float Weight { get; private set; }

        /// <summary>Advances the blend by <paramref name="dt"/> seconds. A release time of 0 ends the effect at once.</summary>
        public void Step(bool inside, float dt, float releaseSeconds)
        {
            if (inside) Weight = 1f;
            else if (releaseSeconds <= 0f) Weight = 0f;
            else Weight = Mathf.Max(0f, Weight - dt / releaseSeconds);
        }

        /// <summary>Puts the blend back to clear.</summary>
        public void Clear() => Weight = 0f;
    }
}
