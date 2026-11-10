using System;
using UncannyValet.Core.Events;

namespace UncannyValet.Audio
{
    /// <summary>Listens to game events on the bus and asks for the matching sound effect.</summary>
    public sealed class SfxEventRouter
    {
        private readonly IEventBus _bus;
        private readonly Action<SfxId> _play;
        private readonly Func<float> _clock;
        private readonly SfxRateLimiter _bumpLimit = new SfxRateLimiter(BumpMinInterval);
        private bool _active;

        /// <summary>Fewest seconds between two pedestrian bump sounds.</summary>
        public const float BumpMinInterval = 0.15f;

        /// <param name="clock">Seconds clock used to rate-limit repeated sounds; without one nothing is limited.</param>
        public SfxEventRouter(IEventBus bus, Action<SfxId> play, Func<float> clock = null)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _play = play ?? throw new ArgumentNullException(nameof(play));
            _clock = clock;
        }

        public void Subscribe()
        {
            if (_active) return;
            _active = true;
            _bus.Subscribe<CarEntered>(OnCarEntered);
            _bus.Subscribe<CarExited>(OnCarExited);
            _bus.Subscribe<CarParked>(OnCarParked);
            _bus.Subscribe<CarArrived>(OnCarArrived);
            _bus.Subscribe<PickupRequested>(OnPickupRequested);
            _bus.Subscribe<TipEarned>(OnTipEarned);
            _bus.Subscribe<TicketTimerExpired>(OnTimerExpired);
            _bus.Subscribe<ShiftEnded>(OnShiftEnded);
            _bus.Subscribe<RetryShiftRequested>(OnRetryShiftRequested);
            _bus.Subscribe<CarFullyCharged>(OnCarFullyCharged);
            _bus.Subscribe<CarCleaned>(OnCarCleaned);
            _bus.Subscribe<CarEnteredStation>(OnCarEnteredStation);
            _bus.Subscribe<PedestrianBumped>(OnPedestrianBumped);
        }

        public void Unsubscribe()
        {
            if (!_active) return;
            _active = false;
            _bus.Unsubscribe<CarEntered>(OnCarEntered);
            _bus.Unsubscribe<CarExited>(OnCarExited);
            _bus.Unsubscribe<CarParked>(OnCarParked);
            _bus.Unsubscribe<CarArrived>(OnCarArrived);
            _bus.Unsubscribe<PickupRequested>(OnPickupRequested);
            _bus.Unsubscribe<TipEarned>(OnTipEarned);
            _bus.Unsubscribe<TicketTimerExpired>(OnTimerExpired);
            _bus.Unsubscribe<ShiftEnded>(OnShiftEnded);
            _bus.Unsubscribe<RetryShiftRequested>(OnRetryShiftRequested);
            _bus.Unsubscribe<CarFullyCharged>(OnCarFullyCharged);
            _bus.Unsubscribe<CarCleaned>(OnCarCleaned);
            _bus.Unsubscribe<CarEnteredStation>(OnCarEnteredStation);
            _bus.Unsubscribe<PedestrianBumped>(OnPedestrianBumped);
        }

        private void OnCarEntered(CarEntered e) => _play(SfxId.CarEnter);
        private void OnCarExited(CarExited e) => _play(SfxId.CarExit);
        private void OnCarParked(CarParked e) => _play(SfxId.CarParked);
        private void OnCarArrived(CarArrived e) => _play(SfxId.CarArrives);
        private void OnPickupRequested(PickupRequested e) => _play(SfxId.PickupRequested);
        private void OnTipEarned(TipEarned e) { if (e.Amount > 0) _play(SfxId.TipEarned); }
        private void OnTimerExpired(TicketTimerExpired e) => _play(SfxId.CarVanished);
        private void OnShiftEnded(ShiftEnded e) => _play(SfxId.NightEnded);
        private void OnRetryShiftRequested(RetryShiftRequested e) => _play(SfxId.ButtonClick);
        private void OnCarFullyCharged(CarFullyCharged e) => _play(SfxId.Charged);
        private void OnCarCleaned(CarCleaned e) => _play(SfxId.Washed);
        private void OnCarEnteredStation(CarEnteredStation e) => _play(SfxId.StationStart);

        private void OnPedestrianBumped(PedestrianBumped e)
        {
            if (_clock == null || _bumpLimit.TryPass(_clock())) _play(SfxId.PedestrianBump);
        }
    }
}
