using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Run;
using UncannyValet.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace UncannyValet.Tests.PlayMode
{
    /// <summary>
    /// Loads each real lot scene and plays a few seconds of the first shift. Any error, exception or assert
    /// logged during start-up or play fails the test: it catches wiring and seeding-order bugs that the unit tests
    /// cannot see. Also switches lots through the picker, as the title screen does.
    /// </summary>
    public sealed class BootstrapSceneSmokeTests
    {
        private const string ScenePath = "Assets/_Project/Scenes/Bootstrap.unity";
        private const string CrazyScenePath = "Assets/_Project/Scenes/Lot_MidgardCrazy.unity";
        private const string CrazySceneName = "Lot_MidgardCrazy";
        private const float PlaySeconds = 4f;
        private const int SwitchFrames = 10;
        private const string StartPlaying = "StartPlaying";

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
            var host = Object.FindFirstObjectByType<RunHost>();
            if (host != null) Object.Destroy(host.gameObject);
            yield return null;
            Services.Reset();
        }

        [UnityTest]
        public IEnumerator BootstrapScene_StartsAndPlaysWithoutErrors() =>
            PlayScene(ScenePath, new LotId(World.Midgard, LotKind.Training));

        [UnityTest]
        public IEnumerator MidgardCrazyScene_StartsAndPlaysWithoutErrors() =>
            PlayScene(CrazyScenePath, new LotId(World.Midgard, LotKind.Crazy));

        [UnityTest]
        public IEnumerator PickerSwitchesToTheCrazyLot_KeepingTheRunState()
        {
            Application.logMessageReceived += OnLog;
            yield return LoadScene(ScenePath);
            var progress = Services.Get<IRunProgress>();
            Assert.AreEqual(new LotId(World.Midgard, LotKind.Training), progress.CurrentLot);
            var picker = Services.Get<ILotPicker>();
            Assert.AreEqual(2, picker.Choices.Count);

            Assert.IsTrue(picker.TryChoose(1));
            for (var i = 0; i < SwitchFrames; i++) yield return null;

            Assert.AreEqual(CrazySceneName, SceneManager.GetActiveScene().name);
            var after = Services.Get<IRunProgress>();
            Assert.AreSame(progress, after, "The run state must survive the scene load.");
            Assert.AreEqual(new LotId(World.Midgard, LotKind.Crazy), after.CurrentLot);
            Assert.AreEqual(1, Object.FindObjectsByType<RunHost>(FindObjectsSortMode.None).Length);
            Application.logMessageReceived -= OnLog;
            Assert.IsEmpty(_problems, string.Join("\n---\n", _problems));
        }

        [UnityTest]
        public IEnumerator TrainingLot_HasThreeQueueSlots_AndAllowsTwoWaiting() => CheckQueue(ScenePath, 3, 2, 3, 8);

        [UnityTest]
        public IEnumerator CrazyLot_HasFourQueueSlots_AndFourCarsCanWait() => CheckQueue(CrazyScenePath, 4, 4, 4, 20);

        // The slot count comes from the lot's definition; cars parked in every slot stay put (nothing overlaps or walls them in).
        private IEnumerator CheckQueue(string path, int slots, int cap, int cars, int minBudget)
        {
            Application.logMessageReceived += OnLog;
            yield return LoadScene(path);
            var layout = Services.Get<ILotLayout>();
            Assert.AreEqual(slots, layout.ArrivalSlots.Count);
            Assert.AreEqual(cap, Services.Get<ITuning>().Get(TuningKey.PlanMaxWaitingCars));
            Assert.GreaterOrEqual(Services.Get<ITuning>().Get(TuningKey.DirArrivalBudget), minBudget, "the lot's own arrival budget must not be capped");
            var factory = Services.Get<ICarFactory>();
            var made = new List<IDrivable>();
            for (var i = 0; i < cars; i++)
                made.Add(factory.Spawn(new CarId(100 + i), layout.ArrivalSlots[i], layout.ArrivalRotationDegrees));
            yield return new WaitForSeconds(0.5f);
            for (var i = 0; i < cars; i++)
                Assert.Less(Vector2.Distance(layout.ArrivalSlots[i], made[i].Transform.position), 0.1f, "car " + i + " was pushed out of its slot");
            Application.logMessageReceived -= OnLog;
            Assert.IsEmpty(_problems, string.Join("\n---\n", _problems));
        }

        private IEnumerator PlayScene(string path, LotId expected)
        {
            Application.logMessageReceived += OnLog;
            yield return LoadScene(path);
            Assert.AreEqual(expected, Services.Get<IRunProgress>().CurrentLot, "LotStarted must record the current lot.");
            var goal = Services.Get<ILotGoal>();
            Assert.AreEqual(expected, goal.Lot, "The lot goal must name the lot that started.");
            Assert.Greater(goal.BlessingThreshold, 0, "The lot goal needs a blessing threshold.");

            var ui = Object.FindFirstObjectByType<GameUi>();
            Assert.IsNotNull(ui, "The lot scene has no GameUi.");
            typeof(GameUi).GetMethod(StartPlaying, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(ui, null);

            var until = Time.time + PlaySeconds;
            while (Time.time < until) yield return null;

            Application.logMessageReceived -= OnLog;
            Assert.IsEmpty(_problems, string.Join("\n---\n", _problems));
        }

        private static IEnumerator LoadScene(string path)
        {
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(
                path, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            yield return null;
        }
    }
}
