using UnityEngine;

namespace UncannyValet.Valet
{
    /// <summary>A fixed set of sprite renderers reused as short-lived sparkles that drift, shrink and fade. Creates everything once; allocates nothing afterwards.</summary>
    public sealed class SparklePool
    {
        private readonly SpriteRenderer[] _renderers;
        private readonly Vector2[] _velocity;
        private readonly Color[] _color;
        private readonly float[] _age;
        private readonly float[] _life;
        private int _next;

        public SparklePool(Transform parent, Sprite sprite, int size, int sortingOrder)
        {
            _renderers = new SpriteRenderer[size];
            _velocity = new Vector2[size];
            _color = new Color[size];
            _age = new float[size];
            _life = new float[size];
            for (var i = 0; i < size; i++)
            {
                var go = new GameObject("Sparkle");
                go.transform.SetParent(parent, false);
                _renderers[i] = go.AddComponent<SpriteRenderer>();
                _renderers[i].sprite = sprite;
                _renderers[i].sortingOrder = sortingOrder;
                _renderers[i].enabled = false;
                _life[i] = 1f;
                _age[i] = 1f;
            }
        }

        /// <summary>Starts a sparkle (replacing the oldest one when all are busy).</summary>
        public void Emit(Vector2 position, Vector2 velocity, float lifeSeconds, Color color)
        {
            var i = _next;
            _next = (_next + 1) % _renderers.Length;
            _renderers[i].transform.position = position;
            _renderers[i].enabled = true;
            _velocity[i] = velocity;
            _color[i] = color;
            _age[i] = 0f;
            _life[i] = Mathf.Max(0.01f, lifeSeconds);
            Apply(i);
        }

        /// <summary>Moves, shrinks and fades every live sparkle.</summary>
        public void Step(float deltaSeconds)
        {
            for (var i = 0; i < _renderers.Length; i++)
            {
                if (!_renderers[i].enabled) continue;
                _age[i] += deltaSeconds;
                if (_age[i] >= _life[i]) { _renderers[i].enabled = false; continue; }
                _renderers[i].transform.position += (Vector3)(_velocity[i] * deltaSeconds);
                Apply(i);
            }
        }

        private void Apply(int i)
        {
            var remaining = 1f - _age[i] / _life[i];
            var c = _color[i];
            c.a = remaining;
            _renderers[i].color = c;
            _renderers[i].transform.localScale = Vector3.one * remaining;
        }
    }
}
