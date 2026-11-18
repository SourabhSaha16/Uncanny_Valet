using UnityEngine;

namespace UncannyValet.Lot.Handmade
{
    /// <summary>An invisible solid box (a wall, a hedge, a building): cars and the valet cannot pass. Its size is the BoxCollider2D's.</summary>
    [RequireComponent(typeof(BoxCollider2D))]
    [DisallowMultipleComponent]
    public sealed class SolidBlock : MonoBehaviour
    {
        private void OnDrawGizmos()
        {
            var box = GetComponent<BoxCollider2D>();
            if (box == null) return;
            MarkerGizmos.Box(transform, Vector2.Scale(box.size, transform.lossyScale), new Color(0.85f, 0.3f, 0.3f));
        }
    }
}
