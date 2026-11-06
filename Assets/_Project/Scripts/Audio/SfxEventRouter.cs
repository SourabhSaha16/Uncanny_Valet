using System;
using UncannyValet.Core.Events;

namespace UncannyValet.Audio
{
    /// <summary>Listens to game events on the bus and asks for the matching sound effect.</summary>
    public sealed class SfxEventRouter
    {
        private readonly IEventBus _bus;
        private readonly Action<SfxId> _play;
        private bool _active;

        public SfxEventRouter(IEventBus bus, Action<SfxId> play)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _play = play ?? throw new ArgumentNullException(nameof(play));
        }

        public void Subscribe()
        {
            if (_active) return;
            _active = true;
            _bus.Subscribe<CarEntered>(OnCarEntered);
            _bus.Subscribe<CarExited>(OnCarExited);
            _bus.Subscribe<CarParked>(OnCarParked);
            _bus.Subscribe<TicketOpened>(OnTicketOpened);
            _bus.Subscribe<PickupRequested>(OnPickupRequested);
            _bus.Subscribe<TipEarned>(OnTipEarned);
            _bus.Subscribe<TicketTimerExpired>(OnTimerExpired);
            _bus.Subscribe<NightEnded>(OnNightEnded);
            _bus.Subscribe<NextNightRequested>(OnNextNightRequested);
        }

        public void Unsubscribe()
        {
            if (!_active) return;
            _active = false;
            _bus.Unsubscribe<CarEntered>(OnCarEntered);
            _bus.Unsubscribe<CarExited>(OnCarExited);
            _bus.Unsubscribe<CarParked>(OnCarParked);
            _bus.Unsubscribe<TicketOpened>(OnTicketOpened);
            _bus.Unsubscribe<PickupRequested>(OnPickupRequested);
            _bus.Unsubscribe<TipEarned>(OnTipEarned);
            _bus.Unsubscribe<TicketTimerExpired>(OnTimerExpired);
            _bus.Unsubscribe<NightEnded>(OnNightEnded);
            _bus.Unsubscribe<NextNightRequested>(OnNextNightRequested);
        }

        private void OnCarEntered(CarEntered e) => _play(SfxId.CarEnter);
        private void OnCarExited(CarExited e) => _play(SfxId.CarExit);
        private void OnCarParked(CarParked e) => _play(SfxId.CarParked);
        private void OnTicketOpened(TicketOpened e) => _play(SfxId.CarArrives);
        private void OnPickupRequested(PickupRequested e) => _play(SfxId.PickupRequested);
        private void OnTipEarned(TipEarned e) { if (e.Amount > 0) _play(SfxId.TipEarned); }
        private void OnTimerExpired(TicketTimerExpired e) => _play(SfxId.CarVanished);
        private void OnNightEnded(NightEnded e) => _play(SfxId.NightEnded);
        private void OnNextNightRequested(NextNightRequested e) => _play(SfxId.ButtonClick);
    }
}
