using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UncannyValet.Core;
using UncannyValet.Pedestrians;
using UncannyValet.Run;
using UncannyValet.Traffic;
using UncannyValet.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace UncannyValet.Tests.PlayMode
{
    /// <summary>What the Vanaheim play tests share: loading a lot scene and starting its shift, logged errors, tidying the scene up and counting the crowd and the road cars.</summary>
    public abstract class VanaheimPlayBase
    {
        protected const string TrainingPath = "Assets/_Project/Scenes/Lot_VanaheimTraining.unity";
        protected const string CrazyPath = "Assets/_Project/Scenes/Lot_VanaheimCrazy.unity";

        protected readonly List<string> Problems = new List<string>();

        private void OnLog(string message, string stack, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) Problems.Add(type + ": " + message + "\n" + stack);
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

        protected IEnumerator Play(string path)
        {
            Application.logMessageReceived += OnLog;
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(path, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            yield return null;
            var ui = Object.FindFirstObjectByType<GameUi>();
            typeof(GameUi).GetMethod("StartPlaying", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(ui, null);
            yield return null;
        }

        protected static int VisibleAmbientWalkers()
        {
            var crowd = Object.FindFirstObjectByType<PedestrianCrowd>().transform;
            var count = 0;
            for (var i = 0; i < crowd.childCount; i++)
                if (crowd.GetChild(i).name.StartsWith("Pedestrian") && crowd.GetChild(i).gameObject.activeInHierarchy) count++;
            return count;
        }

        protected static int ActiveAmbientCars()
        {
            var count = 0;
            foreach (var car in Object.FindObjectsByType<TrafficCar>(FindObjectsSortMode.None))
                if (car.gameObject.activeInHierarchy) count++;
            return count;
        }
    }
}
