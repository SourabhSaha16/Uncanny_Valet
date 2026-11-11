using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UnityEngine;

namespace UncannyValet.Valet
{
    /// <summary>
    /// What the speed boost looks like: a rainbow pixel outline around the valet (four tinted copies of the outfit sprite one
    /// pixel out, colours cycling; it blinks in the last second) and a sparkle trail that stays where it was left. No art files, no allocation per frame.
    /// </summary>
    [DisallowMultipleComponent, RequireComponent(typeof(ValetBoost), typeof(Rigidbody2D))]
    public sealed class ValetBoostFx : MonoBehaviour
    {
        [SerializeField, Tooltip("The sprite layer whose shape is outlined (the outfit layer).")] private SpriteRenderer _shape;
        [SerializeField, Min(0.5f), Tooltip("How many source pixels out the outline sits.")] private float _outlinePixels = 1.5f;
        [SerializeField, Min(0.1f), Tooltip("Rainbow turns per second.")] private float _cyclesPerSecond = 1.5f;
        [SerializeField, Min(0f), Tooltip("Seconds before the end when the outline starts to blink.")] private float _warnSeconds = 1f;
        [SerializeField, Min(1f), Tooltip("Blinks per second in the warning.")] private float _blinksPerSecond = 6f;
        [SerializeField, Range(4, 64), Tooltip("Most sparkles alive at once.")] private int _sparkles = 32;
        [SerializeField, Min(0.1f), Tooltip("Seconds a sparkle lives.")] private float _sparkleSeconds = 0.5f;
        [SerializeField, Min(0f), Tooltip("Sparkles per second when the F2 value is missing.")] private float _fallbackRate = 24f;
        [SerializeField, Min(0f), Tooltip("Valet speed above which the trail is left.")] private float _moveThreshold = 0.2f;
        [SerializeField, Min(0f), Tooltip("How far around the valet's feet a sparkle may appear.")] private float _spread = 0.3f;
        [SerializeField, Min(0f), Tooltip("Drift speed of a sparkle.")] private float _drift = 0.5f;

        private static readonly Vector2[] Directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };

        private readonly TrailEmitter _emitter = new TrailEmitter();
        private SpriteRenderer[] _outline;
        private SparklePool _pool;
        private GameObject _trailRoot;
        private Texture2D _texture;
        private Sprite _sprite;
        private ValetBoost _boost;
        private Rigidbody2D _body;
        private ITuning _tuning;

        private void Awake()
        {
            if (_shape == null) { Debug.LogError("ValetBoostFx on " + name + " has no shape renderer; rebuild with Setup/Build Everything.", this); enabled = false; return; }
            _boost = GetComponent<ValetBoost>();
            _body = GetComponent<Rigidbody2D>();
            _outline = new SpriteRenderer[Directions.Length];
            for (var i = 0; i < _outline.Length; i++) _outline[i] = NewOutline(i);
            _sprite = SparkleSprite.Create(out _texture);
            _trailRoot = new GameObject("ValetSparkles");
            _pool = new SparklePool(_trailRoot.transform, _sprite, _sparkles, _shape.sortingOrder + 2);
        }

        private void Start() => Services.TryGet(out _tuning);

        private void OnDestroy()
        {
            if (_trailRoot != null) Destroy(_trailRoot);
            if (_sprite != null) Destroy(_sprite);
            if (_texture != null) Destroy(_texture);
        }

        private void LateUpdate()
        {
            var on = _boost.IsBoosted && _shape.enabled;
            var time = Time.time;
            var visible = on && BoostMath.OutlineVisible(_boost.Remaining, _warnSeconds, time, _blinksPerSecond);
            ShowOutline(visible, time);
            Trail(on, Time.deltaTime);
            _pool.Step(Time.deltaTime);
        }

        private void ShowOutline(bool visible, float time)
        {
            var step = 1f / Mathf.Max(1f, _shape.sprite != null ? _shape.sprite.pixelsPerUnit : 1f);
            for (var i = 0; i < _outline.Length; i++)
            {
                var layer = _outline[i];
                layer.enabled = visible;
                if (!visible) continue;
                layer.sprite = _shape.sprite;
                layer.flipX = _shape.flipX;
                layer.transform.localPosition = Directions[i] * (_outlinePixels * step);
                layer.color = BoostMath.Rainbow(BoostMath.Hue(time + i / (float)_outline.Length, _cyclesPerSecond));
            }
        }

        private void Trail(bool on, float delta)
        {
            var moving = _body.linearVelocity.sqrMagnitude >= _moveThreshold * _moveThreshold;
            if (!on || !moving) { _emitter.Reset(); return; }
            var rate = _tuning != null && _tuning.TryGet(TuningKey.PowerTrailPerSecond, out var tuned) ? tuned : _fallbackRate;
            for (var n = _emitter.Step(delta, rate); n > 0; n--)
            {
                var at = _body.position + Random.insideUnitCircle * _spread;
                _pool.Emit(at, Random.insideUnitCircle * _drift, _sparkleSeconds, BoostMath.Rainbow(Random.value));
            }
        }

        private SpriteRenderer NewOutline(int index)
        {
            var go = new GameObject("BoostOutline" + index);
            go.transform.SetParent(_shape.transform.parent, false);
            var layer = go.AddComponent<SpriteRenderer>();
            layer.sortingLayerID = _shape.sortingLayerID;
            layer.sortingOrder = _shape.sortingOrder - 2;
            layer.enabled = false;
            return layer;
        }
    }
}
