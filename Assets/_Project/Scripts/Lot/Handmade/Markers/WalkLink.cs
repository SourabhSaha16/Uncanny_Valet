using UnityEngine;

namespace UncannyValet.Lot.Handmade
{
    /// <summary>A walkable link between two walk nodes. A link must not pass through a solid.</summary>
    [DisallowMultipleComponent]
    public sealed class WalkLink : MonoBehaviour
    {
        [SerializeField] private WalkNode _a;
        [SerializeField] private WalkNode _b;
        [SerializeField, Tooltip("True when the link is a painted crosswalk.")] private bool _crosswalk;

        public WalkNode A => _a;
        public WalkNode B => _b;
        public bool Crosswalk => _crosswalk;

        private void OnDrawGizmos()
        {
            if (_a == null || _b == null) return;
            Gizmos.color = _crosswalk ? Color.white : new Color(0.3f, 0.9f, 0.3f);
            Gizmos.DrawLine(_a.transform.position, _b.transform.position);
        }
    }
}
