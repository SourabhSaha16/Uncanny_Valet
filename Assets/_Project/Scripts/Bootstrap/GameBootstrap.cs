using UncannyValet.Audio;
using UncannyValet.Cars;
using UncannyValet.Core;
using UncannyValet.Core.Config;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Director;
using UncannyValet.Economy;
using UncannyValet.Lot;
using UncannyValet.Night;
using UncannyValet.Patrons;
using UncannyValet.Pedestrians;
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
        [SerializeField] private ShiftConfig _shiftConfig;
        [SerializeField] private TipConfig _tipConfig;
        [SerializeField] private CarFactory _carFactory;
        [SerializeField] private LotLayoutBuilder _lot;
        [SerializeField] private ShiftClock _clock;
        [SerializeField] private GameObject _valet;
        [SerializeField] private PatronConfig _patronConfig;
        [SerializeField] private PatronSpawner _spawner;
        [SerializeField, Tooltip("Title screen, HUD and results screen on the 480x270 pixel canvas.")]
        private GameUi _ui;
        [SerializeField] private TuningPanel _tuningPanel;
        [SerializeField] private CameraFollow _camera;
        [SerializeField] private AudioDirector _audio;
        [SerializeField, Tooltip("Optional diner scenery; widens the camera frame to show the road and the diner.")]
        private LotDecor _decor;
        [SerializeField, Tooltip("Optional scenery pedestrians; they walk the routes the decor provides.")]
        private PedestrianCrowd _pedestrians;
        [SerializeField, Tooltip("The Patron Director shell; paces arrivals while DirectorConfig is enabled.")]
        private PatronDirector _director;
        [SerializeField] private DirectorConfig _directorConfig;
        [SerializeField, Tooltip("The lot's director settings: intensity curve, weights, arrival budget.")]
        private DirectorProfile _directorProfile;
        [SerializeField, Tooltip("F3 debug overlay for the director.")]
        private DirectorOverlay _directorOverlay;

        private const int DefaultSpotSeed = 9;

        private ShiftScore _score;
        private ShiftFlow _flow;
        private ShiftRandom _random;
        private DirectorBootstrap _directorBoot;

        private void Awake()
        {
            if (_shiftConfig == null)
            {
                Debug.LogError("GameBootstrap has no ShiftConfig: rebuild with Uncanny Valet/Setup/Build Everything.", this);
                enabled = false;
                return;
            }
            Services.Reset();
            var bus = new EventBus();
            var keyboard = _valet.GetComponent<KeyboardInputSource>();
            var input = new GatedInputSource(keyboard, () => _tuningPanel.IsTyping || _ui.IsTitleShowing);
            var tuning = new TuningStore(bus);
            _score = new ShiftScore(bus, _shiftConfig);
            _carFactory.Bus = bus;

            Services.Register<IEventBus>(bus);
            Services.Register<ITuning>(tuning);
            SeedTuning(tuning, 0);
            Services.Register<ICarFactory>(_carFactory);
            Services.Register<ILotLayout>(_lot);
            Services.Register<IInputSource>(input);
            Services.Register<IGameClock>(_clock);
            Services.Register<ITipCalculator>(new TipCalculator(_tipConfig));
            Services.Register<IShiftScoreView>(_score);

            _carFactory.gameObject.SetActive(true);
            _lot.gameObject.SetActive(true);
            SeedTuning(tuning, _lot.RegularSpotCount);
            FrameLot();
            StartPedestrians();
            _clock.gameObject.SetActive(true);
            _clock.Initialize(bus, _shiftConfig);
            SpawnValet();
            _random = new ShiftRandom(bus, tuning);
            _spawner.Random = _random;
            StartDirector(bus, tuning);
            _spawner.gameObject.SetActive(true);
            StartUi();
            _tuningPanel.gameObject.SetActive(true);
            _audio.gameObject.SetActive(true);
            _flow = new ShiftFlow(bus, new ClockShiftStarter(_clock), new ValetPlacer(_valet, _lot, _camera));
        }

        // The shift (and with it the clock and the patrons) starts only when the title screen is dismissed. By then
        // every component activated above has run Start (StationSwitch subscribes to ShiftPlanChanged there).
        private void OnTitleDismissed() => _flow?.StartFirstShift();

        // Seeds before any module is active so none reads an unseeded key. Called again once the lot knows its
        // spot count, which only matters when the plan says 0 ("all spots open").
        private void SeedTuning(ITuning tuning, int spotCount)
        {
            TuningSeeder.Seed(tuning, _shiftConfig, _patronConfig, _tipConfig,
                spotCount > 0 ? spotCount : DefaultSpotSeed, _directorConfig, _directorProfile);
        }

        private void StartDirector(IEventBus bus, ITuning tuning)
        {
            if (_director == null || _directorConfig == null || _directorProfile == null)
            {
                Debug.LogError("GameBootstrap is missing the director, its config or its profile: rebuild with Uncanny Valet/Setup/Build Everything. Arrivals fall back to the fixed timer.", this);
                tuning.Seed(TuningKey.DirEnabled, 0f);
                return;
            }
            _directorBoot = DirectorBootstrap.Start(bus, tuning, _clock, _lot, _director, _directorProfile, _random,
                _shiftConfig.MaxComboMultiplier);
            if (_directorOverlay != null) _directorOverlay.gameObject.SetActive(true);
        }

        private void FrameLot()
        {
            if (LotBounds.TryMeasure(_lot.gameObject, out var rect))
                _camera.SetWorldBounds(_decor != null ? _decor.ExtendView(rect) : rect, rect);
            else Debug.LogError("Lot has no renderers; the camera cannot frame it.", this);
        }

        private void StartPedestrians()
        {
            if (_pedestrians == null || _decor == null || _decor.Paths == null) return;
            Services.Register<IPedestrianPaths>(_decor.Paths);
            Services.Register<IPatronLocator>(_pedestrians);
            _pedestrians.gameObject.SetActive(true);
            _pedestrians.Begin();
        }

        private void SpawnValet()
        {
            _valet.transform.position = Services.Get<ILotLayout>().ValetStartPoint;
            if (_valet.GetComponent<ValetAnimator>() == null) // the animator owns the sprites when present
            {
                var body = _valet.GetComponentInChildren<SpriteRenderer>();
                body.sprite = PlaceholderSprite.Create();
                body.color = new Color(0.95f, 0.85f, 0.2f);
            }
            _valet.SetActive(true);
            if (_camera != null) _camera.SetTarget(_valet.transform);
        }

        private void StartUi()
        {
            var interactor = _valet.GetComponent<Interactor>();
            _ui.Configure(() => interactor.CurrentPrompt);
            _ui.TitleDismissed += OnTitleDismissed;
            _ui.gameObject.SetActive(true);
        }

        private void OnDestroy()
        {
            if (_ui != null) _ui.TitleDismissed -= OnTitleDismissed;
            _flow?.Dispose();
            _directorBoot?.Dispose();
            _random?.Dispose();
            _score?.Dispose();
            Services.Reset();
        }
    }
}
