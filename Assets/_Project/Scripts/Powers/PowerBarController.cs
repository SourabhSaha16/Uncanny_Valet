using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UnityEngine;

namespace UncannyValet.Powers
{
    /// <summary>
    /// Fills the shared power bar from Perfect Parks, perfect drop-offs and drifting, and publishes PowerBarChanged.
    /// The bar works only while Thor owns at least one power (IRunProgress) and starts every shift empty.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PowerBarController : MonoBehaviour, IPowerBucket
    {
        [SerializeField] private PowerBarConfig _config;

        private IEventBus _bus;
        private IRunProgress _progress;
        private DriftFillTracker _drift;
        private bool _started;
        private bool _subscribed;

        /// <summary>The live numbers (config, then F2 changes).</summary>
        public PowerSettings Settings { get; private set; }

        /// <summary>The bar rule.</summary>
        public PowerBar Bar { get; private set; }

        /// <summary>True while at least one power is owned; the bar, its fill and the power keys work only then.</summary>
        public bool IsActive => Progress != null && Progress.EarnedPowers.Count > 0;

        /// <summary>The run state; resolved late because a lot scene registers it after this object is created.</summary>
        public IRunProgress Progress
        {
            get
            {
                if (_progress == null) Services.TryGet(out _progress);
                return _progress;
            }
        }

        private void Awake()
        {
            if (_config == null)
            {
                Debug.LogError("PowerBarController on " + name + " has no PowerBarConfig assigned.", this);
                enabled = false;
                return;
            }
            Settings = new PowerSettings(_config);
            Bar = new PowerBar();
            _drift = new DriftFillTracker(Settings);
        }

        private void Start()
        {
            if (Bar == null) return;
            _bus = Services.Get<IEventBus>();
            if (Services.TryGet<ITuning>(out var tuning)) Settings.Load(tuning);
            _started = true;
            Subscribe();
        }

        private void OnEnable() { if (_started) Subscribe(); }

        private void OnDisable() => Unsubscribe();

        private void Update()
        {
            if (_drift == null || _drift.Active == 0 || !IsActive) return;
            var offered = _drift.Tick(Time.deltaTime);
            _drift.Credit(Fill(offered), offered);
        }

        /// <summary>True when a power is owned and a bucket is open.</summary>
        public bool CanSpend => IsActive && Bar != null && Bar.CanSpend;

        /// <summary>Spends one bucket if a power is owned and one is open. False, and nothing changes, otherwise.</summary>
        public bool TrySpend()
        {
            if (!IsActive || !Bar.TrySpend(out var bucket)) return false;
            Publish(bucket);
            return true;
        }

        /// <summary>Adds to the bar and returns what it really took (0 when inactive or full).</summary>
        private float Fill(float amount)
        {
            if (!IsActive) return 0f;
            var before = Bar.Value;
            if (!Bar.Add(amount)) return 0f;
            Publish(0);
            return Bar.Value - before;
        }

        private void FillFrom(float amount, PowerFillSource source) => ReportFilled(Fill(amount), source);

        private void ReportFilled(float taken, PowerFillSource source)
        {
            if (IsActive && PowerFillRule.IsWorthShowing(taken)) _bus.Publish(new PowerFilled(taken, source));
        }

        private void Publish(int spentBucket) => _bus.Publish(new PowerBarChanged(Bar.Value, Bar.OpenBuckets, spentBucket));

        private void OnPerfectPark(PerfectPark e) => FillFrom(Settings.FillPerfectPark, PowerFillSource.PerfectPark);
        private void OnParkedInside(CarParkedInside e) => FillFrom(Settings.FillParkedInside, PowerFillSource.ParkedInside);
        private void OnPerfectDropOff(PerfectDropOff e) => FillFrom(Settings.FillPerfectDropOff, PowerFillSource.PerfectDropOff);
        private void OnSlideStarted(CarSlideStarted e) => _drift.Begin(e.Car);
        private void OnSlideEnded(CarSlideEnded e) => ReportFilled(_drift.End(e.Car), PowerFillSource.Drift);

        private void OnShiftStarted(ShiftStarted e)
        {
            _drift.Clear();
            if (Bar.Reset()) Publish(0);
        }

        private void OnTuningChanged(TuningChanged e) => Settings.Apply(e.Key, e.Value);

        private void Subscribe()
        {
            if (_subscribed) return;
            _subscribed = true;
            _bus.Subscribe<PerfectPark>(OnPerfectPark);
            _bus.Subscribe<CarParkedInside>(OnParkedInside);
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
            _bus.Unsubscribe<PerfectPark>(OnPerfectPark);
            _bus.Unsubscribe<CarParkedInside>(OnParkedInside);
            _bus.Unsubscribe<PerfectDropOff>(OnPerfectDropOff);
            _bus.Unsubscribe<CarSlideStarted>(OnSlideStarted);
            _bus.Unsubscribe<CarSlideEnded>(OnSlideEnded);
            _bus.Unsubscribe<ShiftStarted>(OnShiftStarted);
            _bus.Unsubscribe<TuningChanged>(OnTuningChanged);
        }
    }
}
