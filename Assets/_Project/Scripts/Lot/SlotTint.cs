using UncannyValet.Core;
using UncannyValet.Core.Events;
using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>
    /// Lights up the fill of one wash slot or charging bay while a car is washed or charged there. Reads the Core
    /// events (enter, leave, clean and charge progress, finished, ticket closed) and lets <see cref="SlotTintRule"/> decide.
    /// Sits next to the slot's <see cref="StopZone"/>, which says whether a car is really in this slot.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SlotTint : MonoBehaviour
    {
        private SlotTintRule _rule;
        private StopZone _zone;
        private StationKind _kind;
        private SpriteRenderer _fill;
        private Color _idle;
        private Color _lit;
        private IEventBus _bus;

        public void Init(StopZone zone, StationKind kind, SpriteRenderer fill, Color lit, LotConfig config)
        {
            _zone = zone;
            _kind = kind;
            _fill = fill;
            _idle = fill.color;
            _lit = lit;
            _rule = new SlotTintRule(config.TintMinShare, config.TintMaxShare, config.TintDoneSeconds);
        }

        private void Start()
        {
            _bus = Services.Get<IEventBus>();
            Subscribe();
        }

        private void OnEnable()
        {
            if (_bus != null) Subscribe();
        }

        private void OnDisable()
        {
            if (_bus == null) return;
            _bus.Unsubscribe<CarEnteredStation>(OnEntered);
            _bus.Unsubscribe<CarLeftStation>(OnLeft);
            _bus.Unsubscribe<CarCleanChanged>(OnCleanChanged);
            _bus.Unsubscribe<CarCleaned>(OnCleaned);
            _bus.Unsubscribe<CarChargeChanged>(OnChargeChanged);
            _bus.Unsubscribe<CarFullyCharged>(OnCharged);
            _bus.Unsubscribe<TicketClosed>(OnClosed);
        }

        private void Subscribe()
        {
            _bus.Subscribe<CarEnteredStation>(OnEntered);
            _bus.Subscribe<CarLeftStation>(OnLeft);
            _bus.Subscribe<CarCleanChanged>(OnCleanChanged);
            _bus.Subscribe<CarCleaned>(OnCleaned);
            _bus.Subscribe<CarChargeChanged>(OnChargeChanged);
            _bus.Subscribe<CarFullyCharged>(OnCharged);
            _bus.Subscribe<TicketClosed>(OnClosed);
        }

        private void OnEntered(CarEnteredStation e)
        {
            if (e.Station == _kind && _zone.Holds(e.Car)) _rule.Enter(e.Car);
            Apply();
        }

        private void OnLeft(CarLeftStation e)
        {
            if (e.Station == _kind) _rule.Leave(e.Car);
            Apply();
        }

        private void OnCleanChanged(CarCleanChanged e)
        {
            if (_kind == StationKind.Wash) _rule.Progress(e.Car, e.Clean01);
            Apply();
        }

        private void OnCleaned(CarCleaned e)
        {
            if (_kind == StationKind.Wash) _rule.Finished(e.Car);
            Apply();
        }

        private void OnChargeChanged(CarChargeChanged e)
        {
            if (_kind == StationKind.Charging) _rule.Progress(e.Car, e.Charge01);
            Apply();
        }

        private void OnCharged(CarFullyCharged e)
        {
            if (_kind == StationKind.Charging) _rule.Finished(e.Car);
            Apply();
        }

        private void OnClosed(TicketClosed e)
        {
            _rule.Closed(e.Car);
            Apply();
        }

        private void Update()
        {
            if (_rule.Phase != SlotTintPhase.Done) return;
            _rule.Tick(Time.deltaTime);
            Apply();
        }

        private void Apply() => _fill.color = Color.Lerp(_idle, _lit, _rule.Share);
    }
}
