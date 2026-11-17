using System;
using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Ids;
using UnityEngine;

namespace UncannyValet.UI.Pie
{
    /// <summary>
    /// Keeps one blue pie over every car being washed in the wash station, draining as the wash progresses. Released the frame the car
    /// leaves the station, the wash completes (a drop symbol takes over, see ServiceIcons), the ticket closes or the car is gone.
    /// </summary>
    public sealed class WashPieLayer : IDisposable
    {
        private readonly ICarFactory _cars;
        private readonly PickupPieSettings _settings;
        private readonly PickupPieSprites _sprites;
        private readonly PieSet _pies;

        public WashPieLayer(ICarFactory cars, PickupPieSettings settings)
        {
            _cars = cars;
            _settings = settings;
            _sprites = new PickupPieSprites(settings.DiameterPixels, settings.Steps);
            _pies = new PieSet("Wash Pies", _sprites.Back, settings.SortingOrder);
        }

        /// <summary>How many pies are showing right now.</summary>
        public int ActiveCount => _pies.ActiveCount;

        /// <summary>Shows a pie for every car being washed and releases the pies of all other cars.</summary>
        public void Update(List<TicketRow> rows)
        {
            _pies.Begin();
            for (var i = 0; i < rows.Count; i++)
                if (WashPieRule.IsShown(rows[i], _settings.Steps)) Show(rows[i]);
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
            var step = WashPieRule.Step(WashPieRule.Washing(row).Progress01, _settings.Steps);
            var shift = PieSlots.WashShift(row, _settings.Steps, _settings.SideGap);
            _pies.Show(row.Car, car.Transform.position + (Vector3)_settings.Offset + Vector3.right * shift, step, _sprites.Wedge(step), _settings.WashColor);
        }
    }
}
