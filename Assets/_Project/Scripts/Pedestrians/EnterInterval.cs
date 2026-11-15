using UnityEngine;

namespace UncannyValet.Pedestrians
{
    /// <summary>Pure draw of how long a roaming person wanders before deciding to go to the diner door.</summary>
    public static class EnterInterval
    {
        /// <summary>A time between the two ends of <paramref name="range"/> (either order) picked by <paramref name="roll01"/> (0..1).</summary>
        public static float Next(Vector2 range, float roll01)
            => Mathf.Lerp(Mathf.Min(range.x, range.y), Mathf.Max(range.x, range.y), Mathf.Clamp01(roll01));
    }
}
