using UnityEngine;

namespace UncannyValet.UI.Pie
{
    /// <summary>One pie in the world: a dark back disc and a tinted wedge on top. Pooled by <see cref="PickupPieLayer"/>.</summary>
    public sealed class PickupPieView
    {
        private readonly GameObject _root;
        private readonly SpriteRenderer _wedge;
        private int _step = -1;

        public PickupPieView(Transform parent, Sprite back, int sortingOrder)
        {
            _root = new GameObject("Pickup Pie");
            _root.transform.SetParent(parent, false);
            var disc = _root.AddComponent<SpriteRenderer>();
            disc.sprite = back;
            disc.sortingOrder = sortingOrder;
            var wedgeObject = new GameObject("Wedge");
            wedgeObject.transform.SetParent(_root.transform, false);
            _wedge = wedgeObject.AddComponent<SpriteRenderer>();
            _wedge.sortingOrder = sortingOrder + 1;
            _root.SetActive(false);
        }

        /// <summary>Shows the pie at <paramref name="position"/> with the given wedge sprite and colour.</summary>
        public void Show(Vector3 position, int step, Sprite wedge, Color color)
        {
            if (!_root.activeSelf) _root.SetActive(true);
            _root.transform.position = position;
            if (step != _step)
            {
                _step = step;
                _wedge.sprite = wedge;
            }
            _wedge.color = color;
        }

        /// <summary>Hides the pie so it can be reused for another car.</summary>
        public void Hide()
        {
            _step = -1;
            if (_root.activeSelf) _root.SetActive(false);
        }

        public void Destroy() => PieObjects.Release(_root);
    }
}
