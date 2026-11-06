using UnityEngine;

namespace UncannyValet.Cars
{
    /// <summary>
    /// Pure mirror placement. Results are in the car's local unit space (body spans -0.5..0.5),
    /// so they can be used directly by children of the scaled car. +Y is the front.
    /// </summary>
    public static class CarMirrorMath
    {
        /// <summary>Local centre of the mirror on the side given by <paramref name="side"/> (-1 left, +1 right).</summary>
        public static Vector2 LocalCentre(Vector2 carSize, Vector2 mirrorSize, float frontInset, float outset, int side)
        {
            float x = carSize.x * 0.5f + outset - mirrorSize.x * 0.5f;
            float y = carSize.y * 0.5f - frontInset - mirrorSize.y * 0.5f;
            return new Vector2(Mathf.Sign(side) * x / carSize.x, y / carSize.y);
        }

        /// <summary>Local scale that gives a child a world size of <paramref name="mirrorSize"/>.</summary>
        public static Vector2 LocalScale(Vector2 carSize, Vector2 mirrorSize)
        {
            return new Vector2(mirrorSize.x / carSize.x, mirrorSize.y / carSize.y);
        }
    }
}
