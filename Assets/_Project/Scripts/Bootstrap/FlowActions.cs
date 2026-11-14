using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Run;
using UncannyValet.UI.Flow;
using UnityEngine;

namespace UncannyValet.Bootstrap
{
    /// <summary>What the story flow asks of the game: start or restart the shift, load a lot's scene, begin a fresh run.</summary>
    public sealed class FlowActions : IFlowActions
    {
        private readonly IEventBus _bus;
        private readonly ShiftFlow _shift;
        private readonly RunHost _host;

        public FlowActions(IEventBus bus, ShiftFlow shift, RunHost host)
        {
            _bus = bus;
            _shift = shift;
            _host = host;
        }

        public void StartShift() => _shift.StartFirstShift();

        public void RetryShift() => _bus.Publish(new RetryShiftRequested());

        public void LoadLot(LotId lot)
        {
            if (!_host.TryChoose(lot)) Debug.LogError("The run does not list lot " + lot + "; its scene cannot be loaded.");
        }

        public void StartNewRun() => _host.Progress.StartNewRun();
    }
}
