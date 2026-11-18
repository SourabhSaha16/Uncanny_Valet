using UnityEngine;

namespace UncannyValet.Lot.Handmade
{
    /// <summary>A place where a patron stands while waiting for the car. Patrons fill the points in order.</summary>
    [DisallowMultipleComponent]
    public sealed class PatronWaitPoint : MonoBehaviour
    {
        [SerializeField, Tooltip("Order in which patrons take the points; lowest first.")] private int _order;

        public int Order => _order;

        private void OnDrawGizmos()
        {
            MarkerGizmos.Dot(transform.position, new Color(0.4f, 1f, 0.5f));
            MarkerGizmos.Label(transform.position, "Wait " + _order);
        }
    }
}
