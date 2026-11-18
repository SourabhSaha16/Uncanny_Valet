using UnityEngine;

namespace UncannyValet.Lot.Handmade
{
    /// <summary>A door where people vanish into the building and appear from it. The main door is for patrons, the side door for ambient people.</summary>
    [DisallowMultipleComponent]
    public sealed class LotDoor : MonoBehaviour
    {
        [SerializeField, Tooltip("Who uses the door.")] private DoorKind _kind;

        public DoorKind Kind => _kind;

        private void OnDrawGizmos()
        {
            MarkerGizmos.Dot(transform.position, _kind == DoorKind.Main ? new Color(1f, 0.85f, 0.2f) : new Color(0.7f, 0.7f, 0.7f));
            MarkerGizmos.Label(transform.position, _kind + " door");
        }
    }
}
