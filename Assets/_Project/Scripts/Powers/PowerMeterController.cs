using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UnityEngine;

namespace UncannyValet.Powers
{
    /// <summary>Fills the power meter from game events and publishes PowerMeterChanged. The meter starts every shift empty.</summary>
    [DisallowMultipleComponent]
    public sealed class PowerMeterController : MonoBehaviour
    {
        [SerializeField] private PowerMeterConfig _config;

        private IEventBus _bus;
        private DriftFillTracker _drift;
        private bool _started;
        private bool _subscribed;

        /// <summary>The live numbers (config, then F2 changes).</summary>
        public PowerSettings Settings { get; private set; }

        /// <summary>The meter rule.</summary>
        public PowerMeter Meter { get; private set; }

        private void Awake()
        {
            if (_config == null)
            {
                Debug.LogError("PowerMeterController on " + name + " has no PowerMeterConfig assigned.", this);
                enabled = false;
                return;
            }
            Settings = new PowerSettings(_config);
            Meter = new PowerMeter(Settings);
            _drift = new DriftFillTracker(Settings);
        }

        private void Start()
        {
            if (Meter == null) return;
            _bus = Services.Get<IEventBus>();
            if (Services.TryGet<ITuning>(out var tuning)) Settings.Load(tuning);
            _started = true;
            Subscribe();
        }

        private void OnEnable() { if (_started) Subscribe(); }

        private void OnDisable() => Unsubscribe();

        private void Update()
        {
            if (_drift == null || _drift.Active == 0) return;
            Fill(_drift.Tick(Time.deltaTime));
        }

        /// <summary>Adds the Perfect Park amount. SEAM: the lot agent's PerfectPark event (issue 16) is not in Core yet; when it lands, subscribe to it here and call this.</summary>
        public void AddPerfectPark() => Fill(Settings.FillPerfectPark);

        /// <summary>Spends the meter if a level is reached: empties it and returns the level. False when nothing is unlocked.</summary>
        public bool TrySpend(out int level)
        {
            level = Meter.Level;
            if (level == 0) return false;
            Meter.Reset();
            Publish();
            return true;
        }

        private void Fill(float amount)
        {
            if (Meter.Add(amount)) Publish();
        }

        private void Publish() => _bus.Publish(new PowerMeterChanged(Meter.Level, Meter.Fill01));

        private void OnPerfectDropOff(PerfectDropOff e) => Fill(Settings.FillPerfectDropOff);
        private void OnSlideStarted(CarSlideStarted e) => _drift.Begin(e.Car);
        private void OnSlideEnded(CarSlideEnded e) => _drift.End(e.Car);

        private void OnShiftStarted(ShiftStarted e)
        {
            _drift.Clear();
            if (Meter.Reset()) Publish();
        }

        private void OnTuningChanged(TuningChanged e) => Settings.Apply(e.Key, e.Value);

        private void Subscribe()
        {
            if (_subscribed) return;
            _subscribed = true;
            _bus.Subscribe<PerfectDropOff>(OnPerfectDropOff);
            _bus.Subscribe<CarSlideStarted>(OnSlideStarted);
            _bus.Subscribe<CarSlideEnded>(OnSlideEnded);
            _bus.Subscribe<ShiftStarted>(OnShiftStarted);
            _bus.Subscribe<TuningChanged>(OnTuningChanged);
        }

        private void Unsubscribe()
        {
            if (!_subscribed) return;
            _subscribed = false;
            _bus.Unsubscribe<PerfectDropOff>(OnPerfectDropOff);
            _bus.Unsubscribe<CarSlideStarted>(OnSlideStarted);
            _bus.Unsubscribe<CarSlideEnded>(OnSlideEnded);
            _bus.Unsubscribe<ShiftStarted>(OnShiftStarted);
            _bus.Unsubscribe<TuningChanged>(OnTuningChanged);
        }
    }
}
