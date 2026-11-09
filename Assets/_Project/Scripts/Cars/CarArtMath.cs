using UnityEngine;

namespace UncannyValet.Cars
{
    /// <summary>Pure rules for fitting a sprite to a car whose root is scaled by its size (width, length).</summary>
    public static class CarArtMath
    {
        private const uint HashMultiplier = 2654435761u;
        private const int HashShift = 8;

        /// <summary>Stable index in 0..count-1 for a car id, or -1 when count is 0.</summary>
        public static int PickIndex(int carId, int count)
        {
            if (count <= 0) return -1;
            unchecked
            {
                var hash = (uint)carId * HashMultiplier;
                return (int)((hash >> HashShift) % (uint)count);
            }
        }

        /// <summary>World units per sprite unit so the sprite's opaque length equals the car length.</summary>
        public static float UniformScale(Vector2 contentSize, Vector2 carSize)
            => contentSize.y <= 0f ? 1f : carSize.y / contentSize.y;

        /// <summary>Local scale of a child under a root scaled by <paramref name="carSize"/>, giving a uniform world scale.</summary>
        public static Vector2 LocalScale(float uniform, Vector2 carSize)
            => new Vector2(uniform / carSize.x, uniform / carSize.y);

        /// <summary>Local position of the child that puts the opaque centre of the sprite on the car centre.</summary>
        public static Vector2 LocalOffset(Vector2 contentCenter, float uniform, Vector2 carSize, float rotationDegrees)
        {
            var world = -(Vector2)(Quaternion.Euler(0f, 0f, rotationDegrees) * (contentCenter * uniform));
            return new Vector2(world.x / carSize.x, world.y / carSize.y);
        }

        /// <summary>True for rotation offsets that keep a uniform world scale under a non-uniform parent.</summary>
        public static bool IsHalfTurn(float degrees) => Mathf.Abs(Mathf.Repeat(degrees, 180f)) < 0.01f;
    }
}
