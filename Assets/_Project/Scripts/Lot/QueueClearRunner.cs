using System.Collections.Generic;
using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>
    /// Watches every arrived car and publishes <see cref="CarClearedQueue"/> the first time one rests clear of the
    /// arrival queue (see <see cref="QueueClearRule"/>). A watch ends when the car is cleared, parked in a bay
    /// (Patrons starts the stay from that itself), its ticket closes, or a shift starts or ends. Nothing allocates per frame.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class QueueClearRunner : MonoBehaviour
    {
        private sealed class Watch
        {
            public IDrivable Car;
            public QueueClearRule Rule;
        }

        private readonly List<Watch> _watches = new List<Watch>();
        private LotConfig _config;
        private float _clearDistance;
        private IEventBus _bus;
        private ICarFactory _cars;
        private ILotLayout _layout;

        /// <summary>Sets the runner up with the lot's rest numbers and the starting clear distance.</summary>
        public void Init(LotConfig config, QueueClearSettings settings)
        {
            _config = config;
            _clearDistance = settings.ClearDistance;
        }

        private void Start()
        {
            _bus = Services.Get<IEventBus>();
            if (Services.TryGet<ITuning>(out var tuning) && tuning.TryGet(TuningKey.QueueClearDistance, out var d)) _clearDistance = d;
            Subscribe();
        }

        private void OnEnable()
        {
            if (_bus != null) Subscribe();
        }

        private void OnDisable()
        {
            if (_bus == null) return;
            _bus.Unsubscribe<CarArrived>(OnArrived);
            _bus.Unsubscribe<CarParked>(OnParked);
            _bus.Unsubscribe<TicketClosed>(OnClosed);
            _bus.Unsubscribe<ShiftStarted>(OnShiftEdge);
            _bus.Unsubscribe<ShiftEnded>(OnShiftEdge);
            _bus.Unsubscribe<TuningChanged>(OnTuning);
        }

        private void Subscribe()
        {
            _bus.Subscribe<CarArrived>(OnArrived);
            _bus.Subscribe<CarParked>(OnParked);
            _bus.Subscribe<TicketClosed>(OnClosed);
            _bus.Subscribe<ShiftStarted>(OnShiftEdge);
            _bus.Subscribe<ShiftEnded>(OnShiftEdge);
            _bus.Subscribe<TuningChanged>(OnTuning);
        }

        private void OnArrived(CarArrived e)
        {
            _cars = _cars ?? Services.Get<ICarFactory>();
            _layout = _layout ?? Services.Get<ILotLayout>();
            if (IndexOf(e.Car) >= 0 || !_cars.TryGet(e.Car, out var car)) return;
            _watches.Add(new Watch { Car = car, Rule = new QueueClearRule(_config.SettleSeconds) });
        }

        private void OnParked(CarParked e) => Remove(e.Car);

        private void OnClosed(TicketClosed e) => Remove(e.Car);

        private void OnShiftEdge<T>(T e) => _watches.Clear();

        private void OnTuning(TuningChanged e)
        {
            if (e.Key == TuningKey.QueueClearDistance) _clearDistance = e.Value;
        }

        private void Update()
        {
            for (var i = _watches.Count - 1; i >= 0; i--)
            {
                var w = _watches[i];
                if (w.Car is Object gone && gone == null) { _watches.RemoveAt(i); continue; }
                var resting = !w.Car.HasDriver && w.Car.Speed < _config.StoppedSpeed;
                var distance = QueueClearRule.DistanceToNearest(_layout.ArrivalSlots,w.Car.Transform.position);
                if (!w.Rule.Tick(Time.deltaTime, distance, _clearDistance, resting)) continue;
                _watches.RemoveAt(i);
                _bus.Publish(new CarClearedQueue(w.Car.Id));
            }
        }

        private int IndexOf(CarId car)
        {
            for (var i = 0; i < _watches.Count; i++) if (_watches[i].Car.Id == car) return i;
            return -1;
        }

        private void Remove(CarId car)
        {
            var i = IndexOf(car);
            if (i >= 0) _watches.RemoveAt(i);
        }
    }
}
