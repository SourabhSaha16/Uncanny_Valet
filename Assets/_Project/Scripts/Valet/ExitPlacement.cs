using UnityEngine;

namespace UncannyValet.Valet
{
    /// <summary>Pure helper: where the valet stands after leaving a car, or after the car vanished.</summary>
    public static class ExitPlacement
    {
        /// <summary>Car position nudged along the aside direction (falls back to world right if it is zero).</summary>
        public static Vector2 Beside(Vector2 carPosition, Vector2 aside, float distance)
        {
            Vector2 dir = aside.sqrMagnitude > 0.0001f ? aside.normalized : Vector2.right;
            return carPosition + dir * distance;
        }
    }
}
