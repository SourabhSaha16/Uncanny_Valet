using UncannyValet.Core;
using UncannyValet.Core.Events;
using UnityEngine;

namespace UncannyValet.Cars.Fx
{
    /// <summary>
    /// Thin shell on a car: while a human drives it and it slides, drops skid marks behind both rear tyres into the shared
    /// <see cref="DecalPool"/>, publishes the slide and hard-brake events and keeps the <see cref="SlipMeter"/> current.
    /// Cars driven by trips or auto-drive (no seated driver) never emit.
    /// </summary>
    [RequireComponent(typeof(CarDriver), typeof(Rigidbody2D))]
    [DisallowMultipleComponent]
    public sealed class SkidEmitter : MonoBehaviour
    {
        private readonly SkidRule _rule = new SkidRule();
        private readonly TyreTrail _left = new TyreTrail();
        private readonly TyreTrail _right = new TyreTrail();
        private CarDriver _driver;
        private Rigidbody2D _body;
        private CarHandlingKit _kit;
        private DecalPool _pool;
        private SlipMeter _meter;
        private IEventBus _bus;

        /// <summary>Gives the emitter the shared handling numbers, the decal pool and the slip meter.</summary>
        public void Initialize(CarHandlingKit kit, DecalPool pool, SlipMeter meter)
        {
            _kit = kit;
            _pool = pool;
            _meter = meter;
        }

        private void Awake()
        {
            _driver = GetComponent<CarDriver>();
            _body = GetComponent<Rigidbody2D>();
        }

        private void Start()
        {
            if (Services.TryGet<IEventBus>(out var bus)) _bus = bus;
        }

        private void OnDisable() => Publish(_rule.Reset());

        private void FixedUpdate()
        {
            if (_kit == null || _pool == null) return;
            var driven = _driver.isActiveAndEnabled && _driver.HasDriver;
            var state = driven ? _rule.Step(_driver.Reading, _kit.Live.Skid) : _rule.Reset();
            Publish(state);
            if (driven) _meter.Report(_driver.Id, state.Sliding, state.Intensity01);
            else _meter.Clear(_driver.Id);
            if (!state.Sliding)
            {
                _left.Break();
                _right.Break();
                return;
            }
            Drop(_left, false, state.Opacity);
            Drop(_right, true, state.Opacity);
        }

        private void Publish(in SkidState state)
        {
            if (_bus == null) return;
            if (state.SlideStarted) _bus.Publish(new CarSlideStarted(_driver.Id));
            if (state.SlideEnded) _bus.Publish(new CarSlideEnded(_driver.Id));
            if (state.HardBrakeStarted) _bus.Publish(new CarHardBraked(_driver.Id));
        }

        private void Drop(TyreTrail trail, bool right, float opacity)
        {
            var skid = _kit.Config.Skid;
            var tyre = TyreMarkMath.RearTyre(_body.position, _body.rotation, _driver.Definition.Size,
                skid.TyreSideShare, skid.TyreRearShare, right);
            if (!trail.TryAdvance(tyre, skid.MarkSpacing, skid.MarkThickness, out var mark)) return;
            _pool.Place(mark.Center, mark.AngleDegrees, mark.Length, skid.MarkThickness, skid.MarkColor, opacity,
                _kit.Live.MarkFadeSeconds);
        }
    }
}
