using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>One painted mark on a route: where it sits and the unit direction of travel there.</summary>
    public readonly struct RouteMark
    {
        public readonly Vector2 Position;
        public readonly Vector2 Direction;

        public RouteMark(Vector2 position, Vector2 direction)
        {
            Position = position;
            Direction = direction;
        }
    }
}
