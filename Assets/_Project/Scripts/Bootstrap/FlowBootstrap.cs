using System.Collections.Generic;
using UncannyValet.Core.Events;
using UncannyValet.Powers;
using UncannyValet.UI;

namespace UncannyValet.Bootstrap
{
    /// <summary>Gives the scene's UI the persistent story flow and what it needs to act: the shift, the run host, the lot and power facts.</summary>
    public static class FlowBootstrap
    {
        public static void Wire(GameUi ui, RunBootstrap run, IEventBus bus, ShiftFlow shift, IReadOnlyList<PowerDefinition> powers)
        {
            var facts = new RunStoryFacts(run.Host.Definition, powers);
            ui.UseFlow(run.Flow, new FlowActions(bus, shift, run.Host), facts);
        }
    }
}
