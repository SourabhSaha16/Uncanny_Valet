using System;
using UncannyValet.Core.Contracts;
using UncannyValet.Lot;
using UncannyValet.Lot.Handmade;
using UnityEngine;

namespace UncannyValet.Bootstrap
{
    /// <summary>
    /// A hand-made lot (#194) as the bootstrap sees it. <see cref="HandmadeLot"/> is the layout, the bay geometry, the road and the walking routes
    /// once it has woken up; the Perfect Park thresholds come from the <see cref="LotConfig"/> on the lot root.
    /// </summary>
    public sealed class HandmadeLotSource : ILotSource
    {
        private readonly HandmadeLot _lot;

        public HandmadeLotSource(HandmadeLot lot) => _lot = lot != null ? lot : throw new ArgumentNullException(nameof(lot));

        public ILotLayout Layout => _lot;

        public ISpotGeometry Spots => _lot;

        public IRoadLayout Road => _lot.Road;

        public IPedestrianPaths Paths => _lot.Paths;

        public PerfectSettings Perfect => _lot.Perfect;

        public GameObject Root => _lot.gameObject;

        /// <summary>Widens the rectangle to the lot's view (the authored one, or the box round the gameplay markers); unchanged until the lot is built.</summary>
        public Rect ExtendView(Rect rect)
        {
            if (!_lot.IsBuilt) return rect;
            var view = _lot.ViewRect;
            return Rect.MinMaxRect(Mathf.Min(rect.xMin, view.xMin), Mathf.Min(rect.yMin, view.yMin), Mathf.Max(rect.xMax, view.xMax), Mathf.Max(rect.yMax, view.yMax));
        }
    }
}
