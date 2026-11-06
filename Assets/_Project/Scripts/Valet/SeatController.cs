using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UnityEngine;

namespace UncannyValet.Valet
{
    /// <summary>
    /// Puts the valet in a car when the car publishes CarEntered, forwards input to it, and gets out beside it
    /// on the interact key (the car publishes CarExited from SetDriver(false)). Hides the walking body while seated.
    /// </summary>
    public sealed class SeatController : MonoBehaviour
    {
        [SerializeField] private float _exitDistance = 1.5f;
        [SerializeField] private OnFootMover _mover;
        [SerializeField] private Interactor _interactor;
        [SerializeField] private Renderer[] _bodyRenderers;
        [SerializeField] private Collider2D[] _bodyColliders;

        private readonly ValetState _state = new ValetState();
        private IInputSource _input;
        private IEventBus _bus;
        private IDrivable _car;
        private CarId _carId;
        private int _enterFrame = -1;

        private void Start()
        {
            _input = GetComponent<IInputSource>();
            if (_input == null) Services.TryGet(out _input);
            _mover.SetInput(_input);
            _interactor.SetInput(_input);
            if (!Services.TryGet(out _bus)) return;
            _bus.Subscribe<CarEntered>(OnCarEntered);
            _bus.Subscribe<CarExited>(OnCarExited);
        }

        private void OnDestroy()
        {
            if (_bus == null) return;
            _bus.Unsubscribe<CarEntered>(OnCarEntered);
            _bus.Unsubscribe<CarExited>(OnCarExited);
        }

        private void OnCarEntered(CarEntered e)
        {
            if (!Services.TryGet<ICarFactory>(out var cars) || !cars.TryGet(e.Car, out var car)) return;
            if (!_state.TryEnterCar()) return;
            _car = car;
            _carId = e.Car;
            _enterFrame = Time.frameCount;
            _car.SetDriver(true);
            SetBodyActive(false);
        }

        // The seated car was despawned: it publishes CarExited from OnDestroy while still driven.
        private void OnCarExited(CarExited e)
        {
            if (_state.IsDriving && e.Car == _carId) LeaveVanishedCar();
        }

        // The valet already sits at the car's last position (Update follows the car), so nudge aside from there.
        private void LeaveVanishedCar()
        {
            _car = null;
            _state.TryExitCar();
            transform.position = ExitPlacement.Beside(transform.position, Vector2.right, _exitDistance);
            _interactor.IgnoreInteractThisFrame();
            SetBodyActive(true);
        }

        private void Update()
        {
            if (!_state.IsDriving || _input == null) return;
            if (_car is Object unityCar && !unityCar) { LeaveVanishedCar(); return; }
            transform.position = _car.Transform.position;
            _car.SetInput(_input.Move.y, _input.Move.x);
            if (_input.InteractPressed && Time.frameCount != _enterFrame) Exit();
        }

        private void Exit()
        {
            var car = _car;
            car.SetInput(0f, 0f);
            _car = null;
            _state.TryExitCar(); // before SetDriver, so the CarExited it publishes is ignored here
            car.SetDriver(false);
            transform.position = ExitPlacement.Beside(car.Transform.position, car.Transform.right, _exitDistance);
            _interactor.IgnoreInteractThisFrame();
            SetBodyActive(true);
        }

        private void SetBodyActive(bool active)
        {
            _mover.enabled = active;
            _interactor.enabled = active;
            foreach (var r in _bodyRenderers) r.enabled = active;
            foreach (var c in _bodyColliders) c.enabled = active;
        }
    }
}
