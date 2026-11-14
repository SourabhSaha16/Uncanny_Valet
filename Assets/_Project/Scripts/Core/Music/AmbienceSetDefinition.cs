using System.Collections.Generic;
using UnityEngine;

namespace UncannyValet.Core.Music
{
    /// <summary>The ambience of a lot: its background layers. Read-only at runtime; a LotDefinition names one.</summary>
    [CreateAssetMenu(menuName = "Uncanny Valet/Ambience Set", fileName = "AmbienceSet")]
    public sealed class AmbienceSetDefinition : ScriptableObject
    {
        [SerializeField, Tooltip("The layers that play with the lot.")]
        private List<AmbienceLayerMix> _layers = new List<AmbienceLayerMix>();

        public IReadOnlyList<AmbienceLayerMix> Layers => _layers;

        /// <summary>True when the set holds this kind of layer.</summary>
        public bool Has(AmbienceKind kind)
        {
            for (var i = 0; i < _layers.Count; i++)
                if (_layers[i].Kind == kind) return true;
            return false;
        }

        /// <summary>Checks the asset; null when it is fine, otherwise what is wrong.</summary>
        public string Check()
        {
            for (var i = 0; i < _layers.Count; i++)
            {
                if (_layers[i].Level < 0f || _layers[i].Level > 1f) return name + ": layer " + i + " has a level outside 0 to 1";
                for (var j = 0; j < i; j++)
                    if (_layers[j].Kind == _layers[i].Kind) return name + ": " + _layers[i].Kind + " is listed twice";
            }
            return null;
        }
    }
}
