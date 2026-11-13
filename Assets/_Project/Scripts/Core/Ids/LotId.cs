using System;
using UnityEngine;

namespace UncannyValet.Core.Ids
{
    /// <summary>
    /// Identifies one of the 17 lots: a world and a kind. Asgard has the single kind Finale. The fields are
    /// serialized (so an asset can hold an id) and never change after construction.
    /// </summary>
    [Serializable]
    public struct LotId : IEquatable<LotId>
    {
        [SerializeField] private World _world;
        [SerializeField] private LotKind _kind;

        public LotId(World world, LotKind kind)
        {
            _world = world;
            _kind = kind;
        }

        public World World => _world;
        public LotKind Kind => _kind;

        /// <summary>True for a real lot: both parts set, and Asgard if and only if Finale.</summary>
        public bool IsValid => _world != World.None && _kind != LotKind.None && (_world == World.Asgard) == (_kind == LotKind.Finale);

        public bool Equals(LotId other) => _world == other._world && _kind == other._kind;
        public override bool Equals(object obj) => obj is LotId other && Equals(other);
        public override int GetHashCode() => (int)_world * 31 + (int)_kind;
        public override string ToString() => _world + " " + _kind;

        public static bool operator ==(LotId a, LotId b) => a.Equals(b);
        public static bool operator !=(LotId a, LotId b) => !a.Equals(b);
    }
}
