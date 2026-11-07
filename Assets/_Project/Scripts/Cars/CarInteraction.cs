using UncannyValet.Core.Contracts;
using UnityEngine;

namespace UncannyValet.Cars
{
    /// <summary>
    /// Lets the valet get in. Interact sets the driver, which publishes CarEntered; the valet
    /// module reacts to that event, so Cars never references Valet code.
    /// </summary>
    [RequireComponent(typeof(CarDriver))]
    public sealed class CarInteraction : MonoBehaviour, IInteractable
    {
        [SerializeField, Min(0.1f)] private float _range = 2.5f;

        private CarDriver _driver;

        public string Prompt => "Enter car";
        public Vector2 Position => transform.position;

        public bool CanInteract(GameObject actor)
        {
            if (actor == null || !isActiveAndEnabled || Driver.HasDriver) return false;
            return Vector2.Distance(actor.transform.position, transform.position) <= _range;
        }

        public void Interact(GameObject actor)
        {
            if (CanInteract(actor)) Driver.SetDriver(true);
        }

        private CarDriver Driver => _driver != null ? _driver : (_driver = GetComponent<CarDriver>());
    }
}
