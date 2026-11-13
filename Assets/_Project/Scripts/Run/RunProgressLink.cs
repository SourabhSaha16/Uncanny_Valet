using System;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Run
{
    /// <summary>
    /// Feeds one scene's events into the persistent <see cref="RunProgress"/>: LotStarted sets the current lot,
    /// LotCleared marks it cleared, ShiftResultReady records the best blessings, PowerGranted earns or upgrades a power, PowerRevoked takes one away (developer panel). Created per scene; dispose with the scene.
    /// </summary>
    public sealed class RunProgressLink : IDisposable
    {
        private readonly IEventBus _bus;
        private readonly RunProgress _progress;

        public RunProgressLink(IEventBus bus, RunProgress progress)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _progress = progress ?? throw new ArgumentNullException(nameof(progress));
            _bus.Subscribe<LotStarted>(OnStarted);
            _bus.Subscribe<LotCleared>(OnCleared);
            _bus.Subscribe<ShiftResultReady>(OnResult);
            _bus.Subscribe<PowerGranted>(OnGranted);
            _bus.Subscribe<PowerRevoked>(OnRevoked);
        }

        public void Dispose()
        {
            _bus.Unsubscribe<LotStarted>(OnStarted);
            _bus.Unsubscribe<LotCleared>(OnCleared);
            _bus.Unsubscribe<ShiftResultReady>(OnResult);
            _bus.Unsubscribe<PowerGranted>(OnGranted);
            _bus.Unsubscribe<PowerRevoked>(OnRevoked);
        }

        private void OnStarted(LotStarted e) => _progress.SetCurrentLot(e.Lot);
        private void OnCleared(LotCleared e) => _progress.MarkCleared(e.Lot);
        private void OnResult(ShiftResultReady e)
        {
            if (_progress.HasCurrentLot) _progress.RecordBest(_progress.CurrentLot, e.Result.Blessings);
        }
        private void OnGranted(PowerGranted e) => _progress.GrantPower(e.Power, e.Level);
        private void OnRevoked(PowerRevoked e) => _progress.RevokePower(e.Power);
    }
}
