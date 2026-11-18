using UnityEngine;

namespace UncannyValet.Lot.Handmade
{
    /// <summary>Where the valet stands when the night starts. One per lot, not inside a solid.</summary>
    [DisallowMultipleComponent]
    public sealed class ValetStart : MonoBehaviour
    {
        private void OnDrawGizmos()
        {
            MarkerGizmos.Dot(transform.position, Color.yellow);
            MarkerGizmos.Label(transform.position, "Valet start");
        }
    }
}
