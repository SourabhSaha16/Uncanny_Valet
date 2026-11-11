using UnityEngine;

namespace UncannyValet.Cars.Fx
{
    /// <summary>Pure placement of a car's rear tyres.</summary>
    public static class TyreMarkMath
    {
        /// <summary>
        /// World position of a rear tyre of a car at <paramref name="position"/> turned <paramref name="rotationDegrees"/>
        /// (0 = nose up). The tyre sits <paramref name="sideShare"/> of the width off the centre line and
        /// <paramref name="rearShare"/> of the length behind the centre.
        /// </summary>
        public static Vector2 RearTyre(Vector2 position, float rotationDegrees, Vector2 size, float sideShare, float rearShare, bool right)
        {
            var local = new Vector2((right ? 1f : -1f) * size.x * sideShare, -size.y * rearShare);
            return position + (Vector2)(Quaternion.Euler(0f, 0f, rotationDegrees) * local);
        }
    }
}
