using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Lot.Handmade;
using UncannyValet.UI.Sandbox;
using UncannyValet.Valet;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace UncannyValet.Tests.PlayMode
{
    /// <summary>Loads the sandbox scene: it must start without errors, put the valet in a car that drives, and switch worlds with the right mud. Refs #180.</summary>
    public sealed class SandboxSceneTests
    {
        private const string ScenePath = "Assets/_Project/Scenes/Sandbox.unity";
        private const int SettleFrames = 5;
        private const float DriveSeconds = 1.5f;
        private const float MinDistance = 1f;
        private const string VanaheimMudWorld = "Storm-Soaked Harvest Feast";
        private const string HandmadeWorld = "Grandma's Diner (hand-made)";
        private const int FirstHandmadeKey = 5;
        private const float ArrivalTolerance = 1.5f;
        private const float WideAspect = 16f / 9f;

        private readonly List<string> _problems = new List<string>();
        private Keyboard _keyboard;
        private InputSettings.EditorInputBehaviorInPlayMode? _behavior;
        private InputSettings.BackgroundBehavior _background;

        private void OnLog(string message, string stack, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                _problems.Add(type + ": " + message + "\n" + stack);
        }

        [UnityTearDown]
        public IEnumerator RemoveTheScene()
        {
            Application.logMessageReceived -= OnLog;
            if (_keyboard != null) InputSystem.RemoveDevice(_keyboard);
            if (_behavior.HasValue)
            {
                InputSystem.settings.editorInputBehaviorInPlayMode = _behavior.Value;
                InputSystem.settings.backgroundBehavior = _background;
            }
            _keyboard = null;
            _behavior = null;
            var roots = SceneManager.GetActiveScene().GetRootGameObjects();
            for (var i = 0; i < roots.Length; i++) Object.Destroy(roots[i]);
            yield return null;
            Services.Reset();
        }

        [UnityTest]
        public IEnumerator SandboxScene_StartsWithoutErrors_AndSeatsTheValetInACar()
        {
            Application.logMessageReceived += OnLog;
            yield return LoadScene();
            var seat = Object.FindFirstObjectByType<SeatController>();
            Assert.IsNotNull(seat);
            Assert.IsTrue(seat.IsDriving, "the valet starts in the car");
            Assert.IsTrue(Services.Get<ISandboxControl>().WorldCount >= 2, "Midgard and Vanaheim at least");
            Application.logMessageReceived -= OnLog;
            Assert.IsEmpty(_problems, string.Join("\n---\n", _problems));
        }

        [UnityTest]
        public IEnumerator HoldingForward_DrivesTheCar()
        {
            Application.logMessageReceived += OnLog;
            yield return LoadScene();
            var seat = Object.FindFirstObjectByType<SeatController>();
            var start = seat.Car.Transform.position;
            _behavior = InputSystem.settings.editorInputBehaviorInPlayMode;
            _background = InputSystem.settings.backgroundBehavior;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            _keyboard = InputSystem.AddDevice<Keyboard>();
            InputSystem.QueueStateEvent(_keyboard, new KeyboardState(Key.W));
            yield return new WaitForSeconds(DriveSeconds);
            Assert.AreSame(_keyboard, Keyboard.current, "the test keyboard must be the current one");
            Assert.IsTrue(_keyboard.wKey.isPressed, "W is held");
            Assert.Greater(Vector2.Distance(start, seat.Car.Transform.position), MinDistance, "the car did not move under input");
            Application.logMessageReceived -= OnLog;
            Assert.IsEmpty(_problems, string.Join("\n---\n", _problems));
        }

        [UnityTest]
        public IEnumerator SwitchingWorld_BuildsThatWorld_WithItsMud_AndPutsTheValetInANewCar()
        {
            Application.logMessageReceived += OnLog;
            yield return LoadScene();
            var control = Services.Get<ISandboxControl>();
            Assert.IsFalse(Services.Get<IMudPatches>().IsInside(Vector2.zero), "the first world has no mud");
            var mudWorld = IndexOf(control, VanaheimMudWorld);
            Assert.GreaterOrEqual(mudWorld, 0, "the sandbox lists " + VanaheimMudWorld);
            Assert.IsTrue(control.TrySelectWorld(mudWorld));
            for (var i = 0; i < SettleFrames; i++) yield return null;

            Assert.AreEqual(mudWorld, control.CurrentWorld);
            Assert.IsTrue(Object.FindFirstObjectByType<SeatController>().IsDriving, "a fresh car takes the valet");
            Assert.IsTrue(HasAnyMud(Services.Get<IMudPatches>()), "Vanaheim's mud patches appear when it is picked");

            control.ResetCar();
            for (var i = 0; i < SettleFrames; i++) yield return null;
            Assert.IsTrue(Object.FindFirstObjectByType<SeatController>().IsDriving, "Reset puts the valet in a new car");
            Assert.IsTrue(control.TrySelectWorld(0));
            for (var i = 0; i < SettleFrames; i++) yield return null;
            Assert.IsFalse(HasAnyMud(Services.Get<IMudPatches>()), "going back leaves no mud behind");
            Application.logMessageReceived -= OnLog;
            Assert.IsEmpty(_problems, string.Join("\n---\n", _problems));
        }

        [UnityTest]
        public IEnumerator HandmadeWorld_IsSelectable_SpawnsTheCarAtTheArrivalPoint_AndTheCameraShowsTheWholeLot()
        {
            Application.logMessageReceived += OnLog;
            yield return LoadScene();
            var control = Services.Get<ISandboxControl>();
            var index = IndexOf(control, HandmadeWorld);
            Assert.GreaterOrEqual(index + 1, FirstHandmadeKey, "the hand-made world is picker key 5 or later");
            Assert.IsTrue(control.TrySelectWorld(index));
            for (var i = 0; i < SettleFrames; i++) yield return null;

            Assert.AreEqual(index, control.CurrentWorld);
            var lot = Object.FindFirstObjectByType<HandmadeLot>();
            Assert.IsNotNull(lot, "the hand-made lot is on screen");
            Assert.IsTrue(lot.IsBuilt, "it passed the validator and built");
            Assert.AreSame(lot, Services.Get<ILotLayout>(), "the sandbox registered the hand-made lot as the layout");
            var seat = Object.FindFirstObjectByType<SeatController>();
            Assert.IsTrue(seat.IsDriving, "the valet is in the car");
            Assert.Less(Vector2.Distance(seat.Car.Transform.position, lot.ArrivalPoint), ArrivalTolerance, "the car spawns at the arrival point");
            AssertCameraShows(lot.ViewRect);
            Application.logMessageReceived -= OnLog;
            Assert.IsEmpty(_problems, string.Join("\n---\n", _problems));
        }

        private static void AssertCameraShows(Rect view)
        {
            var cam = Camera.main;
            cam.aspect = WideAspect;
            var half = new Vector2(cam.orthographicSize * cam.aspect, cam.orthographicSize);
            var centre = (Vector2)cam.transform.position;
            Assert.LessOrEqual(centre.x - half.x, view.xMin, "left edge of the lot view is on screen");
            Assert.GreaterOrEqual(centre.x + half.x, view.xMax, "right edge of the lot view is on screen");
            Assert.LessOrEqual(centre.y - half.y, view.yMin, "bottom edge of the lot view is on screen");
            Assert.GreaterOrEqual(centre.y + half.y, view.yMax, "top edge of the lot view is on screen");
        }

        private static int IndexOf(ISandboxControl control, string name)
        {
            for (var i = 0; i < control.WorldCount; i++)
                if (control.WorldName(i) == name) return i;
            return -1;
        }

        // The puddles lie in the lot; sweeping the lot's area finds one if the world has any.
        private static bool HasAnyMud(IMudPatches mud)
        {
            for (var x = -15f; x <= 15f; x += 0.5f)
                for (var y = -15f; y <= 15f; y += 0.5f)
                    if (mud.IsInside(new Vector2(x, y))) return true;
            return false;
        }

        private static IEnumerator LoadScene()
        {
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(
                ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            for (var i = 0; i < SettleFrames; i++) yield return null;
        }
    }
}
