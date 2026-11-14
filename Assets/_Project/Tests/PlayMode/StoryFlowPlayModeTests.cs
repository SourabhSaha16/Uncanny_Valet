using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UncannyValet.Bootstrap;
using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Run;
using UncannyValet.UI;
using UncannyValet.UI.Flow;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace UncannyValet.Tests.PlayMode
{
    /// <summary>
    /// Plays the story flow through the real scenes (issue 84): title, how-to-play, Midgard training intro, the shift, results, the next lot's
    /// intro in its own scene, a reward card and a restart. Presses go to the flow controller (the keyboard itself is not simulated); the lot
    /// outcome is published on the event bus, as the economy does at shift end.
    /// </summary>
    public sealed class StoryFlowPlayModeTests
    {
        private const string BootstrapPath = "Assets/_Project/Scenes/Bootstrap.unity";
        private const float CardWait = 0.5f;
        private const float ResultsWait = 1f;
        private const int LoadFrames = 12;
        private static readonly LotId MT = new LotId(World.Midgard, LotKind.Training);
        private static readonly LotId MC = new LotId(World.Midgard, LotKind.Crazy);
        private static readonly LotId VT = new LotId(World.Vanaheim, LotKind.Training);

        private readonly List<string> _problems = new List<string>();

        private void OnLog(string message, string stack, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) _problems.Add(type + ": " + message + "\n" + stack);
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

        private static FlowController Controller() =>
            (FlowController)typeof(GameUi).GetField("_flow", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Object.FindFirstObjectByType<GameUi>());

        private static StoryCard Card() =>
            ((StoryCardCache)typeof(GameUi).GetField("_cards", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Object.FindFirstObjectByType<GameUi>())).Current;

        // Waits out the key delay, then presses any key (the frame number differs from the one the screen appeared in).
        private static IEnumerator Press(float wait = CardWait)
        {
            yield return new WaitForSecondsRealtime(wait);
            Controller().Update(Time.unscaledTime, Time.frameCount + 1, true, true, false);
        }

        private static IEnumerator Frames(int count)
        {
            for (var i = 0; i < count; i++) yield return null;
        }

        private static void EndShift(bool cleared, LotId lot)
        {
            var bus = Services.Get<IEventBus>();
            bus.Publish(new ShiftResultReady(new ShiftResult(cleared ? 400 : 5, 150, cleared, 0, 1)));
            if (cleared) bus.Publish(new LotCleared(lot)); else bus.Publish(new LotFailed(lot));
        }

        [UnityTest]
        public IEnumerator TitleToHowToPlayToIntroToShiftToResultsToTheNextLot()
        {
            Application.logMessageReceived += OnLog;
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(BootstrapPath, new LoadSceneParameters(LoadSceneMode.Single));
            yield return Frames(2);
            var machine = Object.FindFirstObjectByType<FlowHost>().Machine;
            Assert.AreEqual(FlowState.Title, machine.State);

            yield return Press();
            Assert.AreEqual(FlowState.HowToPlay, machine.State);
            yield return Press();
            Assert.AreEqual(FlowState.LotIntro, machine.State);
            Assert.AreEqual(MT, machine.IntroLot);
            yield return Frames(2);
            Assert.Greater(Card().Lines.Count, 0, "the intro card was built");
            yield return Press();
            Assert.AreEqual(FlowState.Playing, machine.State);
            yield return Frames(2);

            EndShift(true, MT);
            Assert.AreEqual(FlowState.Results, machine.State);
            Assert.AreEqual(MC, machine.NextLot);
            yield return Press(ResultsWait);
            yield return Frames(LoadFrames);
            Assert.AreEqual("Lot_MidgardCrazy", SceneManager.GetActiveScene().name);
            Assert.AreSame(machine, Object.FindFirstObjectByType<FlowHost>().Machine, "the flow state survives the scene load");
            Assert.AreEqual(FlowState.LotIntro, machine.State);
            Assert.AreEqual(MC, machine.IntroLot);
            yield return Press();
            Assert.AreEqual(FlowState.Playing, machine.State);
            Application.logMessageReceived -= OnLog;
            Assert.IsEmpty(_problems, string.Join("\n---\n", _problems));
        }

        [UnityTest]
        public IEnumerator ClearingTheCrazyLot_ShowsTheRewardCard_ThenTheNextWorldsIntro_AndAFailedTrainingLotRestartsItself()
        {
            Application.logMessageReceived += OnLog;
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/_Project/Scenes/Lot_MidgardCrazy.unity", new LoadSceneParameters(LoadSceneMode.Single));
            yield return Frames(2);
            var machine = Object.FindFirstObjectByType<FlowHost>().Machine;
            Controller().DevStart(MC);
            yield return Frames(LoadFrames);
            Assert.AreEqual(FlowState.LotIntro, machine.State);
            yield return Press();
            Assert.AreEqual(FlowState.Playing, machine.State);
            yield return Frames(2);

            EndShift(true, MC);
            yield return Press(ResultsWait);
            Assert.AreEqual(FlowState.Reward, machine.State);
            Assert.AreEqual(StoryPower.ThunderStride, machine.Reward.Power);
            yield return Frames(2);
            Assert.IsTrue(Card().HasIcon);
            yield return Press();
            yield return Frames(LoadFrames);
            Assert.AreEqual("Lot_VanaheimTraining", SceneManager.GetActiveScene().name);
            Assert.AreEqual(VT, machine.IntroLot);
            yield return Press();
            yield return Frames(2);

            EndShift(false, VT);
            yield return Press(ResultsWait);
            yield return Frames(LoadFrames);
            Assert.AreEqual(FlowState.LotIntro, machine.State);
            Assert.AreEqual(VT, machine.IntroLot, "a failed training lot restarts through its own intro");
            Application.logMessageReceived -= OnLog;
            Assert.IsEmpty(_problems, string.Join("\n---\n", _problems));
        }
    }
}
