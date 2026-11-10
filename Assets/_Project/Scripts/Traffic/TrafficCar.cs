using UnityEngine;

namespace UncannyValet.Traffic
{
    /// <summary>A visual-only car: one sprite, no collider, no body. Pooled and shown or hidden by the traffic module.</summary>
    [RequireComponent(typeof(SpriteRenderer))]
    [DisallowMultipleComponent]
    public sealed class TrafficCar : MonoBehaviour
    {
        private SpriteRenderer _renderer;
        private Transform _transform;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _transform = transform;
        }

        /// <summary>Makes the car visible with a look.</summary>
        public void Show(Sprite sprite, Color tint, int sortingOrder, float scale)
        {
            _renderer.sprite = sprite;
            _renderer.color = tint;
            _renderer.sortingOrder = sortingOrder;
            _transform.localScale = new Vector3(scale, scale, 1f);
            gameObject.SetActive(true);
        }

        /// <summary>Puts the car at a position, facing <paramref name="headingDegrees"/> (0 = nose up).</summary>
        public void Place(Vector2 position, float headingDegrees)
        {
            _transform.SetPositionAndRotation(new Vector3(position.x, position.y, 0f), Quaternion.Euler(0f, 0f, headingDegrees));
        }

        /// <summary>Hides the car.</summary>
        public void Hide() => gameObject.SetActive(false);
    }
}
