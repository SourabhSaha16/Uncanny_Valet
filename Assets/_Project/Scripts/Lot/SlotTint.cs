using UncannyValet.Core;
using UncannyValet.Core.Events;
using UncannyValet.Core.Modifiers;
using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>
    /// Lights up the fill of one wash slot or charging bay while a car is washed or charged there. Reads the Core
    /// events (enter, leave, modifier progress and cleared, ticket closed) and lets <see cref="SlotTintRule"/> decide.
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
        private IModifierCatalog _modifiers;

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
            _modifiers = Services.Get<IModifierCatalog>();
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
            _bus.Unsubscribe<ModifierProgressChanged>(OnProgress);
            _bus.Unsubscribe<ModifierCleared>(OnCleared);
            _bus.Unsubscribe<TicketClosed>(OnClosed);
        }

        private void Subscribe()
        {
            _bus.Subscribe<CarEnteredStation>(OnEntered);
            _bus.Subscribe<CarLeftStation>(OnLeft);
            _bus.Subscribe<ModifierProgressChanged>(OnProgress);
            _bus.Subscribe<ModifierCleared>(OnCleared);
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

        private void OnProgress(ModifierProgressChanged e)
        {
            if (ClearedHere(e.Modifier)) _rule.Progress(e.Car, e.Progress01);
            Apply();
        }

        private void OnCleared(ModifierCleared e)
        {
            if (ClearedHere(e.Modifier)) _rule.Finished(e.Car);
            Apply();
        }

        private bool ClearedHere(ModifierId id) => _modifiers.TryGet(id, out var def) && def.ClearedAt == _kind;

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
