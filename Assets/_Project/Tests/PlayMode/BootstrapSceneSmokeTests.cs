using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UncannyValet.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace UncannyValet.Tests.PlayMode
{
    /// <summary>
    /// Loads the real Bootstrap scene and plays a few seconds of the first shift. Any error, exception or assert
    /// logged during start-up or play fails the test: it catches wiring and seeding-order bugs that the unit tests
    /// cannot see.
    /// </summary>
    public sealed class BootstrapSceneSmokeTests
    {
        private const string ScenePath = "Assets/_Project/Scenes/Bootstrap.unity";
        private const float PlaySeconds = 4f;
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
            yield return null;
            UncannyValet.Core.Services.Reset();
        }

        [UnityTest]
        public IEnumerator BootstrapScene_StartsAndPlaysWithoutErrors()
        {
            Application.logMessageReceived += OnLog;
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(
                ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            yield return null;

            var ui = Object.FindFirstObjectByType<GameUi>();
            Assert.IsNotNull(ui, "The Bootstrap scene has no GameUi.");
            typeof(GameUi).GetMethod(StartPlaying, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(ui, null);

            var until = Time.time + PlaySeconds;
            while (Time.time < until) yield return null;

            Application.logMessageReceived -= OnLog;
            Assert.IsEmpty(_problems, string.Join("\n---\n", _problems));
        }
    }
}
