using UnityEngine;

namespace UncannyValet.Lot.Handmade
{
    /// <summary>Marks a Tilemap as one layer of the lot. Building and Hedge layers must carry a collider (the validator warns otherwise).</summary>
    [DisallowMultipleComponent]
    public sealed class LotTileLayer : MonoBehaviour
    {
        [SerializeField, Tooltip("Which layer this tilemap is.")] private TileLayerKind _kind;

        public TileLayerKind Kind => _kind;
    }
}
