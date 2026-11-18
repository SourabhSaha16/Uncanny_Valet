using System.Collections;
using System.Collections.Generic;
using UncannyValet.Cars;
using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Core.Modifiers;
using UncannyValet.Director;
using UncannyValet.Run;
using UncannyValet.UI;
using UncannyValet.UI.Sandbox;
using UncannyValet.Valet;
using UnityEngine;

namespace UncannyValet.Bootstrap.Sandbox
{
    /// <summary>
    /// The sandbox's counterpart of <see cref="GameBootstrap"/>: the same services, camera, valet and car factory, an empty lot, and no shift (no patrons,
    /// director, pedestrians, traffic, clock, scoring, story or title). It shows one world at a time, applies that world's driving numbers live,
    /// and keeps one car, driven by the valet, that can be reset.
    /// </summary>
    public sealed class SandboxBootstrap : MonoBehaviour, ISandboxControl
    {
        /// <summary>Spots the tuning seed assumes when a plan says "all spots open"; the sandbox has no plan, the F2 rows only need a value.</summary>
        private const int SeedSpotCount = 6;

        [SerializeField] private CarFactory _carFactory;
        [SerializeField] private GameObject _valet;
        [SerializeField] private CameraFollow _camera;
        [SerializeField] private TuningPanel _tuningPanel;
        [SerializeField] private SandboxPicker _picker;
        [SerializeField, Tooltip("The inactive template of every world, in picker order. Built from the SandboxWorldsConfig by the scene builder.")]
        private List<SandboxWorldSlot> _worlds = new List<SandboxWorldSlot>();
        [SerializeField] private SandboxSeedAssets _seed;

        private SandboxWorldSelection _selection;
        private SandboxWorldHost _host;
        private DifficultyDialsLink _dialsLink;
        private ITuning _tuning;
        private IEventBus _bus;
        private IDrivable _car;
        private CarId _carId;
        private int _carCounter;
        private bool _busy = true;

        public int WorldCount => _worlds.Count;

        public int CurrentWorld => _selection.Current;

        public string WorldName(int index) => _worlds[index].Definition.DisplayName;

        private void Awake()
        {
            if (!IsSetUp())
            {
                Debug.LogError("SandboxBootstrap is missing a reference or a world: rebuild with Uncanny Valet/Setup/Build Everything.", this);
                enabled = false;
                return;
            }
            Services.Reset();
            _bus = new EventBus();
            _tuning = new TuningStore(_bus);
            _selection = new SandboxWorldSelection(_worlds.Count);
            var first = _worlds[0].Definition;
            var input = new GatedInputSource(_valet.GetComponent<KeyboardInputSource>(), () => _tuningPanel.IsTyping);
            _carFactory.Bus = _bus;

            Services.Register<IEventBus>(_bus);
            Services.Register<ITuning>(_tuning);
            var modifiers = ModifierBootstrap.Register(first);
            SeedTuning(first, modifiers);
            Services.Register<ICarFactory>(_carFactory);
            Services.Register<IInputSource>(input);
            Services.Register<IPlayerFocus>(new TransformPlayerFocus(_valet.transform));
            Services.Register<ISandboxControl>(this);

            _carFactory.gameObject.SetActive(true);
            _host = new SandboxWorldHost(transform, _worlds, _camera);
            _host.Show(0);
            ValetSpawner.Spawn(_valet, _camera);
            _tuningPanel.gameObject.SetActive(true);
            _picker.gameObject.SetActive(true);
        }

        // One frame later every Start has run (the car factory's handling link, the valet's seat), so the car can be put in at once.
        private IEnumerator Start()
        {
            yield return null;
            SpawnCar();
            _busy = false;
        }

        /// <inheritdoc />
        public bool TrySelectWorld(int index)
        {
            if (_busy || !_selection.TrySelect(index)) return false;
            StartCoroutine(Restart(index));
            return true;
        }

        /// <inheritdoc />
        public void ResetCar()
        {
            if (!_busy) StartCoroutine(Restart(_selection.Current));
        }

        // The old car goes first and the new one comes a frame later: the valet leaves the destroyed car before he can take the new one.
        private IEnumerator Restart(int world)
        {
            _busy = true;
            _carFactory.Despawn(_carId);
            if (world != _host.ShownIndex)
            {
                _host.Show(world);
                SandboxWorldTuning.Apply(_tuning, _worlds[world].Definition, _seed.Shift.GameMinutesPerSecond);
            }
            yield return null;
            SpawnCar();
            _busy = false;
        }

        private void SpawnCar()
        {
            var layout = _host.Layout;
            _carId = new CarId(++_carCounter);
            _car = _carFactory.Spawn(_carId, layout.ArrivalPoint, layout.ArrivalRotationDegrees);
            _valet.transform.position = _car.Transform.position;
            _camera.SetTarget(_valet.transform);
            _car.SetDriver(true);
        }

        private bool IsSetUp()
        {
            if (_carFactory == null || _valet == null || _camera == null || _tuningPanel == null || _picker == null) return false;
            if (_seed == null || !_seed.IsComplete || _worlds.Count == 0) return false;
            for (var i = 0; i < _worlds.Count; i++)
                if (_worlds[i] == null || _worlds[i].Definition == null || _worlds[i].Source == null) return false;
            return true;
        }

        // Every number of the config assets goes in once, from the first world, so no panel row is unseeded; later worlds overwrite the lot's own.
        private void SeedTuning(LotDefinition first, IModifierCatalog modifiers)
        {
            _dialsLink = TuningSeeder.Seed(_tuning, _seed.Shift, _seed.Patrons, _seed.Blessings, SeedSpotCount,
                _seed.Director, _seed.Profile, 0, _seed.Traffic, _bus, _carFactory.HandlingConfig, _seed.Audio,
                powers: _seed.Powers, perfect: _worlds[0].Source.Perfect, emotions: _seed.Emotions, lot: first, modifiers: modifiers);
        }

        private void OnDestroy()
        {
            _dialsLink?.Dispose();
            if (Services.TryGet<IEventBus>(out var current) && ReferenceEquals(current, _bus)) Services.Reset();
        }
    }
}
