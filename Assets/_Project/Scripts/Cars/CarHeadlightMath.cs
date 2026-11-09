using UnityEngine;

namespace UncannyValet.Cars
{
    /// <summary>
    /// Pure placement of the two headlights at the car's nose. The car root is scaled to the car size and its +Y is the
    /// front, so every position is the local offset divided by the car size. The lights are symmetric about the car's
    /// centre line, so a left-right mirrored body sprite never moves them to the back.
    /// </summary>
    public static class CarHeadlightMath
    {
        /// <summary>Centre of one light in the car's local space; <paramref name="side"/> is -1 (left) or +1 (right).</summary>
        public static Vector2 LocalCentre(Vector2 carSize, Vector2 lightSize, float frontInset, float sideInset, int side)
        {
            var x = Mathf.Sign(side) * (carSize.x * 0.5f - sideInset - lightSize.x * 0.5f);
            var y = carSize.y * 0.5f - frontInset - lightSize.y * 0.5f;
            return new Vector2(x / carSize.x, y / carSize.y);
        }

        /// <summary>Local scale that makes a 1x1 unit sprite <paramref name="size"/> world units big on a car of <paramref name="carSize"/>.</summary>
        public static Vector2 LocalScale(Vector2 carSize, Vector2 size)
            => new Vector2(size.x / carSize.x, size.y / carSize.y);
    }
}
