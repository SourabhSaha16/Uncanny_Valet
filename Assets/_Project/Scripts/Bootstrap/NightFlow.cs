using System;
using UncannyValet.Core.Events;

namespace UncannyValet.Bootstrap
{
    /// <summary>
    /// The night-to-night flow. On NextNightRequested it starts the next night, or a new run after the
    /// final one. The valet is only moved when he is on foot; a driven car is ejected by the despawn code.
    /// </summary>
    public sealed class NightFlow : IDisposable
    {
        private readonly IEventBus _bus;
        private readonly INightStarter _starter;
        private readonly IValetPlacer _valet;
        private readonly IRunReset _run;
        private readonly int _totalNights;
        private bool _driving;
        private bool _runLost;

        public NightFlow(IEventBus bus, INightStarter starter, IValetPlacer valet, IRunReset run, int totalNights)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _starter = starter ?? throw new ArgumentNullException(nameof(starter));
            _valet = valet ?? throw new ArgumentNullException(nameof(valet));
            _run = run ?? throw new ArgumentNullException(nameof(run));
            _totalNights = Math.Max(1, totalNights);
            _bus.Subscribe<NextNightRequested>(OnNextNight);
            _bus.Subscribe<CarEntered>(OnCarEntered);
            _bus.Subscribe<CarExited>(OnCarExited);
            _bus.Subscribe<NightSummaryReady>(OnSummary);
        }

        public void Dispose()
        {
            _bus.Unsubscribe<NextNightRequested>(OnNextNight);
            _bus.Unsubscribe<CarEntered>(OnCarEntered);
            _bus.Unsubscribe<CarExited>(OnCarExited);
            _bus.Unsubscribe<NightSummaryReady>(OnSummary);
        }

        public void StartFirstNight() => _starter.StartNight(1);

        private void OnCarEntered(CarEntered e) => _driving = true;
        private void OnCarExited(CarExited e) => _driving = false;
        private void OnSummary(NightSummaryReady e) => _runLost = e.Summary.RunLost;

        private void OnNextNight(NextNightRequested e)
        {
            if (_starter.IsRunning) return;
            if (!_driving) _valet.PlaceAtStart();
            if (_runLost || e.FinishedNight >= _totalNights)
            {
                _runLost = false;
                _run.ResetRun();
                _starter.StartNight(1);
                return;
            }
            _starter.StartNight(e.FinishedNight + 1);
        }
    }
}
