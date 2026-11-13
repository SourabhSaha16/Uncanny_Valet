using UncannyValet.Core;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Core.Modifiers;
using UnityEngine;

namespace UncannyValet.Cars
{
    /// <summary>
    /// Applies a ticket's modifiers to the car's look once they arrive: a modifier with the electric look swaps the car
    /// to an EV sprite (or a blue body), one with the mud look gets the mud overlay. Until then the car looks normal.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CarTraitLook : MonoBehaviour
    {
        private CarId _id;
        private CarDefinition _definition;
        private SpriteRenderer _body;
        private GameObject _mud;
        private CarBodyArt _art;
        private IEventBus _bus;
        private IModifierCatalog _modifiers;

        /// <summary>
        /// Adds the component to <paramref name="car"/>. The mud child starts inactive.
        /// With <paramref name="art"/> an EV swaps its sprite; without it the body colour changes.
        /// </summary>
        public static CarTraitLook Attach(GameObject car, CarId id, CarDefinition definition,
            SpriteRenderer body, CarMudOverlay mud, CarBodyArt art = null)
        {
            var look = car.AddComponent<CarTraitLook>();
            look._art = art;
            look._id = id;
            look._definition = definition;
            look._body = body;
            look._mud = mud.gameObject;
            return look;
        }

        private void OnEnable()
        {
            if (!Services.TryGet<IEventBus>(out _bus)) return;
            Services.TryGet(out _modifiers);
            _bus.Subscribe<TicketModifiersAssigned>(OnModifiers);
        }

        private void OnDisable()
        {
            if (_bus == null) return;
            _bus.Unsubscribe<TicketModifiersAssigned>(OnModifiers);
            _bus = null;
        }

        private void OnModifiers(TicketModifiersAssigned e)
        {
            if (e.Car != _id) return;
            var electric = ModifierLooks.Has(_modifiers, e.Modifiers, ModifierLook.ElectricBody);
            if (_art != null) _art.SetEv(electric);
            else _body.color = electric ? _definition.EvBodyColor : _definition.Color;
            _mud.SetActive(ModifierLooks.Has(_modifiers, e.Modifiers, ModifierLook.MudOverlay));
        }
    }
}
