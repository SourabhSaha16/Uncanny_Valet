using UnityEngine;

namespace UncannyValet.Cars
{
    /// <summary>Pure rules for fitting a sprite to a car whose root is scaled by its size (width, length).</summary>
    public static class CarArtMath
    {
        private const uint MixMultiplierA = 0x85EBCA6Bu;
        private const uint MixMultiplierB = 0xC2B2AE35u;
        private const int MixShiftA = 16;
        private const int MixShiftB = 13;
        private const int FlipShift = 31;

        /// <summary>Stable index in 0..count-1 for a car id, or -1 when count is 0.</summary>
        public static int PickIndex(int carId, int count)
        {
            if (count <= 0) return -1;
            return (int)(Mix(carId) % (uint)count);
        }

        /// <summary>
        /// True when a car id draws its sprite mirrored left to right. Independent of <see cref="PickIndex"/>, so a
        /// sprite appears in two looks. Stable for a car id.
        /// </summary>
        public static bool IsFlipped(int carId) => (Mix(carId) >> FlipShift) == 1u;

        /// <summary>The content centre as seen after an optional left-right mirror of the sprite.</summary>
        public static Vector2 MirrorCenter(Vector2 contentCenter, bool flipped)
            => flipped ? new Vector2(-contentCenter.x, contentCenter.y) : contentCenter;

        // Murmur3 finaliser: sequential car ids spread evenly instead of repeating in pairs.
        private static uint Mix(int carId)
        {
            unchecked
            {
                var h = (uint)carId;
                h ^= h >> MixShiftA;
                h *= MixMultiplierA;
                h ^= h >> MixShiftB;
                h *= MixMultiplierB;
                h ^= h >> MixShiftA;
                return h;
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
