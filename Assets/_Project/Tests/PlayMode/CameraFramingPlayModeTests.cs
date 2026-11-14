using System.Collections;
using NUnit.Framework;
using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace UncannyValet.Tests.PlayMode
{
    /// <summary>Issues 123 and 127: at the start of a shift the camera shows the valet, the diner door and every queue slot, on 16:9 and 16:10.</summary>
    public sealed class CameraFramingPlayModeTests
    {
        private const float Margin = 0.5f;
        private const int SettleFrames = 6;

        private static readonly float[] Aspects = { 16f / 9f, 16f / 10f };

        private const string BootstrapPath = "Assets/_Project/Scenes/Bootstrap.unity";
        private const string MidgardCrazyPath = "Assets/_Project/Scenes/Lot_MidgardCrazy.unity";
        private const string VanaheimTrainingPath = "Assets/_Project/Scenes/Lot_VanaheimTraining.unity";
        private const string VanaheimCrazyPath = "Assets/_Project/Scenes/Lot_VanaheimCrazy.unity";

        [UnityTearDown]
        public IEnumerator RemoveTheScene()
        {
            var roots = SceneManager.GetActiveScene().GetRootGameObjects();
            for (var i = 0; i < roots.Length; i++) Object.Destroy(roots[i]);
            yield return null;
            Services.Reset();
        }

        [UnityTest]
        public IEnumerator MidgardTraining_ShowsTheWholePlayArea() => Check(BootstrapPath);

        [UnityTest]
        public IEnumerator MidgardCrazy_ShowsTheWholePlayArea() => Check(MidgardCrazyPath);

        [UnityTest]
        public IEnumerator VanaheimTraining_ShowsTheWholePlayArea() => Check(VanaheimTrainingPath);

        [UnityTest]
        public IEnumerator VanaheimCrazy_ShowsTheWholePlayArea() => Check(VanaheimCrazyPath);

        private static IEnumerator Check(string path)
        {
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(
                path, new LoadSceneParameters(LoadSceneMode.Single));
            for (var i = 0; i < SettleFrames; i++) yield return null;
            var cam = Camera.main;
            Assert.IsNotNull(cam, "no main camera in " + path);
            for (var a = 0; a < Aspects.Length; a++)
            {
                cam.aspect = Aspects[a];
                for (var i = 0; i < SettleFrames; i++) yield return null;
                AssertVisible(cam, path, Aspects[a]);
            }
        }

        private static void AssertVisible(Camera cam, string path, float aspect)
        {
            var half = new Vector2(cam.orthographicSize * cam.aspect, cam.orthographicSize);
            var center = (Vector2)cam.transform.position;
            var view = new Rect(center - half, half * 2f);
            var why = path + " at " + aspect + " view " + view + " size " + cam.orthographicSize + ": ";
            var layout = Services.Get<ILotLayout>();
            Assert.IsTrue(Inside(view, Services.Get<IPlayerFocus>().Position), why + "valet");
            Assert.IsTrue(Inside(view, Services.Get<IPedestrianPaths>().DinerDoor), why + "diner door");
            Assert.IsTrue(Inside(view, layout.ValetStartPoint), why + "valet start");
            for (var i = 0; i < layout.ArrivalSlots.Count; i++)
                Assert.IsTrue(Inside(view, layout.ArrivalSlots[i]), why + "queue slot " + i);
        }

        private static bool Inside(Rect view, Vector2 p) =>
            p.x >= view.xMin + Margin && p.x <= view.xMax - Margin && p.y >= view.yMin + Margin && p.y <= view.yMax - Margin;
    }
}
