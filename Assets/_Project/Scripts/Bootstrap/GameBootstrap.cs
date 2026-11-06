using System.Collections;
using UncannyValet.Audio;
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
        [SerializeField] private NightPlanConfig _planConfig;
        [SerializeField] private CarFactory _carFactory;
        [SerializeField] private LotLayoutBuilder _lot;
        [SerializeField] private NightClock _clock;
        [SerializeField] private GameObject _valet;
        [SerializeField] private PatronSpawner _spawner;
        [SerializeField] private DebugOverlay _overlay;
        [SerializeField] private CameraFollow _camera;
        [SerializeField] private NightSummaryCard _summaryCard;
        [SerializeField] private AudioDirector _audio;

        private RunDebt _debt;
        private NightTallyRecorder _recorder;
        private NightFlow _flow;

        private void Awake()
        {
            Services.Reset();
            var bus = new EventBus();
            var input = _valet.GetComponent<KeyboardInputSource>();
            _debt = new RunDebt(bus, _runConfig.StartingDebt);
            _recorder = new NightTallyRecorder(bus, _debt, _runConfig.Nights,
                n => _planConfig.For(n).NewTonight);
            _carFactory.Bus = bus;

            Services.Register<IEventBus>(bus);
            Services.Register<ICarFactory>(_carFactory);
            Services.Register<ILotLayout>(_lot);
            Services.Register<IInputSource>(input);
            Services.Register<IGameClock>(_clock);
            Services.Register<ITipCalculator>(new TipCalculator(_tipConfig));
            Services.Register<IDebtView>(_debt);

            _carFactory.gameObject.SetActive(true);
            _lot.gameObject.SetActive(true);
            _clock.gameObject.SetActive(true);
            _clock.Initialize(bus, _runConfig.NightLengthMinutes, _runConfig.GameMinutesPerSecond, _planConfig);
            SpawnValet();
            _spawner.Random = new UnityPatronRandom();
            _spawner.gameObject.SetActive(true);
            StartOverlay();
            _summaryCard.gameObject.SetActive(true);
            _audio.gameObject.SetActive(true);
            _flow = new NightFlow(bus, new ClockNightStarter(_clock),
                new ValetPlacer(_valet, _lot, _camera), _debt, _runConfig.Nights);
        }

        // One frame later, so every component activated above has run Start (StationSwitch subscribes to
        // NightPlanChanged there) before night 1's plan is published.
        private IEnumerator Start()
        {
            yield return null;
            _flow.StartFirstNight();
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
            _flow?.Dispose();
            _recorder?.Dispose();
            _debt?.Dispose();
            Services.Reset();
        }
    }
}
