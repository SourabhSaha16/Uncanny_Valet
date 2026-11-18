using UnityEngine;

namespace UncannyValet.Lot.Handmade
{
    /// <summary>A lane for ambient traffic, beside the entry or exit road and running the other way. Points are in local space, in travel order.</summary>
    [DisallowMultipleComponent]
    public sealed class AmbientLane : MonoBehaviour
    {
        [SerializeField, Tooltip("Which road this lane lies beside.")] private AmbientLaneSide _side;
        [SerializeField, Tooltip("Lane centre line in travel order, starting and ending off screen.")]
        private Vector2[] _points = { new Vector2(30f, 0f), new Vector2(-30f, 0f) };

        public AmbientLaneSide Side => _side;
        public Vector2[] Points => _points;

        private void OnDrawGizmos()
        {
            MarkerGizmos.Path(transform, _points, new Color(0.6f, 0.6f, 0.7f));
            MarkerGizmos.Label(transform.TransformPoint(_points[0]), "Ambient " + _side);
        }
    }
}
