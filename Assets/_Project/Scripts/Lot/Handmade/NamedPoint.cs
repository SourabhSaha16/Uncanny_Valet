using UnityEngine;

namespace UncannyValet.Lot.Handmade
{
    /// <summary>A marker that is just a place: the valet start, a patron wait point, an ambient spawn point or a walk node.</summary>
    public readonly struct NamedPoint
    {
        public readonly string Name;
        public readonly Vector2 Position;

        public NamedPoint(string name, Vector2 position)
        {
            Name = name;
            Position = position;
        }
    }
}
