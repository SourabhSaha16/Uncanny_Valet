using System;
using UncannyValet.Audio;
using UncannyValet.Cars;
using UncannyValet.Core;
using UncannyValet.Core.Config;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Modifiers;
using UncannyValet.Director;
using UncannyValet.Economy;
using UncannyValet.Lot;
using UncannyValet.Night;
using UncannyValet.Patrons;
using UncannyValet.Pedestrians;
using UncannyValet.Powers;
using UncannyValet.Run;
using UncannyValet.Traffic;
using UncannyValet.UI;
using UncannyValet.Valet;
using UncannyValet.Watchers;
using UnityEngine;
using UnityEngine.Serialization;

namespace UncannyValet.Bootstrap
{
    /// <summary>
    /// The only place concrete types meet. Child objects start inactive (set up by the editor script);
    /// this registers every service first, then activates them in a fixed order.
    /// </summary>
    public sealed class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private ShiftConfig _shiftConfig;
        [SerializeField, FormerlySerializedAs("_tipConfig")] private BlessingConfig _blessingConfig;
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
        [SerializeField, Tooltip("Road traffic: ambient cars and the patron entry and exit trips. Starts after the lot registers the road.")]
        private TrafficModule _traffic;
        [SerializeField, Tooltip("Seeds the traffic keys of the F2 panel.")]
        private TrafficConfig _trafficConfig;
        [SerializeField, Tooltip("Valet powers: the power bar, the activation and the Perfect feedback. Starts once the bus is registered.")]
        private PerfectFeedback _powers;
        [SerializeField, Tooltip("Seeds the power keys of the F2 panel.")]
        private PowerBarConfig _powerConfig;
        [SerializeField, Tooltip("Speech bubble timings and the thresholds of what makes a watcher speak.")]
        private WatcherConfig _watcherConfig;

        [SerializeField, Tooltip("This scene's lot: shift length, pacing, dial positions, layout and blessing threshold.")]
        private LotDefinition _lotDefinition;
        [SerializeField, Tooltip("Every lot of the run, in play order; feeds the title-screen picker and the run state.")]
        private RunDefinition _runDefinition;

        private const int DefaultSpotSeed = 6;

        private ShiftBlessings _blessings;
        private ShiftFlow _flow;
        private ShiftRandom _random;
        private DirectorBootstrap _directorBoot;
        private TrafficTuningLink _trafficLink;
        private DifficultyDialsLink _dialsLink;
        private RunBootstrap _runBoot;
        private IEventBus _bus;

        private void Awake()
        {
            if (_shiftConfig == null || _lotDefinition == null || _runDefinition == null)
            {
                Debug.LogError("GameBootstrap has no ShiftConfig, LotDefinition or RunDefinition: rebuild with Uncanny Valet/Setup/Build Everything.", this);
                enabled = false;
                return;
            }
            if (!_lotDefinition.TryValidate(out var problem)) Debug.LogError("Invalid lot definition: " + problem, _lotDefinition);
            Services.Reset();
            var bus = new EventBus();
            _bus = bus;
            var keyboard = _valet.GetComponent<KeyboardInputSource>();
            Func<bool> inputBlocked = () => _tuningPanel.IsTyping || _ui.IsTitleShowing;
            var input = new GatedInputSource(keyboard, inputBlocked);
            var tuning = new TuningStore(bus);
            _blessings = new ShiftBlessings(bus, _shiftConfig, () => Services.TryGet<ILotGoal>(out var goal) ? goal : null);
            _carFactory.Bus = bus;

            Services.Register<IEventBus>(bus);
            Services.Register<ITuning>(tuning);
            var modifiers = ModifierBootstrap.Register(_lotDefinition);
            SeedTuning(tuning, 0);
            Services.Register<ICarFactory>(_carFactory);
            Services.Register<ILotLayout>(_lot);
            Services.Register<IInputSource>(input);
            Services.Register<IPowerInput>(new KeyboardPowerInput(inputBlocked));
            Services.Register<IGameClock>(_clock);
            Services.Register<IBlessingCalculator>(new BlessingCalculator(_blessingConfig, null, modifiers));
            Services.Register<IShiftBlessingsView>(_blessings);
            Services.Register<IPlayerFocus>(new TransformPlayerFocus(_valet.transform));

            _carFactory.gameObject.SetActive(true);
            _lot.gameObject.SetActive(true);
            Services.Register<IRoadLayout>(_lot.Road);
            QueueSlotCheck.Verify(_lotDefinition, _lot, this);
            SeedTuning(tuning, _lot.RegularSpotCount);
            _trafficLink = TrafficBootstrap.Start(bus, _traffic, this); // after the lot registered the road
            if (!LotBounds.TryFrame(_lot.gameObject, _decor, _camera)) Debug.LogError("Lot has no renderers; the camera cannot frame it.", this);
            PedestrianBootstrap.Start(_pedestrians, _decor);
            _powers.gameObject.SetActive(true);
            _clock.gameObject.SetActive(true);
            _clock.Initialize(bus, _shiftConfig);
            ValetSpawner.Spawn(_valet, _camera);
            _random = new ShiftRandom(bus, tuning);
            _spawner.Random = _random;
            StartDirector(bus, tuning);
            _spawner.gameObject.SetActive(true);
            _runBoot = RunBootstrap.Start(bus, _runDefinition, _lotDefinition); // before the UI, which reads the lot picker
            StartUi();
            WatcherBootstrap.Start(_lotDefinition, _watcherConfig, _pedestrians, _lot.transform);
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
            _dialsLink?.Dispose();
            _dialsLink = TuningSeeder.Seed(tuning, _shiftConfig, _patronConfig, _blessingConfig,
                spotCount > 0 ? spotCount : DefaultSpotSeed, _directorConfig, _directorProfile,
                _pedestrians != null ? _pedestrians.StartCount : 0, _trafficConfig, Services.Get<IEventBus>(), _carFactory.HandlingConfig,
                _audio != null ? _audio.Config : null, powers: _powerConfig, perfect: _lot.Perfect,
                emotions: _ui.Emotions, queueClear: _lot.QueueClear, lot: _lotDefinition, modifiers: Services.Get<IModifierCatalog>());
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
                _shiftConfig.MaxFavourMultiplier);
            if (_directorOverlay != null) _directorOverlay.gameObject.SetActive(true);
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
            _trafficLink?.Dispose();
            _dialsLink?.Dispose();
            _random?.Dispose();
            _blessings?.Dispose();
            _runBoot?.Dispose();
            // A newer lot scene may already have registered its own services; only the owner of the current bus clears them.
            if (Services.TryGet<IEventBus>(out var current) && ReferenceEquals(current, _bus)) Services.Reset();
        }
    }
}
