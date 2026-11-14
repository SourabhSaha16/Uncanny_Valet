using System;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Core.Modifiers;

namespace UncannyValet.Audio
{
    /// <summary>Listens to game events on the bus and asks for the matching sound effect.</summary>
    public sealed class SfxEventRouter
    {
        private readonly IEventBus _bus;
        private readonly Action<SfxId> _play;
        private readonly Func<float> _clock;
        private readonly IModifierCatalog _modifiers;
        private readonly SfxRateLimiter _bumpLimit = new SfxRateLimiter(BumpMinInterval);
        private readonly SfxRateLimiter _yelpLimit = new SfxRateLimiter(YelpMinInterval);
        private readonly SfxRateLimiter _carBumpLimit = new SfxRateLimiter(CarBumpMinInterval);
        private readonly SfxRateLimiter _chirpLimit = new SfxRateLimiter(ChirpMinInterval);
        private readonly SfxRateLimiter _screechLimit = new SfxRateLimiter(ScreechMinInterval);
        private bool _active;

        /// <summary>Fewest seconds between two pedestrian bump sounds.</summary>
        public const float BumpMinInterval = 0.15f;

        /// <summary>Fewest seconds between two pedestrian yelps.</summary>
        public const float YelpMinInterval = 0.2f;

        /// <summary>Fewest seconds between two car-on-car bump sounds (the Cars module already limits each pair).</summary>
        public const float CarBumpMinInterval = 0.1f;

        /// <summary>Fewest seconds between two slide chirps.</summary>
        public const float ChirpMinInterval = 0.25f;

        /// <summary>Fewest seconds between two hard-braking screeches.</summary>
        public const float ScreechMinInterval = 0.6f;

        /// <param name="clock">Seconds clock used to rate-limit repeated sounds; without one nothing is limited.</param>
        /// <param name="modifiers">The modifier definitions, to tell a charge from a wash when a modifier clears; null plays the wash sound.</param>
        public SfxEventRouter(IEventBus bus, Action<SfxId> play, Func<float> clock = null, IModifierCatalog modifiers = null)
        {
            _modifiers = modifiers;
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
            _bus.Subscribe<BlessingEarned>(OnBlessingEarned);
            _bus.Subscribe<TicketTimerExpired>(OnTimerExpired);
            _bus.Subscribe<ShiftEnded>(OnShiftEnded);
            _bus.Subscribe<RetryShiftRequested>(OnRetryShiftRequested);
            _bus.Subscribe<ModifierCleared>(OnModifierCleared);
            _bus.Subscribe<CarEnteredStation>(OnCarEnteredStation);
            _bus.Subscribe<PedestrianBumped>(OnPedestrianBumped);
            _bus.Subscribe<PedestrianHit>(OnPedestrianHit);
            _bus.Subscribe<CarsCollided>(OnCarsCollided);
            _bus.Subscribe<CarSlideStarted>(OnSlideStarted);
            _bus.Subscribe<CarHardBraked>(OnHardBraked);
            _bus.Subscribe<PerfectPark>(OnPerfectPark);
            _bus.Subscribe<PerfectDropOff>(OnPerfectDropOff);
            _bus.Subscribe<PowerActivated>(OnPowerActivated);
            _bus.Subscribe<PowerEnded>(OnPowerEnded);
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
            _bus.Unsubscribe<BlessingEarned>(OnBlessingEarned);
            _bus.Unsubscribe<TicketTimerExpired>(OnTimerExpired);
            _bus.Unsubscribe<ShiftEnded>(OnShiftEnded);
            _bus.Unsubscribe<RetryShiftRequested>(OnRetryShiftRequested);
            _bus.Unsubscribe<ModifierCleared>(OnModifierCleared);
            _bus.Unsubscribe<CarEnteredStation>(OnCarEnteredStation);
            _bus.Unsubscribe<PedestrianBumped>(OnPedestrianBumped);
            _bus.Unsubscribe<PedestrianHit>(OnPedestrianHit);
            _bus.Unsubscribe<CarsCollided>(OnCarsCollided);
            _bus.Unsubscribe<CarSlideStarted>(OnSlideStarted);
            _bus.Unsubscribe<CarHardBraked>(OnHardBraked);
            _bus.Unsubscribe<PerfectPark>(OnPerfectPark);
            _bus.Unsubscribe<PerfectDropOff>(OnPerfectDropOff);
            _bus.Unsubscribe<PowerActivated>(OnPowerActivated);
            _bus.Unsubscribe<PowerEnded>(OnPowerEnded);
        }

        private void OnCarEntered(CarEntered e) => _play(SfxId.CarEnter);
        private void OnCarExited(CarExited e) => _play(SfxId.CarExit);
        private void OnCarParked(CarParked e) => _play(SfxId.CarParked);
        private void OnCarArrived(CarArrived e) => _play(SfxId.CarArrives);
        private void OnPickupRequested(PickupRequested e) => _play(SfxId.PickupRequested);
        private void OnBlessingEarned(BlessingEarned e) { if (e.Amount > 0) _play(SfxId.BlessingEarned); }
        private void OnTimerExpired(TicketTimerExpired e) => _play(SfxId.CarVanished);
        private void OnShiftEnded(ShiftEnded e) => _play(SfxId.NightEnded);
        private void OnRetryShiftRequested(RetryShiftRequested e) => _play(SfxId.ButtonClick);
        private void OnModifierCleared(ModifierCleared e)
        {
            var charged = _modifiers != null && _modifiers.TryGet(e.Modifier, out var def) && def.ClearedAt == StationKind.Charging;
            _play(charged ? SfxId.Charged : SfxId.Washed);
        }
        private void OnCarEnteredStation(CarEnteredStation e) => _play(SfxId.StationStart);

        private void OnPerfectPark(PerfectPark e) => _play(SfxId.PerfectDing);
        private void OnPerfectDropOff(PerfectDropOff e) => _play(SfxId.PerfectDing);

        private void OnPowerActivated(PowerActivated e)
        {
            if (e.Power == StoryPower.FreezeTime) _play(SfxId.TimeStop);
            else if (e.Power == StoryPower.ThunderStride) _play(SfxId.ThunderCrack);
        }

        private void OnPowerEnded(PowerEnded e)
        {
            if (e.Power == StoryPower.FreezeTime) _play(SfxId.TimeResume);
        }

        private void OnSlideStarted(CarSlideStarted e)
        {
            if (_clock == null || _chirpLimit.TryPass(_clock())) _play(SfxId.TyreChirp);
        }

        private void OnHardBraked(CarHardBraked e)
        {
            if (_clock == null || _screechLimit.TryPass(_clock())) _play(SfxId.TyreScreech);
        }

        private void OnCarsCollided(CarsCollided e)
        {
            if (_clock == null || _carBumpLimit.TryPass(_clock())) _play(SfxId.CarBump);
        }

        private void OnPedestrianHit(PedestrianHit e)
        {
            if (_clock == null || _yelpLimit.TryPass(_clock())) _play(SfxId.PedestrianYelp);
        }

        private void OnPedestrianBumped(PedestrianBumped e)
        {
            if (_clock == null || _bumpLimit.TryPass(_clock())) _play(SfxId.PedestrianBump);
        }
    }
}
