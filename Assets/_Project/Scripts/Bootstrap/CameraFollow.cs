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
        [SerializeField, Range(0.5f, 1f), Tooltip("Zooms in on the fitted view by this factor (0.78 = about 22% closer). The play area is never cropped.")]
        private float _fitTightness = 0.78f;
        [SerializeField, Min(0f)] private float _smoothing = 8f;
        [SerializeField, Range(0f, 0.5f), Tooltip("Seconds the Perfect Park camera kick lasts (out and back).")]
        private float _kickSeconds = 0.08f;
        [SerializeField, Range(0f, 0.5f), Tooltip("World units the Perfect Park camera kick moves the view.")]
        private float _kickUnits = 0.05f;
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
        private Vector2 _basePosition;
        private CameraKick _kick;

        /// <summary>Follows this transform; snaps to it immediately.</summary>
        public void SetTarget(Transform target)
        {
            _target = target;
            if (target != null) Snap();
        }

        /// <summary>Nudges the view along a direction for a moment (the Perfect Park kick). Length and time are serialized.</summary>
        public void Kick(Vector2 direction) => _kick.Start(direction, _kickUnits, _kickSeconds, Time.unscaledTime);

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
            _basePosition = transform.position;
        }

        private void LateUpdate()
        {
            if (!Mathf.Approximately(_aspect, Cam.aspect)) ApplySize();
            if (_target == null) return;
            var t = _smoothing <= 0f ? 1f : 1f - Mathf.Exp(-_smoothing * Time.deltaTime);
            _basePosition = Vector2.Lerp(_basePosition, Goal(), t);
            Place();
        }

        private void Snap()
        {
            _basePosition = Goal();
            Place();
        }

        // The follow position is kept apart from the kick, so the smoothing never chases the nudge.
        private void Place()
        {
            var at = _basePosition + _kick.Offset(Time.unscaledTime);
            transform.position = new Vector3(at.x, at.y, transform.position.z);
        }

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

        private Vector2 Goal()
        {
            var p = (Vector2)_target.position;
            if (!_hasBounds) p += _focusOffset;
            else
            {
                var half = new Vector2(Cam.orthographicSize * Cam.aspect, Cam.orthographicSize);
                p.x = Clamp(p.x, _bounds.xMin - _boundsMargin, _bounds.xMax + _boundsMargin, half.x);
                p.y = Clamp(p.y, _bounds.yMin - _boundsMargin, _bounds.yMax + _boundsMargin, half.y);
            }
            return p;
        }

        private static float Clamp(float v, float min, float max, float half)
        {
            if (max - min <= half * 2f) return (min + max) * 0.5f;
            return Mathf.Clamp(v, min + half, max - half);
        }
    }
}
