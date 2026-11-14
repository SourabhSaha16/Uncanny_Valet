using System;
using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Run;
using UncannyValet.UI.Flow;
using UnityEngine;

namespace UncannyValet.Bootstrap
{
    /// <summary>
    /// Connects a lot scene to the run: finds (or, in the first scene, creates) the persistent <see cref="RunHost"/>,
    /// registers the run services (including the lot goal), links the scene's events to the run state and publishes LotStarted.
    /// </summary>
    public sealed class RunBootstrap : IDisposable
    {
        private const string HostName = "RunHost";
        private readonly RunProgressLink _link;
        private readonly RunFlow _flow;
        private readonly DevPowerLink _devPowers;

        private RunBootstrap(RunHost host, FlowHost flowHost, RunProgressLink link, RunFlow flow, DevPowerLink devPowers)
        {
            Host = host; Flow = flowHost.Machine; _link = link; _flow = flow; _devPowers = devPowers;
        }

        /// <summary>The persistent run host: the run definition, the run state and the lot loader.</summary>
        public RunHost Host { get; }

        /// <summary>The story flow state, which survives every scene load.</summary>
        public FlowMachine Flow { get; }

        /// <summary>
        /// The only lookup of the persistent host: a scene load destroys every scene object, so the host cannot be
        /// handed over through Services (a previous scene's teardown may clear it).
        /// </summary>
        public static RunBootstrap Start(IEventBus bus, RunDefinition run, LotDefinition lot)
        {
            var host = UnityEngine.Object.FindFirstObjectByType<RunHost>();
            if (host == null)
            {
                host = new GameObject(HostName).AddComponent<RunHost>();
                host.Initialize(run);
                host.gameObject.AddComponent<FlowHost>().Initialize(() => StartLotRule.First(run, host.Progress));
            }
            if (host.Definition.IndexOf(lot.Id) < 0)
                Debug.LogError("The run definition does not list lot " + lot.Id + " (scene " + lot.name + ").", lot);
            Services.Register<IRunProgress>(host.Progress);
            Services.Register<IRunFlowState>(host.Progress);
            Services.Register<ILotPicker>(host);
            Services.Register<ILotGoal>(new LotGoal(lot.Id, lot.BlessingThreshold));
            var link = new RunProgressLink(bus, host.Progress);
            var flow = new RunFlow(bus, host.Progress, host.Definition);
            var devPowers = Services.TryGet<ITuning>(out var tuning) ? new DevPowerLink(bus, tuning, host.Progress) : null;
            bus.Publish(new LotStarted(lot.Id));
            return new RunBootstrap(host, host.GetComponent<FlowHost>(), link, flow, devPowers);
        }

        public void Dispose()
        {
            _link.Dispose();
            _flow.Dispose();
            _devPowers?.Dispose();
        }
    }
}
