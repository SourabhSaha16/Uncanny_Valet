using UnityEngine;

namespace UncannyValet.Lot.Handmade
{
    /// <summary>Where ambient people appear (and vanish) at the edge of the lot, off the road side. They walk from here to the side door.</summary>
    [DisallowMultipleComponent]
    public sealed class AmbientSpawnPoint : MonoBehaviour
    {
        private void OnDrawGizmos()
        {
            MarkerGizmos.Dot(transform.position, new Color(0.8f, 0.5f, 1f));
            MarkerGizmos.Label(transform.position, "Ambient spawn");
        }
    }
}
