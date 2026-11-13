using UnityEngine;

namespace UncannyValet.Valet
{
    /// <summary>Which way a dash goes: the held movement direction, else the way the valet faces.</summary>
    public static class DashDirection
    {
        /// <summary>A unit vector: <paramref name="move"/> when a movement key is held, else <paramref name="facing"/>'s direction.</summary>
        public static Vector2 Choose(Vector2 move, ValetFacing facing) =>
            move.sqrMagnitude > 0f ? move.normalized : FromFacing(facing);

        /// <summary>The unit vector a sprite facing points along.</summary>
        public static Vector2 FromFacing(ValetFacing facing)
        {
            switch (facing)
            {
                case ValetFacing.Right: return Vector2.right;
                case ValetFacing.Up: return Vector2.up;
                case ValetFacing.Left: return Vector2.left;
                default: return Vector2.down;
            }
        }
    }
}
