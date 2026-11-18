using UnityEngine;

namespace UncannyValet.Lot.Handmade
{
    /// <summary>A junction of the pedestrian graph. People walk from node to node along <see cref="WalkLink"/> markers.</summary>
    [DisallowMultipleComponent]
    public sealed class WalkNode : MonoBehaviour
    {
        private void OnDrawGizmos() => MarkerGizmos.Dot(transform.position, new Color(0.3f, 0.9f, 0.3f));
    }
}
