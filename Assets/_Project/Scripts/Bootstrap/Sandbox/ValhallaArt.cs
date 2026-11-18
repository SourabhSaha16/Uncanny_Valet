using System;
using UnityEngine;

namespace UncannyValet.Bootstrap.Sandbox
{
    /// <summary>The sprites of the Valhalla Midgard lot, one per <see cref="ValhallaPiece"/> in enum order. Filled by the editor setup; read only at runtime.</summary>
    [CreateAssetMenu(menuName = "Uncanny Valet/Valhalla Art", fileName = "ValhallaArt")]
    public sealed class ValhallaArt : ScriptableObject
    {
        [SerializeField] private Sprite[] _sprites = Array.Empty<Sprite>();

        /// <summary>True when every piece has a sprite.</summary>
        public bool IsComplete
        {
            get
            {
                if (_sprites.Length != Enum.GetValues(typeof(ValhallaPiece)).Length) return false;
                foreach (var sprite in _sprites) if (sprite == null) return false;
                return true;
            }
        }

        /// <summary>The sprite of a piece.</summary>
        public Sprite Get(ValhallaPiece piece) => _sprites[(int)piece];
    }
}
