using UncannyValet.Core;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UnityEngine;

namespace UncannyValet.Cars
{
    /// <summary>
    /// Applies a ticket's traits to the car's look once they arrive: EVs swap to an EV sprite (or a blue body),
    /// dirty cars get the mud overlay. Until then the car looks normal.
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
            if (_art != null) _art.SetEv(e.IsEv);
            else _body.color = e.IsEv ? _definition.EvBodyColor : _definition.Color;
            _mud.SetActive(e.IsDirty);
        }
    }
}
