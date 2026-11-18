using UnityEngine;

namespace UncannyValet.Lot.Handmade
{
    /// <summary>A mud patch: cars slow down in it. Round when the height is 0, otherwise a rectangle.</summary>
    [DisallowMultipleComponent]
    public sealed class HazardPatch : MonoBehaviour
    {
        [SerializeField, Min(0.5f), Tooltip("Width of the patch, or the diameter of a round one.")] private float _size = 3f;
        [SerializeField, Min(0f), Tooltip("Height of a rectangular patch. 0 makes the patch a round puddle.")] private float _height;

        public float Size => _size;
        public float Height => _height;

        private void OnDrawGizmos()
        {
            var color = new Color(0.5f, 0.35f, 0.15f);
            MarkerGizmos.Box(transform, new Vector2(_size, _height > 0f ? _height : _size), color);
            MarkerGizmos.Label(transform.position, "Mud");
        }
    }
}
