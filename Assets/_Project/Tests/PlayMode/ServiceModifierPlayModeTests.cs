using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Modifiers;
using UncannyValet.Patrons;
using UncannyValet.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace UncannyValet.Tests.PlayMode
{
    /// <summary>
    /// The real training lot, with the real services: a car holding Muddy and NeedsCharge is washed at the wash slot and charged
    /// at a charging bay (the station events are published the way Lot does), and each modifier clears through the real ticket
    /// registry, catalog and tuning. Any logged error fails the test.
    /// </summary>
    public sealed class ServiceModifierPlayModeTests
    {
        private const string ScenePath = "Assets/_Project/Scenes/Bootstrap.unity";
        private const int SpawnAttempts = 120;
        private const int WashMinutes = 16;
        private const int ChargeMinutes = 17;
        private const float StayMinutes = 600f;
        private const float PickupWindowMinutes = 600f;

        private readonly List<string> _problems = new List<string>();

        private void OnLog(string message, string stack, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                _problems.Add(type + ": " + message + "\n" + stack);
        }

        [UnityTearDown]
        public IEnumerator RemoveTheScene()
        {
            Application.logMessageReceived -= OnLog;
            var roots = SceneManager.GetActiveScene().GetRootGameObjects();
            for (var i = 0; i < roots.Length; i++) Object.Destroy(roots[i]);
            yield return null;
            Services.Reset();
        }

        [UnityTest]
        public IEnumerator TheLotRegistersTheCatalogAndSeedsTheChances()
        {
            Application.logMessageReceived += OnLog;
            yield return LoadScene();
            var catalog = Services.Get<IModifierCatalog>();
            Assert.AreEqual(2, catalog.All.Count);
            Assert.IsTrue(catalog.TryGet(ModifierId.Muddy, out _));
            Assert.IsTrue(catalog.TryGet(ModifierId.NeedsCharge, out _));
            var tuning = Services.Get<ITuning>();
            Assert.AreEqual(0.3f, tuning.Get(TuningKey.ModifierChanceMuddy), 1e-5f);
            Assert.AreEqual(0.3f, tuning.Get(TuningKey.ModifierChanceNeedsCharge), 1e-5f);
            Application.logMessageReceived -= OnLog;
            Assert.IsEmpty(_problems, string.Join("\n---\n", _problems));
        }

        [UnityTest]
        public IEnumerator AMuddyCarIsWashed_AndACarThatNeedsAChargeIsCharged_EndToEnd()
        {
            Application.logMessageReceived += OnLog;
            yield return LoadScene();
            var bus = Services.Get<IEventBus>();
            var cleared = new List<ModifierCleared>();
            bus.Subscribe<ModifierCleared>(e => cleared.Add(e));
            var spawner = Object.FindFirstObjectByType<PatronSpawner>();
            var request = new PatronRequest(StayMinutes, 20, false, PickupWindowMinutes,
                ModifierSet.Of(ModifierId.Muddy, ModifierId.NeedsCharge));
            Ticket ticket = null;
            for (var i = 0; i < SpawnAttempts && ticket == null; i++)
            {
                if (!spawner.TrySpawn(request, out ticket)) yield return null;
            }
            Assert.IsNotNull(ticket, "no queue slot ever cleared for the test car");
            Assert.AreEqual(2, ticket.Modifiers.Count);

            bus.Publish(new CarEnteredStation(ticket.CarId, StationKind.Wash));
            for (var m = 0; m < WashMinutes; m++) bus.Publish(new ShiftTick(m));
            Assert.IsTrue(cleared.Exists(e => e.Car == ticket.CarId && e.Modifier == ModifierId.Muddy), "the wash cleared Muddy");
            Assert.IsFalse(cleared.Exists(e => e.Car == ticket.CarId && e.Modifier == ModifierId.NeedsCharge), "the wash does not charge");

            bus.Publish(new CarLeftStation(ticket.CarId, StationKind.Wash));
            bus.Publish(new CarEnteredStation(ticket.CarId, StationKind.Charging));
            for (var m = 0; m < ChargeMinutes; m++) bus.Publish(new ShiftTick(m));
            Assert.IsTrue(cleared.Exists(e => e.Car == ticket.CarId && e.Modifier == ModifierId.NeedsCharge), "the bay cleared NeedsCharge");
            Assert.IsTrue(ticket.OpenModifiers.IsEmpty);

            Application.logMessageReceived -= OnLog;
            Assert.IsEmpty(_problems, string.Join("\n---\n", _problems));
        }

        private static IEnumerator LoadScene()
        {
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(
                ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            yield return null;
            var ui = Object.FindFirstObjectByType<GameUi>();
            typeof(GameUi).GetMethod("StartPlaying", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(ui, null);
            yield return null;
        }
    }
}
