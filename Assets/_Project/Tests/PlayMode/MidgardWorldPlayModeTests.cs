using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Core.Watchers;
using UncannyValet.Lot;
using UncannyValet.Pedestrians;
using UncannyValet.Traffic;
using UncannyValet.UI;
using UncannyValet.Valet;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace UncannyValet.Tests.PlayMode
{
    /// <summary>The two Midgard lot scenes as built: eight bays and no stations, the traffic and crowd of each lot, the Ferris wheel and the floating gag lines.</summary>
    public sealed class MidgardWorldPlayModeTests
    {
        private const string TrainingPath = "Assets/_Project/Scenes/Bootstrap.unity";
        private const string CrazyPath = "Assets/_Project/Scenes/Lot_MidgardCrazy.unity";
        private const float SampleSeconds = 0.5f;
        private const int Samples = 8;
        private const float GagSeconds = 16f;
        private const float WheelRunSeconds = 6f;
        private const float WheelApproach = 14f;
        private const float WheelSpeed = 10f;
        private const string CrazyLotPath = "Assets/_Project/Data/Run/MidgardCrazyLot.asset";

        private readonly List<string> _problems = new List<string>();

        private void OnLog(string message, string stack, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) _problems.Add(type + ": " + message);
        }

        [UnityTearDown]
        public IEnumerator RemoveTheScene()
        {
            Application.logMessageReceived -= OnLog;
            var roots = SceneManager.GetActiveScene().GetRootGameObjects();
            for (var i = 0; i < roots.Length; i++) Object.Destroy(roots[i]);
            var host = Object.FindFirstObjectByType<UncannyValet.Run.RunHost>();
            if (host != null) Object.Destroy(host.gameObject);
            yield return null;
            Services.Reset();
        }

        [UnityTest]
        public IEnumerator TrainingLot_HasEightBays_NoStations_NoAmbientCars_AndThreeWalkers()
        {
            yield return Play(TrainingPath);
            AssertBaysAndNoStations();
            Assert.AreEqual(3f, Services.Get<ITuning>().Get(TuningKey.PedestrianCount));
            var most = 0;
            var cars = 0;
            for (var i = 0; i < Samples; i++)
            {
                yield return new WaitForSeconds(SampleSeconds);
                most = Mathf.Max(most, VisibleAmbientWalkers());
                cars = Mathf.Max(cars, ActiveAmbientCars());
            }
            Assert.AreEqual(0, cars, "the training lot has no ambient road cars");
            Assert.LessOrEqual(most, 3, "no more than three walkers");
            Assert.GreaterOrEqual(most, 1, "the walkers are out");
            Assert.IsEmpty(_problems, string.Join("\n", _problems));
        }

        [UnityTest]
        public IEnumerator CrazyLot_HasEightBays_NoStations_TodaysTraffic_AndATenWalkerCrowd()
        {
            yield return Play(CrazyPath);
            AssertBaysAndNoStations();
            Assert.AreEqual(10f, Services.Get<ITuning>().Get(TuningKey.PedestrianCount));
            Assert.GreaterOrEqual(Object.FindFirstObjectByType<PedestrianCrowd>().Count, 10, "the pool holds ten");
            var most = 0;
            var cars = 0;
            for (var i = 0; i < Samples; i++)
            {
                yield return new WaitForSeconds(SampleSeconds);
                most = Mathf.Max(most, VisibleAmbientWalkers());
                cars = Mathf.Max(cars, ActiveAmbientCars());
            }
            Assert.Greater(cars, 0, "the fair night has road traffic");
            Assert.GreaterOrEqual(most, 6, "a crowd streams through the lot");
            Assert.IsEmpty(_problems, string.Join("\n", _problems));
        }

        [UnityTest]
        public IEnumerator TheFerrisWheel_IsSolid_AndDamagesACarThatHitsIt()
        {
            yield return Play(CrazyPath);
            var wheel = GameObject.Find("MidgardFerrisWheel");
            Assert.IsNotNull(wheel, "the wheel is in the crazy lot");
            Assert.IsNotNull(wheel.GetComponent<BoxCollider2D>(), "the wheel is solid");
            Assert.IsNull(GameObject.Find("MidgardStallRed")?.GetComponent<Collider2D>(), "stalls are decor");
            var damaged = new List<CarDamaged>();
            Services.Get<IEventBus>().Subscribe<CarDamaged>(damaged.Add);
            var at = (Vector2)wheel.transform.position;
            var car = Services.Get<ICarFactory>().Spawn(new CarId(900), at + Vector2.left * WheelApproach, -90f);
            var body = car.Transform.GetComponent<Rigidbody2D>(); // not seated: a seated car would be steered by the valet's keyboard
            for (var t = 0f; t < WheelRunSeconds; t += Time.fixedDeltaTime)
            {
                yield return new WaitForFixedUpdate();
                body.linearVelocity = Vector2.right * WheelSpeed;
            }
            Services.Get<IEventBus>().Unsubscribe<CarDamaged>(damaged.Add);
            Assert.IsTrue(damaged.Exists(d => d.Car == car.Id && d.Damage01 > 0f), "the car took damage from the wheel");
        }

        [UnityTest]
        public IEnumerator Pedestrians_PopGagLines_WhenTheValetPassesClose_WithTheLockouts()
        {
            yield return Play(CrazyPath);
            var asset = CrazyGagLines();
            var requests = new List<(float time, SpeechRequest request)>();
            Services.Get<IEventBus>().Subscribe<SpeechRequested>(e => requests.Add((Time.unscaledTime, e.Request)));
            var valet = Object.FindFirstObjectByType<KeyboardInputSource>().transform;
            for (var t = 0f; t < GagSeconds; t += SampleSeconds)
            {
                var walker = FirstVisibleWalker();
                if (walker != null) valet.position = walker.position + Vector3.right * 0.5f;
                yield return new WaitForSecondsRealtime(SampleSeconds);
            }
            var gags = requests.FindAll(r => r.request.Priority == SpeechPriority.GagLine);
            Assert.GreaterOrEqual(gags.Count, 1, "a walker popped a line");
            foreach (var gag in gags)
            {
                Assert.AreEqual("FAIRGOER", gag.request.Speaker);
                Assert.IsTrue(new List<string>(asset.Lines).Contains(gag.request.Text), gag.request.Text);
                Assert.IsNotNull(gag.request.Anchor, "the bubble follows the walker");
            }
            for (var i = 1; i < gags.Count; i++) Assert.GreaterOrEqual(gags[i].time - gags[i - 1].time, asset.GlobalLockoutSeconds - 0.1f);
            Assert.IsEmpty(_problems, string.Join("\n", _problems));
        }

        private static GagLineAsset CrazyGagLines()
            => UnityEditor.AssetDatabase.LoadAssetAtPath<UncannyValet.Run.LotDefinition>(CrazyLotPath).GagLines;

        private IEnumerator Play(string path)
        {
            Application.logMessageReceived += OnLog;
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(path, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            yield return null;
            var ui = Object.FindFirstObjectByType<GameUi>();
            typeof(GameUi).GetMethod("StartPlaying", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(ui, null);
            yield return null;
        }

        private static void AssertBaysAndNoStations()
        {
            var layout = Services.Get<ILotLayout>();
            Assert.AreEqual(8, layout.RegularSpotCount);
            Assert.AreEqual(0, layout.ChargingSpotCount);
            Assert.IsFalse(layout.HasWashStation);
            Assert.AreEqual(8, layout.SpotIds.Count, "all eight bays are open");
            Assert.AreEqual(0, Object.FindObjectsByType<StationZone>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length, "no wash slot");
        }

        private static IEnumerable<Transform> Walkers()
        {
            var crowd = Object.FindFirstObjectByType<PedestrianCrowd>().transform;
            for (var i = 0; i < crowd.childCount; i++)
                if (crowd.GetChild(i).name.StartsWith("Pedestrian") && crowd.GetChild(i).gameObject.activeInHierarchy) yield return crowd.GetChild(i);
        }

        private static int VisibleAmbientWalkers()
        {
            var count = 0;
            foreach (var _ in Walkers()) count++;
            return count;
        }

        private static Transform FirstVisibleWalker()
        {
            foreach (var walker in Walkers()) return walker;
            return null;
        }

        private static int ActiveAmbientCars()
        {
            var count = 0;
            foreach (var car in Object.FindObjectsByType<TrafficCar>(FindObjectsSortMode.None))
                if (car.gameObject.activeInHierarchy) count++;
            return count;
        }
    }
}
