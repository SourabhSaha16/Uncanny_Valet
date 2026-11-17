using UnityEngine;

namespace UncannyValet.Cars.Fx
{
    /// <summary>
    /// A small pool of fading ground decals (skid marks now, blood tracks later with a tint). Every decal is one
    /// stretched copy of a shared 1x1 sprite; colours and alpha are per renderer, never a material copy.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class DecalPool : MonoBehaviour
    {
        private DecalLedger _ledger;
        private SpriteRenderer[] _renderers;
        private Color[] _tints;
        private bool[] _shown;

        /// <summary>Decals alive now.</summary>
        public int ActiveCount => _ledger != null ? _ledger.ActiveCount : 0;
        /// <summary>Most decals alive at once.</summary>
        public int Capacity => _ledger != null ? _ledger.Capacity : 0;

        /// <summary>Creates a pool as a child object of <paramref name="parent"/>.</summary>
        public static DecalPool Create(Transform parent, int capacity, Sprite sprite, int sortingOrder)
        {
            var go = new GameObject("Decals");
            go.transform.SetParent(parent, false);
            var pool = go.AddComponent<DecalPool>();
            pool.Initialize(capacity, sprite, sortingOrder);
            return pool;
        }

        /// <param name="sprite">A 1x1 white sprite (pixels per unit 1) shared by every decal.</param>
        public void Initialize(int capacity, Sprite sprite, int sortingOrder)
        {
            _ledger = new DecalLedger(capacity);
            _renderers = new SpriteRenderer[capacity];
            _tints = new Color[capacity];
            _shown = new bool[capacity];
            for (var i = 0; i < capacity; i++)
            {
                var go = new GameObject("Decal");
                go.transform.SetParent(transform, false);
                var renderer = go.AddComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                renderer.sortingOrder = sortingOrder;
                renderer.enabled = false;
                _renderers[i] = renderer;
            }
        }

        /// <summary>
        /// Puts a stroke at <paramref name="center"/>: <paramref name="length"/> along the direction the local up axis
        /// points at <paramref name="angleDegrees"/>, <paramref name="thickness"/> across. The decal lives <paramref name="fadeSeconds"/> and fades over its last <paramref name="fadeTailSeconds"/>. The oldest decal is reused when full.
        /// </summary>
        public void Place(Vector2 center, float angleDegrees, float length, float thickness, Color tint, float opacity, float fadeSeconds,
            float fadeTailSeconds = float.MaxValue)
        {
            if (_ledger == null)
            {
                Debug.LogError("DecalPool on " + name + " was not initialized.", this);
                return;
            }
            var slot = _ledger.Acquire(fadeSeconds, opacity, fadeTailSeconds);
            var t = _renderers[slot].transform;
            t.SetPositionAndRotation(new Vector3(center.x, center.y, 0f), Quaternion.Euler(0f, 0f, angleDegrees));
            t.localScale = new Vector3(thickness, length, 1f);
            _tints[slot] = tint;
            Paint(slot);
        }

        /// <summary>Ages the decals by <paramref name="dt"/> seconds. Public so tests can drive it.</summary>
        public void Tick(float dt)
        {
            if (_ledger == null) return;
            _ledger.Tick(dt);
            for (var i = 0; i < _renderers.Length; i++)
            {
                if (_ledger.IsActive(i)) Paint(i);
                else if (_shown[i]) Hide(i);
            }
        }

        private void Update() => Tick(Time.deltaTime);

        private void Hide(int slot)
        {
            _renderers[slot].enabled = false;
            _shown[slot] = false;
        }

        private void Paint(int slot)
        {
            _shown[slot] = true;
            var tint = _tints[slot];
            _renderers[slot].color = new Color(tint.r, tint.g, tint.b, _ledger.Alpha(slot));
            _renderers[slot].enabled = true;
        }
    }
}
