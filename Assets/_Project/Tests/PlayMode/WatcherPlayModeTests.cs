#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Core.Watchers;
using UncannyValet.Pedestrians;
using UncannyValet.Run;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace UncannyValet.Tests.PlayMode
{
    /// <summary>
    /// The watchers in the real lot scenes: each stands at its lot's spot, is scenery (no collider, no body), and speaks in a bubble after a
    /// triggering event. Vanaheim has no lot scene yet, so its two characters are built from their real definitions.
    /// </summary>
    public sealed class WatcherPlayModeTests
    {
        private const string TrainingScene = "Assets/_Project/Scenes/Bootstrap.unity";
        private const string CrazyScene = "Assets/_Project/Scenes/Lot_MidgardCrazy.unity";
        private const string CrazyLot = "Assets/_Project/Data/Run/MidgardCrazyLot.asset";
        private const string TrainingLot = "Assets/_Project/Data/Run/MidgardTrainingLot.asset";
        private const string Folder = "Assets/_Project/Data/Watchers/";
        private const string PedestrianPath = "Assets/_Project/Data/Pedestrians/PedestrianConfig.asset";
        private const float WaitSeconds = 2f;

        private GameObject _extra;

        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            Object.Destroy(_extra);
            var roots = SceneManager.GetActiveScene().GetRootGameObjects();
            for (var i = 0; i < roots.Length; i++) Object.Destroy(roots[i]);
            var host = Object.FindFirstObjectByType<RunHost>();
            if (host != null) Object.Destroy(host.gameObject);
            yield return null;
            Services.Reset();
        }

        private static IEnumerator Load(string path)
        {
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(path, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            yield return null;
        }

        private IEnumerator CheckLot(string scene, string lotPath, string speaker, WatcherKind kind)
        {
            yield return Load(scene);
            var body = Object.FindFirstObjectByType<WatcherBody>();
            Assert.IsNotNull(body, "the lot has a watcher");
            var lot = AssetDatabase.LoadAssetAtPath<LotDefinition>(lotPath);
            Assert.Less(Vector2.Distance(body.transform.position, lot.WatcherSpot), 0.01f, "the watcher stands at its lot's spot");
            Assert.IsTrue(body.IsBuilt, "the watcher was drawn");
            Assert.AreEqual(0, body.GetComponentsInChildren<Collider2D>().Length, "a watcher is not solid");
            Assert.IsNull(body.GetComponent<Rigidbody2D>(), "a watcher has no body to hit");
            Assert.Greater(body.GetComponentsInChildren<SpriteRenderer>().Length, 4, "layers and accessories");

            Assert.IsTrue(Services.Get<IWatcherSource>().TryGet(out var watcher));
            Assert.AreEqual(speaker, watcher.SpeakerName);
            Assert.AreEqual(kind, watcher.Kind);

            var bus = Services.Get<IEventBus>();
            var shown = default(SpeechShown);
            var count = 0;
            bus.Subscribe<SpeechShown>(e => { shown = e; count++; });
            bus.Publish(new PerfectPark(new CarId(1), new TicketId(1)));
            var until = Time.unscaledTime + WaitSeconds;
            while (count == 0 && Time.unscaledTime < until) yield return null;

            Assert.AreEqual(1, count, "one bubble after the event");
            Assert.AreEqual(speaker, shown.Request.Speaker);
            Assert.AreEqual(SpeechPriority.Comment, shown.Request.Priority);
            Assert.AreEqual((Vector2)body.transform.position, shown.Request.CurrentPosition, "the bubble is above the watcher");
        }

        [UnityTest]
        public IEnumerator MidgardCrazy_MrsOkaforWatches() => CheckLot(CrazyScene, CrazyLot, "MRS. OKAFOR", WatcherKind.God);

        [UnityTest]
        public IEnumerator MidgardTraining_GrandmaWatches() => CheckLot(TrainingScene, TrainingLot, "GRANDMA", WatcherKind.Trainer);

        [UnityTest]
        public IEnumerator ASecondBubbleWithinTheLockout_IsDropped()
        {
            yield return Load(CrazyScene);
            var bus = Services.Get<IEventBus>();
            var count = 0;
            bus.Subscribe<SpeechShown>(e => count++);
            bus.Publish(new PerfectPark(new CarId(1), new TicketId(1)));
            yield return new WaitForSeconds(0.3f);
            bus.Publish(new PerfectDropOff(new CarId(1), new TicketId(1)));
            yield return new WaitForSeconds(0.3f);
            Assert.AreEqual(1, count, "at most one bubble every 8 seconds");
        }

        [UnityTest]
        public IEnumerator VanaheimCharacters_AreDrawnAndNotSolid()
        {
            var config = AssetDatabase.LoadAssetAtPath<PedestrianConfig>(PedestrianPath);
            foreach (var asset in new[] { "Freyja", "VanirFarmhand" })
            {
                var definition = AssetDatabase.LoadAssetAtPath<WatcherDefinition>(Folder + asset + ".asset");
                Assert.AreEqual(World.Vanaheim, definition.World);
                _extra = new GameObject(asset);
                var body = _extra.AddComponent<WatcherBody>();
                body.Build(definition.Look, config);
                yield return null;
                Assert.IsTrue(body.IsBuilt, asset);
                Assert.AreEqual(0, body.GetComponentsInChildren<Collider2D>().Length, asset);
                Object.Destroy(_extra);
                yield return null;
            }
        }
    }
}
#endif
