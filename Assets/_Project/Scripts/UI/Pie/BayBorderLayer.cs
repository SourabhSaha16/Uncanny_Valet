using System;
using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.UI.Mood;
using UnityEngine;

namespace UncannyValet.UI.Pie
{
    /// <summary>
    /// Flashes the border of bays: green for a moment after a car parks completely inside, then in the pickup-timer colour at the
    /// emotion's speed while its patron waits. A border is taken from a pool and returned when the car leaves the bay, the ticket
    /// closes, the shift ends or the car is destroyed. Allocates nothing once the pool has grown.
    /// </summary>
    public sealed class BayBorderLayer : IDisposable
    {
        private readonly struct ParkFlash
        {
            public readonly CarId Car;
            public readonly float Start;
            public ParkFlash(CarId car, float start) { Car = car; Start = start; }
        }

        private readonly IEventBus _bus;
        private readonly ISpotGeometry _geometry;
        private readonly ICarFactory _cars;
        private readonly EmotionStyle _style;
        private readonly BayBorderSettings _settings;
        private readonly Func<float> _time;
        private readonly BayBorderSprites _sprites;
        private readonly Transform _root;
        private readonly Dictionary<int, ParkFlash> _parked = new Dictionary<int, ParkFlash>();
        private readonly Dictionary<int, BayBorderView> _active = new Dictionary<int, BayBorderView>();
        private readonly HashSet<int> _seen = new HashSet<int>();
        private readonly List<int> _stale = new List<int>(8);
        private readonly Stack<BayBorderView> _spare = new Stack<BayBorderView>();
        private readonly Color _parkedColor;

        public BayBorderLayer(IEventBus bus, ISpotGeometry geometry, ICarFactory cars, EmotionStyle style, BayBorderSettings settings, Color parkedColor, Func<float> time)
        {
            _bus = bus;
            _geometry = geometry;
            _cars = cars;
            _style = style;
            _settings = settings;
            _parkedColor = parkedColor;
            _time = time;
            _sprites = new BayBorderSprites(settings.ThicknessPixels);
            _root = new GameObject("Bay Borders").transform;
            _bus.Subscribe<CarParkedInside>(OnParkedInside);
            _bus.Subscribe<CarLeftSpot>(OnLeftSpot);
            _bus.Subscribe<ShiftEnded>(OnShiftEnded);
        }

        /// <summary>How many borders are in use (lit or in the dark half of a blink) right now.</summary>
        public int ActiveCount => _active.Count;

        /// <summary>Flashes the bay of every waiting car and every fresh clean park, and releases all other borders.</summary>
        public void Update(List<TicketRow> rows)
        {
            var now = _time();
            _seen.Clear();
            for (var i = 0; i < rows.Count; i++)
                if (BayBorderRule.TimerFlashes(rows[i])) ShowTimer(rows[i], now);
            DropFinishedParkFlashes(now);
            foreach (var pair in _parked)
                if (!_seen.Contains(pair.Key)) ShowParked(pair.Key, pair.Value, now);
            ReleaseUnseen();
        }

        public void Dispose()
        {
            _bus.Unsubscribe<CarParkedInside>(OnParkedInside);
            _bus.Unsubscribe<CarLeftSpot>(OnLeftSpot);
            _bus.Unsubscribe<ShiftEnded>(OnShiftEnded);
            foreach (var pair in _active) pair.Value.Destroy();
            foreach (var view in _spare) view.Destroy();
            _active.Clear();
            _spare.Clear();
            _parked.Clear();
            if (_root != null) PieObjects.Release(_root.gameObject);
            _sprites.Dispose();
        }

        private void OnParkedInside(CarParkedInside e) => _parked[e.SpotId] = new ParkFlash(e.Car, _time());

        private void OnLeftSpot(CarLeftSpot e) => _parked.Remove(e.SpotId);

        private void OnShiftEnded(ShiftEnded e) => _parked.Clear();

        private void ShowTimer(TicketRow row, float now)
        {
            var emotion = PickupPieRule.ColorState(row, _style.Rule);
            if (Draw(row.Spot, BayBorderRule.IsLit(now, 0f, BayBorderRule.Hz(emotion, _settings)), _style.ColorOf(emotion))) _seen.Add(row.Spot);
        }

        private void ShowParked(int spot, ParkFlash flash, float now)
        {
            if (Draw(spot, BayBorderRule.IsLit(now, flash.Start, _settings.ParkedFlashHz), _parkedColor)) _seen.Add(spot);
        }

        private bool Draw(int spot, bool lit, Color color)
        {
            if (!_geometry.TryGetSpot(spot, out var bay)) return false;
            if (!_active.TryGetValue(spot, out var view))
            {
                view = _spare.Count > 0 ? _spare.Pop() : new BayBorderView(_root, _settings.SortingOrder);
                view.Place(bay, _sprites.Get(bay.Size));
                _active.Add(spot, view);
            }
            view.Show(lit, color);
            return true;
        }

        private void DropFinishedParkFlashes(float now)
        {
            _stale.Clear();
            foreach (var pair in _parked)
            {
                var gone = !_cars.TryGet(pair.Value.Car, out var car) || car.Transform == null;
                if (gone || !BayBorderRule.ParkedFlashRuns(now, pair.Value.Start, _settings.ParkedFlashSeconds)) _stale.Add(pair.Key);
            }
            for (var i = 0; i < _stale.Count; i++) _parked.Remove(_stale[i]);
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
