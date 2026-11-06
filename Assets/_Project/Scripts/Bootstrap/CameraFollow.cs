using UnityEngine;

namespace UncannyValet.Bootstrap
{
    /// <summary>
    /// Keeps an orthographic camera on a target. The valet's transform tracks the car he is driving,
    /// so following the valet also follows the car.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public sealed class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        /// <summary>Half the visible height in world units. 8.5 at 16:9 shows the queue below, the curb and the stations above from the valet start.</summary>
        public const float DefaultOrthographicSize = 8.5f;

        [SerializeField, Min(1f)] private float _orthographicSize = DefaultOrthographicSize;
        [SerializeField, Min(0f)] private float _smoothing = 8f;
        [SerializeField, Tooltip("Shifts the view from the target. At the valet start (y 0) a y offset between -0.3 and 0.2 keeps both the queue cars (down to y -8.3) and the stations (up to y 8.2) in view.")]
        private Vector2 _focusOffset = Vector2.zero;

        public void SetTarget(Transform target)
        {
            _target = target;
            if (target != null) Snap();
        }

        private void Awake()
        {
            var cam = GetComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = _orthographicSize;
        }

        private void LateUpdate()
        {
            if (_target == null) return;
            var goal = Goal();
            var t = _smoothing <= 0f ? 1f : 1f - Mathf.Exp(-_smoothing * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, goal, t);
        }

        private void Snap()
        {
            transform.position = Goal();
        }

        private Vector3 Goal()
        {
            return new Vector3(_target.position.x + _focusOffset.x, _target.position.y + _focusOffset.y, transform.position.z);
        }
    }
}
