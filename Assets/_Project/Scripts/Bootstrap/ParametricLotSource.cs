using System;
using UncannyValet.Core.Contracts;
using UncannyValet.Lot;
using UnityEngine;

namespace UncannyValet.Bootstrap
{
    /// <summary>The generated lot: a <see cref="LotLayoutBuilder"/> that builds the bays and queue from its config, and an optional <see cref="LotDecor"/> for the scenery.</summary>
    public sealed class ParametricLotSource : ILotSource
    {
        private readonly LotLayoutBuilder _lot;
        private readonly LotDecor _decor;

        /// <param name="lot">The builder; required.</param>
        /// <param name="decor">The scenery; null for a lot without scenery (then no pedestrian routes and no extended view).</param>
        public ParametricLotSource(LotLayoutBuilder lot, LotDecor decor)
        {
            _lot = lot != null ? lot : throw new ArgumentNullException(nameof(lot));
            _decor = decor;
        }

        public ILotLayout Layout => _lot;

        public ISpotGeometry Spots => _lot;

        public IRoadLayout Road => _lot.Road;

        public IPedestrianPaths Paths => _decor != null ? _decor.Paths : null;

        public PerfectSettings Perfect => _lot.Perfect;

        public GameObject Root => _lot.gameObject;

        public Rect ExtendView(Rect rect) => _decor != null ? _decor.ExtendView(rect) : rect;
    }
}
