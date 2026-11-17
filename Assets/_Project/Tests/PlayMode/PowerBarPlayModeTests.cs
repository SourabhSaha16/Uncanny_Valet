using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Powers;
using UncannyValet.Run;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace UncannyValet.Tests.PlayMode
{
    /// <summary>The real lot scene: with Freeze time owned (the F2 developer row), a full bucket and key 1, the world freezes.</summary>
    public sealed class PowerBarPlayModeTests
    {
        private const string ScenePath = "Assets/_Project/Scenes/Bootstrap.unity";

        private sealed class PressedKey : IPowerInput
        {
            public bool Pressed;
            public bool PowerPressed(int slot) => Pressed && slot == 0;
        }

        [UnityTearDown]
        public IEnumerator RemoveTheScene()
        {
            var roots = SceneManager.GetActiveScene().GetRootGameObjects();
            for (var i = 0; i < roots.Length; i++) Object.Destroy(roots[i]);
            var host = Object.FindFirstObjectByType<RunHost>();
            if (host != null) Object.Destroy(host.gameObject);
            yield return null;
            Services.Reset();
        }

        private static IEnumerator Load()
        {
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(
                ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            yield return null;
        }

        private static PressedKey Hook(out PowerBarController bar)
        {
            var key = new PressedKey();
            var activation = Object.FindFirstObjectByType<PowerActivationController>();
            typeof(PowerActivationController).GetField("_input", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(activation, key);
            bar = Object.FindFirstObjectByType<PowerBarController>();
            return key;
        }

        [UnityTest]
        public IEnumerator FreezeTime_OwnedWithAFullBucket_FreezesTheWorldOnKeyOne()
        {
            yield return Load();
            var key = Hook(out var bar);
            var bus = Services.Get<IEventBus>();
            var world = Services.Get<IWorldTime>();
            Assert.IsFalse(bar.IsActive, "no power owned at the start");

            Services.Get<ITuning>().Set(TuningKey.DevOwnFreezeTime, 1f);
            Assert.IsTrue(bar.IsActive);
            bus.Publish(new PerfectPark(new CarId(1), TicketId.None));
            key.Pressed = true;
            yield return null;
            key.Pressed = false;
            Assert.AreEqual(1f, world.Scale, "one Perfect Park is not a full bucket");

            bus.Publish(new PerfectPark(new CarId(2), TicketId.None));
            yield return null;
            Assert.AreEqual(1f, world.Scale);
            key.Pressed = true;
            yield return null;
            key.Pressed = false;
            Assert.AreEqual(0f, world.Scale, "freeze time runs");
            Assert.AreEqual(36f - PowerBar.BucketSize, bar.Bar.Value, 0.01f);

            Services.Get<ITuning>().Set(TuningKey.DevOwnFreezeTime, 0f);
            Assert.IsFalse(Services.Get<IRunProgress>().EarnedPowers.Count > 0);
        }

        [UnityTest]
        public IEnumerator Freeze_RunsAtItsUpgradedLevel_ThenCoolsDown_AndAPressWhileCoolingCostsNothing()
        {
            yield return Load();
            var key = Hook(out var bar);
            var bus = Services.Get<IEventBus>();
            var tuning = Services.Get<ITuning>();
            var view = Services.Get<IPowerView>();
            tuning.Set(TuningKey.PowerFreezeTimeL2Duration, 0.3f);
            tuning.Set(TuningKey.PowerFreezeTimeL2Cooldown, 30f);
            tuning.Set(TuningKey.DevOwnFreezeTime, 2f);
            Assert.IsTrue(view.TryGetState(StoryPower.FreezeTime, out var state));
            Assert.AreEqual(2, state.Level, "the upgrade level comes from the run progress");
            Assert.AreEqual(PowerPhase.Ready, state.Phase);
            for (var i = 0; i < 4; i++) bus.Publish(new PerfectPark(new CarId(i + 1), TicketId.None));

            key.Pressed = true;
            yield return null;
            key.Pressed = false;
            view.TryGetState(StoryPower.FreezeTime, out state);
            Assert.AreEqual(PowerPhase.Active, state.Phase);
            Assert.AreEqual(0f, Services.Get<IWorldTime>().Scale);
            yield return new WaitForSecondsRealtime(0.6f);
            view.TryGetState(StoryPower.FreezeTime, out state);
            Assert.AreEqual(PowerPhase.Cooling, state.Phase, "the cooldown runs after the use ends");
            Assert.AreEqual(1f, Services.Get<IWorldTime>().Scale);

            for (var i = 0; i < 4; i++) bus.Publish(new PerfectPark(new CarId(10 + i), TicketId.None));
            var value = bar.Bar.Value;
            key.Pressed = true;
            yield return null;
            key.Pressed = false;
            Assert.AreEqual(value, bar.Bar.Value, 0.001f, "no bucket is spent while the power cools down");
            Assert.AreEqual(1f, Services.Get<IWorldTime>().Scale);
        }

        [UnityTest]
        public IEnumerator WithNoPowerOwned_TheKeyDoesNothing()
        {
            yield return Load();
            var key = Hook(out var bar);
            Services.Get<IEventBus>().Publish(new PerfectPark(new CarId(1), TicketId.None));
            key.Pressed = true;
            yield return null;
            key.Pressed = false;
            Assert.AreEqual(0f, bar.Bar.Value);
            Assert.AreEqual(1f, Services.Get<IWorldTime>().Scale);
        }
    }
}
