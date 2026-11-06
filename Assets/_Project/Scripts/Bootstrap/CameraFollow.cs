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
        /// <summary>Half the visible height in world units. 11 shows the curb and queue slots 0 and 1 together.</summary>
        public const float DefaultOrthographicSize = 11f;

        [SerializeField, Min(1f)] private float _orthographicSize = DefaultOrthographicSize;
        [SerializeField, Min(0f)] private float _smoothing = 8f;

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
            var goal = new Vector3(_target.position.x, _target.position.y, transform.position.z);
            var t = _smoothing <= 0f ? 1f : 1f - Mathf.Exp(-_smoothing * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, goal, t);
        }

        private void Snap()
        {
            transform.position = new Vector3(_target.position.x, _target.position.y, transform.position.z);
        }
    }
}
