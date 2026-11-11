using UncannyValet.Core.Contracts;
using UnityEngine;

namespace UncannyValet.Bootstrap
{
    /// <summary>The player's position: the valet's transform, which tracks the car he drives.</summary>
    public sealed class TransformPlayerFocus : IPlayerFocus
    {
        private readonly Transform _valet;

        public TransformPlayerFocus(Transform valet) { _valet = valet; }

        public Vector2 Position => _valet != null ? (Vector2)_valet.position : Vector2.zero;
    }
}
