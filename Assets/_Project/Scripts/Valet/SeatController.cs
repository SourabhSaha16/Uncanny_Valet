using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UnityEngine;

namespace UncannyValet.Valet
{
    /// <summary>
    /// Puts the valet in a car when the car publishes CarEntered, forwards input to it, and gets out beside it
    /// on the interact key (publishing CarExited). Hides the walking body while seated.
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
        private int _enterFrame = -1;

        private void Start()
        {
            _input = GetComponent<IInputSource>();
            if (_input == null) Services.TryGet(out _input);
            _mover.SetInput(_input);
            _interactor.SetInput(_input);
            if (Services.TryGet(out _bus)) _bus.Subscribe<CarEntered>(OnCarEntered);
        }

        private void OnDestroy() => _bus?.Unsubscribe<CarEntered>(OnCarEntered);

        private void OnCarEntered(CarEntered e)
        {
            if (!Services.TryGet<ICarFactory>(out var cars) || !cars.TryGet(e.Car, out var car)) return;
            if (!_state.TryEnterCar()) return;
            _car = car;
            _enterFrame = Time.frameCount;
            _car.SetDriver(true);
            SetBodyActive(false);
        }

        private void Update()
        {
            if (!_state.IsDriving || _input == null) return;
            transform.position = _car.Transform.position;
            _car.SetInput(_input.Move.y, _input.Move.x);
            if (_input.InteractPressed && Time.frameCount != _enterFrame) Exit();
        }

        private void Exit()
        {
            var car = _car;
            car.SetInput(0f, 0f);
            car.SetDriver(false);
            _car = null;
            _state.TryExitCar();
            transform.position = car.Transform.position + car.Transform.right * _exitDistance;
            SetBodyActive(true);
            _bus?.Publish(new CarExited(car.Id));
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
