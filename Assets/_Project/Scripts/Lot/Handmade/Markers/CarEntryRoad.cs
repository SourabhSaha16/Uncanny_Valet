using UnityEngine;

namespace UncannyValet.Lot.Handmade
{
    /// <summary>The way patron cars drive in. All points are in this object's local space.</summary>
    [DisallowMultipleComponent]
    public sealed class CarEntryRoad : MonoBehaviour
    {
        [SerializeField, Tooltip("The lane centre line in travel order. The first point must be off screen.")]
        private Vector2[] _lane = { new Vector2(-30f, 0f), new Vector2(30f, 0f) };
        [SerializeField, Tooltip("The point on the lane where a car turns towards the queue.")]
        private Vector2 _queueTurn = new Vector2(0f, 0f);
        [SerializeField, Tooltip("Points from the turn towards queue slot 0. The last one must be in line with the slots; the route then runs straight into the slot.")]
        private Vector2[] _bend = { new Vector2(0f, 3f) };

        public Vector2[] Lane => _lane;
        public Vector2 QueueTurn => _queueTurn;
        public Vector2[] Bend => _bend;

        private void OnDrawGizmos()
        {
            MarkerGizmos.Path(transform, _lane, new Color(0.3f, 0.8f, 1f));
            MarkerGizmos.Dot(transform.TransformPoint(_queueTurn), Color.green);
            var turn = new Vector2[_bend.Length + 1];
            turn[0] = _queueTurn;
            for (var i = 0; i < _bend.Length; i++) turn[i + 1] = _bend[i];
            MarkerGizmos.Path(transform, turn, Color.green);
            MarkerGizmos.Label(transform.TransformPoint(_lane[0]), "Car entry");
        }
    }
}
