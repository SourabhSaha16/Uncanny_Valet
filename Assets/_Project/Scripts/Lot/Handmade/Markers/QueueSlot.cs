using UnityEngine;

namespace UncannyValet.Lot.Handmade
{
    /// <summary>A place where a waiting car stops. Index 0 is the valet pickup spot touching the curb; later slots lie further back. All slots share one rotation.</summary>
    [DisallowMultipleComponent]
    public sealed class QueueSlot : MonoBehaviour
    {
        [SerializeField, Min(0), Tooltip("Position in the queue: 0 is nearest the curb, then 1, 2 ... with no gaps.")] private int _index;
        [SerializeField, Tooltip("Footprint of a waiting car (width, length).")] private Vector2 _carSize = new Vector2(2.2f, 4.5f);

        public int Index => _index;
        public Vector2 CarSize => _carSize;

        private void OnDrawGizmos()
        {
            MarkerGizmos.Box(transform, _carSize, _index == 0 ? new Color(1f, 0.85f, 0.2f) : Color.white);
            MarkerGizmos.Label(transform.position, "Queue " + _index);
        }
    }
}
