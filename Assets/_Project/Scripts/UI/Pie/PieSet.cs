using System;
using System.Collections.Generic;
using UnityEngine;

namespace UncannyValet.UI.Pie
{
    /// <summary>
    /// A pool of pie views keyed by car: a frame is Begin, any number of Show calls, End. A view is taken from the pool the first frame a
    /// car shows a pie and returned the first frame it does not. Allocates nothing once the pool has grown.
    /// </summary>
    public sealed class PieSet : IDisposable
    {
        private readonly Transform _root;
        private readonly Sprite _back;
        private readonly int _sortingOrder;
        private readonly Dictionary<int, PickupPieView> _active = new Dictionary<int, PickupPieView>();
        private readonly HashSet<int> _seen = new HashSet<int>();
        private readonly List<int> _stale = new List<int>(8);
        private readonly Stack<PickupPieView> _spare = new Stack<PickupPieView>();

        public PieSet(string name, Sprite back, int sortingOrder)
        {
            _root = new GameObject(name).transform;
            _back = back;
            _sortingOrder = sortingOrder;
        }

        /// <summary>How many pies are showing right now.</summary>
        public int ActiveCount => _active.Count;

        /// <summary>Starts a frame: every pie not shown again before <see cref="End"/> is released.</summary>
        public void Begin() => _seen.Clear();

        /// <summary>Shows the car's pie at <paramref name="position"/>, taking a view from the pool if the car has none yet.</summary>
        public void Show(int car, Vector3 position, int step, Sprite wedge, Color color)
        {
            if (!_active.TryGetValue(car, out var view))
            {
                view = _spare.Count > 0 ? _spare.Pop() : new PickupPieView(_root, _back, _sortingOrder);
                _active.Add(car, view);
            }
            _seen.Add(car);
            view.Show(position, step, wedge, color);
        }

        /// <summary>Ends the frame: returns the pies of cars that were not shown since <see cref="Begin"/> to the pool.</summary>
        public void End()
        {
            _stale.Clear();
            foreach (var pair in _active)
                if (!_seen.Contains(pair.Key)) _stale.Add(pair.Key);
            for (var i = 0; i < _stale.Count; i++)
            {
                var view = _active[_stale[i]];
                view.Hide();
                _spare.Push(view);
                _active.Remove(_stale[i]);
            }
        }

        public void Dispose()
        {
            foreach (var pair in _active) pair.Value.Destroy();
            foreach (var view in _spare) view.Destroy();
            _active.Clear();
            _spare.Clear();
            if (_root != null) PieObjects.Release(_root.gameObject);
        }
    }
}
