using UnityEngine;

namespace UncannyValet.Lot.Handmade
{
    /// <summary>The valet curb: where delivered cars are handed back. One per lot, not rotated (cars are judged facing up the screen).</summary>
    [DisallowMultipleComponent]
    public sealed class CurbZoneMarker : MonoBehaviour
    {
        [SerializeField, Tooltip("Width and height of the curb zone.")] private Vector2 _size = new Vector2(3f, 5f);

        public Vector2 Size => _size;

        private void OnDrawGizmos()
        {
            MarkerGizmos.Box(transform, _size, new Color(0.95f, 0.78f, 0.15f));
            MarkerGizmos.Label(transform.position, "Curb");
        }
    }
}
