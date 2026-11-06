using System;
using UncannyValet.Core.Contracts;
using UnityEngine;

namespace UncannyValet.Bootstrap
{
    /// <summary>Moves the on-foot valet to <see cref="ILotLayout.ValetStartPoint"/> through his body, and snaps the camera.</summary>
    public sealed class ValetPlacer : IValetPlacer
    {
        private readonly Transform _valet;
        private readonly Rigidbody2D _body;
        private readonly ILotLayout _lot;
        private readonly CameraFollow _camera;

        public ValetPlacer(GameObject valet, ILotLayout lot, CameraFollow camera)
        {
            if (valet == null) throw new ArgumentNullException(nameof(valet));
            _valet = valet.transform;
            _body = valet.GetComponent<Rigidbody2D>();
            _lot = lot ?? throw new ArgumentNullException(nameof(lot));
            _camera = camera;
        }

        public void PlaceAtStart()
        {
            var start = _lot.ValetStartPoint;
            if (_body != null)
            {
                _body.position = start;
                _body.linearVelocity = Vector2.zero;
            }
            _valet.position = start;
            if (_camera != null) _camera.SetTarget(_valet);
        }
    }
}
