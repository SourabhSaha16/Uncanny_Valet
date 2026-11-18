using UnityEngine;

namespace UncannyValet.Lot.Handmade
{
    /// <summary>The way delivered cars drive out. All points are in this object's local space.</summary>
    [DisallowMultipleComponent]
    public sealed class CarExitRoad : MonoBehaviour
    {
        [SerializeField, Tooltip("The lane centre line in travel order. The last point must be off screen.")]
        private Vector2[] _lane = { new Vector2(-30f, 0f), new Vector2(30f, 0f) };
        [SerializeField, Tooltip("The route from inside the curb zone to the merge point. The first point is in the curb, the last one on the lane.")]
        private Vector2[] _curbExit = { new Vector2(0f, -8f), new Vector2(0f, -2f), new Vector2(2f, 0f) };

        public Vector2[] Lane => _lane;
        public Vector2[] CurbExit => _curbExit;

        private void OnDrawGizmos()
        {
            MarkerGizmos.Path(transform, _lane, new Color(1f, 0.55f, 0.2f));
            MarkerGizmos.Path(transform, _curbExit, Color.green);
            MarkerGizmos.Label(transform.TransformPoint(_lane[0]), "Car exit");
        }
    }
}
