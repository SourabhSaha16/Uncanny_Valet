using UncannyValet.Core;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UnityEngine;

namespace UncannyValet.Cars
{
    /// <summary>
    /// Applies a ticket's traits to the car's look once they arrive: EVs get a blue body and a charge bar,
    /// dirty cars get the mud overlay. Until then the car looks normal.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CarTraitLook : MonoBehaviour
    {
        private CarId _id;
        private CarDefinition _definition;
        private SpriteRenderer _body;
        private GameObject _chargeBar;
        private GameObject _mud;
        private IEventBus _bus;

        /// <summary>Adds the component to <paramref name="car"/>. The bar and mud children start inactive.</summary>
        public static CarTraitLook Attach(GameObject car, CarId id, CarDefinition definition,
            SpriteRenderer body, CarChargeBar chargeBar, CarMudOverlay mud)
        {
            var look = car.AddComponent<CarTraitLook>();
            look._id = id;
            look._definition = definition;
            look._body = body;
            look._chargeBar = chargeBar.gameObject;
            look._mud = mud.gameObject;
            return look;
        }

        private void OnEnable()
        {
            if (!Services.TryGet<IEventBus>(out _bus)) return;
            _bus.Subscribe<TicketTraitsAssigned>(OnTraits);
        }

        private void OnDisable()
        {
            if (_bus == null) return;
            _bus.Unsubscribe<TicketTraitsAssigned>(OnTraits);
            _bus = null;
        }

        private void OnTraits(TicketTraitsAssigned e)
        {
            if (e.Car != _id) return;
            _body.color = e.IsEv ? _definition.EvBodyColor : _definition.Color;
            _chargeBar.SetActive(e.IsEv);
            _mud.SetActive(e.IsDirty);
        }
    }
}
