using UnityEngine;

namespace UncannyValet.Lot.Handmade
{
    /// <summary>A parking bay: a regular numbered bay, an EV charging bay or a wash bay. Position and rotation (z) come from the transform.</summary>
    [DisallowMultipleComponent]
    public sealed class LotBay : MonoBehaviour
    {
        [SerializeField, Tooltip("What the bay is for.")] private BayKind _kind;
        [SerializeField, Min(1), Tooltip("Number among bays of this kind, from 1. Regular bays open in this order as the night grows; charging bays follow the regular ones.")] private int _number = 1;
        [SerializeField, Tooltip("Width and length of the bay before rotation.")] private Vector2 _size = new Vector2(2.6f, 4.4f);

        public BayKind Kind => _kind;
        public int Number => _number;
        public Vector2 Size => _size;

        private void OnDrawGizmos()
        {
            var color = _kind == BayKind.Charging ? new Color(0.3f, 0.6f, 1f) : _kind == BayKind.Wash ? new Color(0.4f, 0.95f, 0.9f) : new Color(0.95f, 0.95f, 0.85f);
            MarkerGizmos.Box(transform, _size, color);
            MarkerGizmos.Label(transform.position, _kind + " " + _number);
        }
    }
}
