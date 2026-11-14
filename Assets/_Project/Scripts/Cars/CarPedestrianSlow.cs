using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UnityEngine;

namespace UncannyValet.Cars
{
    /// <summary>
    /// Slows the car the valet drives when it knocks a pedestrian away: the car keeps its pre-impact heading (no bounce
    /// back) and loses a share of its speed, never going below a share of its top speed. Runs once per knockback.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(CarDriver)), DisallowMultipleComponent]
    public sealed class CarPedestrianSlow : MonoBehaviour
    {
        private Rigidbody2D _body;
        private CarDriver _driver;
        private CarHandlingKit _kit;
        private IEventBus _bus;
        private ITuning _tuning;

        /// <summary>Wires the shared handling numbers (the factory calls this right after adding the component).</summary>
        public void Initialize(CarHandlingKit kit)
        {
            _kit = kit;
            _body = GetComponent<Rigidbody2D>();
            _driver = GetComponent<CarDriver>();
        }

        private void Start()
        {
            if (!Services.TryGet(out _bus)) return;
            Services.TryGet(out _tuning);
            _bus.Subscribe<PedestrianHit>(OnHit);
        }

        private void OnEnable() => _bus?.Subscribe<PedestrianHit>(OnHit);

        private void OnDisable() => _bus?.Unsubscribe<PedestrianHit>(OnHit);

        private void OnHit(PedestrianHit e)
        {
            if (!e.ByPlayerCar || e.Car != _driver.Id || _kit == null) return;
            var before = _driver.PreSolveVelocity;
            if (before.sqrMagnitude < Mathf.Epsilon) return;
            var share = Read(TuningKey.PedestrianCarSlowdown, PedestrianHitRows.CarSlowdown);
            var floor = Read(TuningKey.PedestrianCarSlowFloor, PedestrianHitRows.CarSlowFloor) * _kit.Live.DriveFor(_driver.Definition).TopSpeed;
            _body.linearVelocity = before.normalized * PedestrianSlowRule.SlowedSpeed(before.magnitude, share, floor);
        }

        private float Read(TuningKey key, float fallback) => _tuning != null && _tuning.TryGet(key, out var v) ? v : fallback;
    }
}
