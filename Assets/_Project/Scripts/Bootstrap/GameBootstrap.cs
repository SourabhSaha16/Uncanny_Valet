using UncannyValet.Cars;
using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Economy;
using UncannyValet.Lot;
using UncannyValet.Night;
using UncannyValet.Patrons;
using UncannyValet.UI;
using UncannyValet.Valet;
using UnityEngine;

namespace UncannyValet.Bootstrap
{
    /// <summary>
    /// The only place concrete types meet. Child objects start inactive (set up by the editor script);
    /// this registers every service first, then activates them in a fixed order.
    /// </summary>
    public sealed class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private RunConfig _runConfig;
        [SerializeField] private TipConfig _tipConfig;
        [SerializeField] private CarFactory _carFactory;
        [SerializeField] private LotLayoutBuilder _lot;
        [SerializeField] private NightClock _clock;
        [SerializeField] private GameObject _valet;
        [SerializeField] private PatronSpawner _spawner;
        [SerializeField] private DebugOverlay _overlay;
        [SerializeField] private CameraFollow _camera;

        private DebtLedger _ledger;

        private void Awake()
        {
            Services.Reset();
            var bus = new EventBus();
            var input = _valet.GetComponent<KeyboardInputSource>();
            _ledger = new DebtLedger(bus, _runConfig.StartingDebt);
            _carFactory.Bus = bus;

            Services.Register<IEventBus>(bus);
            Services.Register<ICarFactory>(_carFactory);
            Services.Register<ILotLayout>(_lot);
            Services.Register<IInputSource>(input);
            Services.Register<IGameClock>(_clock);
            Services.Register<ITipCalculator>(new TipCalculator(_tipConfig));
            Services.Register<IDebtView>(_ledger);

            _carFactory.gameObject.SetActive(true);
            _lot.gameObject.SetActive(true);
            _clock.gameObject.SetActive(true);
            SpawnValet();
            _spawner.gameObject.SetActive(true);
            StartOverlay();
            _clock.StartNight(1);
        }

        private void SpawnValet()
        {
            _valet.transform.position = Services.Get<ILotLayout>().ValetStartPoint;
            var body = _valet.GetComponentInChildren<SpriteRenderer>();
            body.sprite = PlaceholderSprite.Create();
            body.color = new Color(0.95f, 0.85f, 0.2f);
            _valet.SetActive(true);
            if (_camera != null) _camera.SetTarget(_valet.transform);
        }

        private void StartOverlay()
        {
            var interactor = _valet.GetComponent<Interactor>();
            _overlay.Configure(() => interactor.CurrentPrompt, m => ClockFormat.Format(m));
            _overlay.gameObject.SetActive(true);
        }

        private void OnDestroy()
        {
            _ledger?.Dispose();
            Services.Reset();
        }
    }
}
