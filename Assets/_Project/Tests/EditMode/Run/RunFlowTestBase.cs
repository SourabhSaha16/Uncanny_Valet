using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Core;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Run;
using UnityEditor;

namespace UncannyValet.Tests.Run
{
    /// <summary>Shared setup for the run-flow tests: a real event bus, a RunProgress and a RunFlow over a run definition.</summary>
    public abstract class RunFlowTestBase
    {
        protected static readonly LotId MT = new LotId(World.Midgard, LotKind.Training);
        protected static readonly LotId MC = new LotId(World.Midgard, LotKind.Crazy);
        protected static readonly LotId VT = new LotId(World.Vanaheim, LotKind.Training);
        protected static readonly LotId VC = new LotId(World.Vanaheim, LotKind.Crazy);
        protected static readonly LotId NT = new LotId(World.Niflheim, LotKind.Training);
        protected static readonly LotId NC = new LotId(World.Niflheim, LotKind.Crazy);
        protected static readonly LotId Asgard = new LotId(World.Asgard, LotKind.Finale);

        protected RunFixtures _fx;
        protected EventBus _bus;
        protected RunProgress _progress;
        protected RunFlow _flow;
        protected readonly List<NextLotChosen> _chosen = new List<NextLotChosen>();
        protected readonly List<RunCompleted> _completed = new List<RunCompleted>();
        protected readonly List<PowerGranted> _granted = new List<PowerGranted>();

        [SetUp]
        public void SetUp()
        {
            _fx = new RunFixtures();
            _chosen.Clear();
            _completed.Clear();
            _granted.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            _flow?.Dispose();
            _fx.Dispose();
        }

        protected void Start(RunDefinition run)
        {
            _bus = new EventBus();
            _progress = new RunProgress(run);
            _flow = new RunFlow(_bus, _progress, run);
            _bus.Subscribe<NextLotChosen>(e => _chosen.Add(e));
            _bus.Subscribe<RunCompleted>(e => _completed.Add(e));
            _bus.Subscribe<PowerGranted>(e => _granted.Add(e));
        }

        protected void StartReal() =>
            Start(AssetDatabase.LoadAssetAtPath<RunDefinition>(VanaheimAssets.RunPath));

        protected NextLotChosen Clear(LotId lot) { _bus.Publish(new LotCleared(lot)); return _chosen[_chosen.Count - 1]; }
        protected NextLotChosen Fail(LotId lot) { _bus.Publish(new LotFailed(lot)); return _chosen[_chosen.Count - 1]; }

        protected static void AssertPlay(NextLotChosen e, LotId next, RunOutcome reason)
        {
            Assert.IsTrue(e.HasNext);
            Assert.AreEqual(next, e.Next);
            Assert.AreEqual(reason, e.Reason);
        }
    }
}
