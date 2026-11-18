using System;
using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Ids;
using UncannyValet.UI.Mood;
using UnityEngine;

namespace UncannyValet.UI.Pie
{
    /// <summary>
    /// Keeps one pie over every car whose pickup timer runs (green to yellow to red). The pie is released the frame the timer stops,
    /// the ticket closes or the car is gone.    /// </summary>
    public sealed class PickupPieLayer : IDisposable
    {
        private readonly ICarFactory _cars;
        private readonly EmotionStyle _style;
        private readonly PickupPieSettings _settings;
        private readonly PickupPieSprites _sprites;
        private readonly PieSet _pies;

        public PickupPieLayer(ICarFactory cars, EmotionStyle style, PickupPieSettings settings)
        {
            _cars = cars;
            _style = style;
            _settings = settings;
            _sprites = new PickupPieSprites(settings.DiameterPixels, settings.Steps);
            _pies = new PieSet("Pickup Pies", _sprites.Back, settings.SortingOrder);
        }

        /// <summary>How many pies are showing right now.</summary>
        public int ActiveCount => _pies.ActiveCount;

        /// <summary>Shows a pie for every row with a running pickup timer and releases the pies of all other cars.</summary>
        public void Update(List<TicketRow> rows)
        {
            _pies.Begin();
            for (var i = 0; i < rows.Count; i++)
                if (PickupPieRule.IsShown(rows[i], _settings.Steps)) Show(rows[i]);
            _pies.End();
        }

        public void Dispose()
        {
            _pies.Dispose();
            _sprites.Dispose();
        }

        private void Show(TicketRow row)
        {
            if (!_cars.TryGet(new CarId(row.Car), out var car) || car.Transform == null) return;
            var step = PickupPieRule.Step(row.Elapsed01, _settings.Steps);
            var color = _style.ColorOf(PickupPieRule.ColorState(row, _style.Rule));
            _pies.Show(row.Car, car.Transform.position + (Vector3)_settings.Offset, step, _sprites.Wedge(step), color);
        }
    }
}
