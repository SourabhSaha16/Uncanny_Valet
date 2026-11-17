using System;
using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Ids;
using UncannyValet.UI.Mood;
using UnityEngine;

namespace UncannyValet.UI.Pie
{
    /// <summary>
    /// Keeps one <see cref="PickupPieView"/> over every car whose pickup timer runs. A pie is taken from a pool when its car's timer
    /// starts and returned the frame the timer stops, the ticket closes or the car is gone. Allocates nothing once the pool has grown.
    /// </summary>
    public sealed class PickupPieLayer : IDisposable
    {
        private readonly ICarFactory _cars;
        private readonly EmotionStyle _style;
        private readonly PickupPieSettings _settings;
        private readonly PickupPieSprites _sprites;
        private readonly Transform _root;
        private readonly Dictionary<int, PickupPieView> _active = new Dictionary<int, PickupPieView>();
        private readonly HashSet<int> _seen = new HashSet<int>();
        private readonly List<int> _stale = new List<int>(8);
        private readonly Stack<PickupPieView> _spare = new Stack<PickupPieView>();

        public PickupPieLayer(ICarFactory cars, EmotionStyle style, PickupPieSettings settings)
        {
            _cars = cars;
            _style = style;
            _settings = settings;
            _sprites = new PickupPieSprites(settings.DiameterPixels, settings.Steps);
            _root = new GameObject("Pickup Pies").transform;
        }

        /// <summary>How many pies are showing right now.</summary>
        public int ActiveCount => _active.Count;

        /// <summary>Shows a pie for every row with a running pickup timer and releases the pies of all other cars.</summary>
        public void Update(List<TicketRow> rows)
        {
            _seen.Clear();
            for (var i = 0; i < rows.Count; i++)
                if (PickupPieRule.IsShown(rows[i], _settings.Steps) && TryShow(rows[i])) _seen.Add(rows[i].Car);
            ReleaseUnseen();
        }

        public void Dispose()
        {
            foreach (var pair in _active) pair.Value.Destroy();
            foreach (var view in _spare) view.Destroy();
            _active.Clear();
            _spare.Clear();
            if (_root != null) PieObjects.Release(_root.gameObject);
            _sprites.Dispose();
        }

        private bool TryShow(TicketRow row)
        {
            if (!_cars.TryGet(new CarId(row.Car), out var car) || car.Transform == null) return false;
            if (!_active.TryGetValue(row.Car, out var view))
            {
                view = _spare.Count > 0 ? _spare.Pop() : new PickupPieView(_root, _sprites.Back, _settings.SortingOrder);
                _active.Add(row.Car, view);
            }
            var step = PickupPieRule.Step(row.Elapsed01, _settings.Steps);
            var color = _style.ColorOf(PickupPieRule.ColorState(row, _style.Rule));
            view.Show(car.Transform.position + (Vector3)_settings.Offset, step, _sprites.Wedge(step), color);
            return true;
        }

        private void ReleaseUnseen()
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
    }
}
