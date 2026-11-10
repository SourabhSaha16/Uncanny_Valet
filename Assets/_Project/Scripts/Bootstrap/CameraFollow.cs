using UnityEngine;

namespace UncannyValet.Bootstrap
{
    /// <summary>
    /// Keeps an orthographic camera on a target. The valet's transform tracks the car he is driving,
    /// so following the valet also follows the car. When world bounds are set, the zoom is chosen so the
    /// whole lot fits (within a legibility cap) and the view never leaves the bounds.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public sealed class CameraFollow : MonoBehaviour
    {
        /// <summary>Half the visible height in world units used until bounds are known.</summary>
        public const float DefaultOrthographicSize = 8.5f;

        [SerializeField] private Transform _target;
        [SerializeField, Min(1f), Tooltip("Smallest zoom (half the visible height). Also the zoom used when no bounds are set.")]
        private float _orthographicSize = DefaultOrthographicSize;
        [SerializeField, Min(1f), Tooltip("Largest zoom when fitting the lot. Beyond this the camera follows instead of showing everything.")]
        private float _maxOrthographicSize = 15f;
        [SerializeField, Min(0f), Tooltip("World units of extra room around the lot bounds when fitting.")]
        private float _boundsMargin = 1f;
        [SerializeField, Range(0.5f, 1f), Tooltip("Zooms in on the fitted view by this factor (0.87 = about 15% closer). The play area is never cropped.")]
        private float _fitTightness = 0.87f;
        [SerializeField, Min(0f)] private float _smoothing = 8f;
        [SerializeField, Tooltip("Shifts the view from the target when no bounds are set.")]
        private Vector2 _focusOffset = Vector2.zero;

        private Camera _camera;

        // GameBootstrap.Awake can run before this Awake (scene order), so the camera is cached on first use.
        private Camera Cam
        {
            get
            {
                if (_camera != null) return _camera;
                _camera = GetComponent<Camera>();
                _camera.orthographic = true;
                return _camera;
            }
        }
        private Rect _bounds;
        private Rect _mustShow;
        private bool _hasBounds;
        private float _aspect;

        /// <summary>Follows this transform; snaps to it immediately.</summary>
        public void SetTarget(Transform target)
        {
            _target = target;
            if (target != null) Snap();
        }

        /// <summary>Sets the world rectangle of the lot (walls included) and refits the zoom.</summary>
        public void SetWorldBounds(Rect bounds) => SetWorldBounds(bounds, bounds);

        /// <summary>Sets the view bounds and the play area that must stay fully in frame when zooming in.</summary>
        public void SetWorldBounds(Rect bounds, Rect mustShow)
        {
            _bounds = bounds;
            _mustShow = mustShow;
            _hasBounds = true;
            ApplySize();
            if (_target != null) Snap();
        }

        private void Awake()
        {
            ApplySize();
        }

        private void LateUpdate()
        {
            if (!Mathf.Approximately(_aspect, Cam.aspect)) ApplySize();
            if (_target == null) return;
            var t = _smoothing <= 0f ? 1f : 1f - Mathf.Exp(-_smoothing * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, Goal(), t);
        }

        private void Snap() => transform.position = Goal();

        private void ApplySize()
        {
            _aspect = Cam.aspect;
            var size = _orthographicSize;
            if (_hasBounds)
            {
                size = CameraFit.Size(_bounds, _mustShow, _aspect, _boundsMargin, _fitTightness,
                    _orthographicSize, _maxOrthographicSize);
            }
            Cam.orthographicSize = size;
        }

        private Vector3 Goal()
        {
            var p = (Vector2)_target.position;
            if (!_hasBounds) p += _focusOffset;
            else
            {
                var half = new Vector2(Cam.orthographicSize * Cam.aspect, Cam.orthographicSize);
                p.x = Clamp(p.x, _bounds.xMin - _boundsMargin, _bounds.xMax + _boundsMargin, half.x);
                p.y = Clamp(p.y, _bounds.yMin - _boundsMargin, _bounds.yMax + _boundsMargin, half.y);
            }
            return new Vector3(p.x, p.y, transform.position.z);
        }

        private static float Clamp(float v, float min, float max, float half)
        {
            if (max - min <= half * 2f) return (min + max) * 0.5f;
            return Mathf.Clamp(v, min + half, max - half);
        }
    }
}
