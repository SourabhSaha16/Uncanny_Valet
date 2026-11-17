using UncannyValet.Core.Contracts;
using UnityEngine;

namespace UncannyValet.UI.Pie
{
    /// <summary>One bay outline in the world. Pooled by <see cref="BayBorderLayer"/>; tinted with the renderer colour, never a material.</summary>
    public sealed class BayBorderView
    {
        private readonly GameObject _root;
        private readonly SpriteRenderer _renderer;

        public BayBorderView(Transform parent, int sortingOrder)
        {
            _root = new GameObject("Bay Border");
            _root.transform.SetParent(parent, false);
            _renderer = _root.AddComponent<SpriteRenderer>();
            _renderer.sortingOrder = sortingOrder;
            _root.SetActive(false);
        }

        /// <summary>Puts the border on the bay with the sprite (call once when taken from the pool).</summary>
        public void Place(SpotBounds bay, Sprite sprite)
        {
            _renderer.sprite = sprite;
            _root.transform.SetPositionAndRotation(bay.Center, Quaternion.Euler(0f, 0f, bay.RotationDegrees));
        }

        /// <summary>Shows or hides the border (hidden during the dark half of a blink) and sets its colour.</summary>
        public void Show(bool lit, Color color)
        {
            if (_root.activeSelf != lit) _root.SetActive(lit);
            _renderer.color = color;
        }

        public void Hide() { if (_root.activeSelf) _root.SetActive(false); }

        public void Destroy() => PieObjects.Release(_root);
    }
}
