using UnityEngine;

namespace UncannyValet.Valet
{
    /// <summary>
    /// A fixed pool of pixel lightning streaks drawn in world space with one cached white pixel sprite: each streak is a few
    /// jagged bars and a couple of spark puffs that fade out. Nothing is allocated after construction.
    /// </summary>
    public sealed class ThunderStreaks : System.IDisposable
    {
        private const int Slots = 24;
        private const int Bars = 3;
        private const int Sparks = 2;
        private const float PixelsPerUnit = 16f;
        private const float Lifetime = 0.3f;
        private const float BarThickness = 3f / PixelsPerUnit;
        private const float SparkSize = 3f / PixelsPerUnit;
        private const float Jag = 0.35f;
        private const float SparkSpread = 0.5f;
        private const int SortingOrder = 40;
        private static readonly Color Core = new Color(1f, 0.98f, 0.7f, 1f);
        private static readonly Color Spark = new Color(0.7f, 0.9f, 1f, 1f);

        private readonly SpriteRenderer[] _renderers = new SpriteRenderer[Slots * (Bars + Sparks)];
        private readonly float[] _born = new float[Slots];
        private readonly bool[] _shown = new bool[Slots];
        private readonly Vector2[] _points = new Vector2[Bars + 1];
        private readonly float[] _offsets = new float[Bars - 1];
        private readonly GameObject _root;
        private readonly Sprite _pixel;
        private int _next;

        public ThunderStreaks(Transform parent)
        {
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            _pixel = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            _root = new GameObject("Thunder streaks");
            if (parent != null) _root.transform.SetParent(parent, false);
            for (var i = 0; i < _renderers.Length; i++)
            {
                var go = new GameObject("Streak piece");
                go.transform.SetParent(_root.transform, false);
                _renderers[i] = go.AddComponent<SpriteRenderer>();
                _renderers[i].sprite = _pixel;
                _renderers[i].sortingOrder = SortingOrder;
                _renderers[i].enabled = false;
            }
            for (var i = 0; i < Slots; i++) _born[i] = float.NegativeInfinity;
        }

        /// <summary>Draws one jagged streak between two points, born at <paramref name="now"/> (seconds).</summary>
        public void Emit(Vector2 from, Vector2 to, float now)
        {
            var slot = _next;
            _next = (_next + 1) % Slots;
            _born[slot] = now;
            for (var i = 0; i < _offsets.Length; i++) _offsets[i] = Random.Range(-1f, 1f);
            BoltShape.Layout(from, to, Jag, _offsets, PixelsPerUnit, _points);
            var first = slot * (Bars + Sparks);
            for (var b = 0; b < Bars; b++) Place(_renderers[first + b], _points[b], _points[b + 1], BarThickness);
            for (var s = 0; s < Sparks; s++) PlaceSpark(_renderers[first + Bars + s], Vector2.Lerp(from, to, Random.value));
            Show(slot, 1f);
        }

        /// <summary>Fades the streaks and hides the ones that ran out.</summary>
        public void Tick(float now)
        {
            for (var slot = 0; slot < Slots; slot++)
            {
                var age = now - _born[slot];
                if (age < 0f || age > Lifetime) { if (_shown[slot]) Hide(slot); continue; }
                Show(slot, BoltShape.Fade(age, Lifetime));
            }
        }

        /// <summary>Removes the streaks and the sprite they share.</summary>
        public void Dispose()
        {
            if (_root != null) Object.Destroy(_root);
            if (_pixel != null) { Object.Destroy(_pixel.texture); Object.Destroy(_pixel); }
        }

        private static void Place(SpriteRenderer piece, Vector2 a, Vector2 b, float thickness)
        {
            var delta = b - a;
            piece.transform.SetPositionAndRotation((a + b) * 0.5f, Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg));
            piece.transform.localScale = new Vector3(Mathf.Max(delta.magnitude, thickness), thickness, 1f);
        }

        private static void PlaceSpark(SpriteRenderer piece, Vector2 at)
        {
            var spot = BoltShape.Snap(at + Random.insideUnitCircle * SparkSpread, PixelsPerUnit);
            piece.transform.SetPositionAndRotation(spot, Quaternion.identity);
            piece.transform.localScale = new Vector3(SparkSize, SparkSize, 1f);
        }

        private void Show(int slot, float alpha)
        {
            _shown[slot] = true;
            var first = slot * (Bars + Sparks);
            for (var i = 0; i < Bars + Sparks; i++)
            {
                var color = i < Bars ? Core : Spark;
                color.a = alpha;
                _renderers[first + i].color = color;
                _renderers[first + i].enabled = true;
            }
        }

        private void Hide(int slot)
        {
            _shown[slot] = false;
            var first = slot * (Bars + Sparks);
            for (var i = 0; i < Bars + Sparks; i++) _renderers[first + i].enabled = false;
        }
    }
}
