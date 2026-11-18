using UnityEngine;

namespace UncannyValet.Lot.Handmade
{
    /// <summary>A door where people vanish into the building and appear from it.</summary>
    public readonly struct DoorData
    {
        public readonly string Name;
        public readonly DoorKind Kind;
        public readonly Vector2 Position;

        public DoorData(string name, DoorKind kind, Vector2 position)
        {
            Name = name;
            Kind = kind;
            Position = position;
        }
    }
}
